﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;
using LinqToDB.Data.DbCommandProcessor;
using LinqToDB.DataProvider.Informix;
using LinqToDB.Expressions;
using LinqToDB.Extensions;
using LinqToDB.Linq;
using LinqToDB.Mapping;
using LinqToDB.Reflection;
using LinqToDB.Tools;
using LinqToDB.Tools.Comparers;

#if NET472
using System.ServiceModel;
using System.ServiceModel.Description;
using LinqToDB.ServiceModel;
#endif

using NUnit.Framework;
using NUnit.Framework.Internal;
using TypeHelper = LinqToDB.Common.TypeHelper;

namespace Tests
{
	using Model;
	using Tools;

	public partial class TestBase
	{
		protected static class TestData
		{
			// offset 40 is not used by any timezone, so we can detect tz handling issues, which could be hidden when offset match current TZ
			public static readonly DateTimeOffset DateTimeOffset          = new DateTimeOffset(2020, 2, 29, 17, 54, 55, 123, TimeSpan.FromMinutes(40)).AddTicks(1234);
			public static readonly DateTimeOffset DateTimeOffsetUtc       = new DateTimeOffset(2020, 2, 29, 17, 9, 55, 123, TimeSpan.Zero).AddTicks(1234);
			public static readonly DateTime DateTime                      = new DateTime(2020, 2, 29, 17, 54, 55, 123).AddTicks(1234);
			public static readonly DateTime DateTimeUtc                   = new DateTime(2020, 2, 29, 17, 54, 55, 123, DateTimeKind.Utc).AddTicks(1234);
			public static readonly DateTime DateTime4Utc                  = new DateTime(2020, 2, 29, 17, 54, 55, 123, DateTimeKind.Utc).AddTicks(1000);
			public static readonly DateTime Date                          = new (2020, 2, 29);
			public static readonly TimeSpan TimeOfDay                     = new TimeSpan(0, 17, 54, 55, 123).Add(TimeSpan.FromTicks(1234));
			public static readonly TimeSpan TimeOfDay4                    = new TimeSpan(0, 17, 54, 55, 123).Add(TimeSpan.FromTicks(1000));
			public static readonly Guid     Guid1                         = new ("bc7b663d-0fde-4327-8f92-5d8cc3a11d11");
			public static readonly Guid     Guid2                         = new ("a948600d-de21-4f74-8ac2-9516b287076e");
			public static readonly Guid     Guid3                         = new ("bd3973a5-4323-4dd8-9f4f-df9f93e2a627");

			public static byte[] Binary(int size)
			{
				var value = new byte[size];
				for (var i = 0; i < value.Length; i++)
					value[i] = (byte)(i % 256);

				return value;
			}

			public static Guid SequentialGuid(int n)  => new ($"233bf399-9710-4e79-873d-2ec7bf1e{n:x4}");
		}

		private const int TRACES_LIMIT = 50000;

		private static string? _baselinesPath;

		static TestBase()
		{
			TestContext.WriteLine("Tests started in {0}...", Environment.CurrentDirectory);

			TestContext.WriteLine("CLR Version: {0}...", Environment.Version);

			var traceCount = 0;

			DataConnection.TurnTraceSwitchOn();
			DataConnection.WriteTraceLine = (message, name, level) =>
			{
				var ctx   = CustomTestContext.Get();

				if (ctx.Get<bool>(CustomTestContext.BASELINE_DISABLED) != true)
				{
					if (message?.StartsWith("BeforeExecute") == true)
					{
						var baseline = ctx.Get<StringBuilder>(CustomTestContext.BASELINE);
						if (baseline == null)
						{
							baseline = new StringBuilder();
							ctx.Set(CustomTestContext.BASELINE, baseline);
						}
						baseline.AppendLine(message);
					}
				}

				if (ctx.Get<bool>(CustomTestContext.TRACE_DISABLED) != true)
				{
					var trace = ctx.Get<StringBuilder>(CustomTestContext.TRACE);
					if (trace == null)
					{
						trace = new StringBuilder();
						ctx.Set(CustomTestContext.TRACE, trace);
					}

					trace.AppendLine($"{name}: {message}");

					if (traceCount < TRACES_LIMIT || level == TraceLevel.Error)
					{
						ctx.Set(CustomTestContext.LIMITED, true);
						TestContext.WriteLine("{0}: {1}", name, message);
						Debug.WriteLine(message, name);
					}

					traceCount++;
				}
			};

			// Configuration.RetryPolicy.Factory = db => new Retry();

			Configuration.Linq.TraceMapperExpression = false;
			// Configuration.Linq.GenerateExpressionTest  = true;
			var assemblyPath = typeof(TestBase).Assembly.GetPath()!;

#if NET472
			try
			{
				SqlServerTypes.Utilities.LoadNativeAssemblies(assemblyPath);
			}
			catch // this can fail during tests discovering with NUnitTestAdapter
			{
				// ignore
			}
#endif

			Environment.CurrentDirectory = assemblyPath;

			TestExternals.Log($"CurrentDirectory          : {Environment.CurrentDirectory}");

			var dataProvidersJsonFile     = GetFilePath(assemblyPath, @"DataProviders.json")!;
			var userDataProvidersJsonFile = GetFilePath(assemblyPath, @"UserDataProviders.json")!;

			TestExternals.Log($"dataProvidersJsonFile     : {dataProvidersJsonFile}");
			TestExternals.Log($"userDataProvidersJsonFile : {userDataProvidersJsonFile}");

			var dataProvidersJson     = File.ReadAllText(dataProvidersJsonFile);
			var userDataProvidersJson =
				File.Exists(userDataProvidersJsonFile) ? File.ReadAllText(userDataProvidersJsonFile) : null;

#if NETCOREAPP2_1
			var configName = "CORE21";
#elif NETCOREAPP3_1
			var configName = "CORE31";
#elif NET5_0
			var configName = "NET50";
#elif NET472
			var configName = "NET472";
#else
			var configName = "";
#error Unknown framework
#endif

#if AZURE
			TestContext.WriteLine("Azure configuration detected.");
			configName += ".Azure";
#endif
			var testSettings = SettingsReader.Deserialize(configName, dataProvidersJson, userDataProvidersJson);

			CopyDatabases();

			UserProviders  = new HashSet<string>(testSettings.Providers ?? Array<string>.Empty, StringComparer.OrdinalIgnoreCase);
			SkipCategories = new HashSet<string>(testSettings.Skip      ?? Array<string>.Empty, StringComparer.OrdinalIgnoreCase);

			var logLevel = testSettings.TraceLevel;
			var traceLevel = TraceLevel.Info;

			if (!string.IsNullOrEmpty(logLevel))
				if (!Enum.TryParse(logLevel, true, out traceLevel))
					traceLevel = TraceLevel.Info;

			if (!string.IsNullOrEmpty(testSettings.NoLinqService))
				DataSourcesBaseAttribute.NoLinqService = ConvertTo<bool>.From(testSettings.NoLinqService);

			DataConnection.TurnTraceSwitchOn(traceLevel);

			TestContext.WriteLine("Connection strings:");
			TestExternals.Log("Connection strings:");

#if !NET472
			DataConnection.DefaultSettings            = TxtSettings.Instance;
			TxtSettings.Instance.DefaultConfiguration = "SQLiteMs";

			foreach (var provider in testSettings.Connections/*.Where(c => UserProviders.Contains(c.Key))*/)
			{
				if (string.IsNullOrWhiteSpace(provider.Value.ConnectionString))
					throw new InvalidOperationException("ConnectionString should be provided");

				TestContext.WriteLine($"\tName=\"{provider.Key}\", Provider=\"{provider.Value.Provider}\", ConnectionString=\"{provider.Value.ConnectionString}\"");

				TxtSettings.Instance.AddConnectionString(
					provider.Key, provider.Value.Provider ?? "", provider.Value.ConnectionString);
			}
#else
			foreach (var provider in testSettings.Connections)
			{
				var str = $"\tName=\"{provider.Key}\", Provider=\"{provider.Value.Provider}\", ConnectionString=\"{provider.Value.ConnectionString}\"";

				TestContext.WriteLine(str);
				TestExternals.Log(str);

				DataConnection.AddOrSetConfiguration(
					provider.Key,
					provider.Value.ConnectionString,
					provider.Value.Provider ?? "");
			}
#endif

			TestContext.WriteLine("Providers:");
			TestExternals.Log("Providers:");

			foreach (var userProvider in UserProviders)
			{
				TestContext.WriteLine($"\t{userProvider}");
				TestExternals.Log($"\t{userProvider}");
			}

			DefaultProvider = testSettings.DefaultConfiguration;

			if (!DefaultProvider.IsNullOrEmpty())
			{
				DataConnection.DefaultConfiguration = DefaultProvider;
#if !NET472
				TxtSettings.Instance.DefaultConfiguration = DefaultProvider;
#endif
			}

#if NET472
			LinqService.TypeResolver = str =>
			{
				return str switch
				{
					"Tests.Model.Gender" => typeof(Gender),
					"Tests.Model.Person" => typeof(Person),
					_ => null,
				};
			};
#endif

			// baselines
			if (!string.IsNullOrWhiteSpace(testSettings.BaselinesPath))
			{
				var baselinesPath = Path.GetFullPath(testSettings.BaselinesPath);
				if (Directory.Exists(baselinesPath))
					_baselinesPath = baselinesPath;
			}
		}

		static void CopyDatabases()
		{
			var databasePath = Path.GetFullPath("Database");
			var dataPath     = Path.Combine(databasePath, "Data");

			TestExternals.Log($"Copy databases {databasePath} => {dataPath}");

			if (TestExternals.IsParallelRun && TestExternals.Configuration != null)
			{
				try
				{
					foreach (var file in Directory.GetFiles(databasePath, "*.*"))
					{
						var fileName = Path.GetFileName(file);

						switch (TestExternals.Configuration, fileName)
						{
							case ("Access.Data",         "TestData.mdb")       :
							case ("SqlCe.Data",          "TestData.sdf")       :
							case ("SQLite.Classic.Data", "TestData.sqlite")    :
							case ("SQLite.MS.Data",      "TestData.MS.sqlite") :
							{
								var destination = Path.Combine(dataPath, Path.GetFileName(file));

								TestExternals.Log($"{file} => {destination}");
								File.Copy(file, destination, true);

								break;
							}
						}
					}
				}
				catch (Exception e)
				{
					TestExternals.Log(e.ToString());
					TestContext.WriteLine(e);
					throw;
				}
			}
			else
			{
				if (Directory.Exists(dataPath))
					Directory.Delete(dataPath, true);

				Directory.CreateDirectory(dataPath);

				foreach (var file in Directory.GetFiles(databasePath, "*.*"))
				{
					var destination = Path.Combine(dataPath, Path.GetFileName(file));
					TestContext.WriteLine("{0} => {1}", file, destination);
					File.Copy(file, destination, true);
				}
			}
		}

		protected static string? GetFilePath(string basePath, string findFileName)
		{
			var fileName = Path.GetFullPath(Path.Combine(basePath, findFileName));

			string? path = basePath;
			while (!File.Exists(fileName))
			{
				TestContext.WriteLine($"File not found: {fileName}");

				path = Path.GetDirectoryName(path);

				if (path == null)
					return null;

				fileName = Path.GetFullPath(Path.Combine(path, findFileName));
			}

			TestContext.WriteLine($"Base path found: {fileName}");

			return fileName;
		}

#if NET472
		const           int          IP = 22654;
		static          bool         _isHostOpen;
		static          LinqService? _service;
		static readonly object       _syncRoot = new ();
#endif

		static void OpenHost(MappingSchema? ms)
		{
#if NET472
			if (_isHostOpen)
			{
				_service!.MappingSchema = ms;
				return;
			}

			ServiceHost host;

			lock (_syncRoot)
			{
				if (_isHostOpen)
				{
					_service!.MappingSchema = ms;
					return;
				}

				host        = new ServiceHost(_service = new LinqService(ms) { AllowUpdates = true }, new Uri("net.tcp://localhost:" + (IP + TestExternals.RunID)));
				_isHostOpen = true;
			}

			host.Description.Behaviors.Add(new ServiceMetadataBehavior());
			host.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
			host.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), "mex");
			host.AddServiceEndpoint(
				typeof(ILinqService),
				new NetTcpBinding(SecurityMode.None)
				{
					MaxReceivedMessageSize = 10000000,
					MaxBufferPoolSize      = 10000000,
					MaxBufferSize          = 10000000,
					CloseTimeout           = new TimeSpan(00, 01, 00),
					OpenTimeout            = new TimeSpan(00, 01, 00),
					ReceiveTimeout         = new TimeSpan(00, 10, 00),
					SendTimeout            = new TimeSpan(00, 10, 00),
				},
				"LinqOverWCF");

			host.Open();

			TestExternals.Log($"host opened, Address : {host.BaseAddresses[0]}");
#endif
		}

		public static readonly HashSet<string> UserProviders;
		public static readonly string?         DefaultProvider;
		public static readonly HashSet<string> SkipCategories;

		public static readonly List<string> Providers = CustomizationSupport.Interceptor.GetSupportedProviders(new List<string>
		{
#if NET472
			ProviderName.Sybase,
			ProviderName.OracleNative,
			TestProvName.Oracle11Native,
			ProviderName.Informix,
#endif
			ProviderName.SqlCe,
			ProviderName.Access,
			ProviderName.AccessOdbc,
			ProviderName.DB2,
			ProviderName.InformixDB2,
			ProviderName.SQLiteClassic,
			TestProvName.SQLiteClassicMiniProfilerMapped,
			TestProvName.SQLiteClassicMiniProfilerUnmapped,
			ProviderName.SybaseManaged,
			ProviderName.OracleManaged,
			TestProvName.Oracle11Managed,
			ProviderName.Firebird,
			TestProvName.Firebird3,
			TestProvName.Firebird4,
			ProviderName.SqlServer2008,
			ProviderName.SqlServer2012,
			ProviderName.SqlServer2014,
			ProviderName.SqlServer2016,
			ProviderName.SqlServer2017,
			TestProvName.SqlServer2019,
			TestProvName.SqlServer2019SequentialAccess,
			TestProvName.SqlServer2019FastExpressionCompiler,
			TestProvName.SqlServerContained,
			ProviderName.SqlServer2000,
			ProviderName.SqlServer2005,
			TestProvName.SqlAzure,
			ProviderName.PostgreSQL,
			ProviderName.PostgreSQL92,
			ProviderName.PostgreSQL93,
			ProviderName.PostgreSQL95,
			TestProvName.PostgreSQL10,
			TestProvName.PostgreSQL11,
			TestProvName.PostgreSQL12,
			TestProvName.PostgreSQL13,
			ProviderName.MySql,
			ProviderName.MySqlConnector,
			TestProvName.MySql55,
			TestProvName.MariaDB,
			ProviderName.SQLiteMS,
			ProviderName.SapHanaNative,
			ProviderName.SapHanaOdbc
		}).ToList();

		protected ITestDataContext GetDataContext(string configuration, MappingSchema? ms = null, bool testLinqService = true)
		{
			if (configuration.EndsWith(".LinqService"))
			{
#if NET472
				OpenHost(ms);

				var str = configuration.Substring(0, configuration.Length - ".LinqService".Length);

				var dx  = testLinqService
					? new ServiceModel.TestLinqServiceDataContext(new LinqService(ms) { AllowUpdates = true }) { Configuration = str }
					: new TestServiceModelDataContext(IP + TestExternals.RunID) { Configuration = str } as RemoteDataContextBase;

				Debug.WriteLine(((IDataContext)dx).ContextID, "Provider ");

				if (ms != null)
					dx.MappingSchema = new MappingSchema(dx.MappingSchema, ms);

				return (ITestDataContext)dx;
#else
				configuration = configuration.Substring(0, configuration.Length - ".LinqService".Length);
#endif
			}

			Debug.WriteLine(configuration, "Provider ");

			var res = new TestDataConnection(configuration);
			if (ms != null)
				res.AddMappingSchema(ms);

			// add extra mapping schema to not share mappers with other sql2017/2019 providers
			// use same schema to use cache within test provider scope
			if (configuration == TestProvName.SqlServer2019SequentialAccess)
				res.AddMappingSchema(_sequentialAccessSchema);
			else if (configuration == TestProvName.SqlServer2019FastExpressionCompiler)
				res.AddMappingSchema(_fecSchema);

			return res;
		}

		private static readonly MappingSchema _sequentialAccessSchema = new ();
		private static readonly MappingSchema _fecSchema = new ();

		protected static char GetParameterToken(string context)
		{
			var token = '@';

			switch (context)
			{
				case ProviderName.SapHanaOdbc:
				case ProviderName.Informix:
					token = '?'; break;
				case ProviderName.SapHanaNative:
				case TestProvName.Oracle11Managed:
				case TestProvName.Oracle11Native:
				case ProviderName.OracleManaged:
				case ProviderName.OracleNative:
					token = ':'; break;
			}

			return CustomizationSupport.Interceptor.GetParameterToken(token, context);
		}

		protected void TestOnePerson(int id, string firstName, IQueryable<Person> persons)
		{
			var list = persons.ToList();

			Assert.AreEqual(1, list.Count);

			var person = list[0];

			Assert.AreEqual(id, person.ID);
			Assert.AreEqual(firstName, person.FirstName);
		}

		protected void TestOneJohn(IQueryable<Person> persons)
		{
			TestOnePerson(1, "John", persons);
		}

		protected void TestPerson(int id, string firstName, IQueryable<IPerson> persons)
		{
			var person = persons.ToList().First(p => p.ID == id);

			Assert.AreEqual(id, person.ID);
			Assert.AreEqual(firstName, person.FirstName);
		}

		protected void TestJohn(IQueryable<IPerson> persons)
		{
			TestPerson(1, "John", persons);
		}

		private   List<LinqDataTypes>?      _types;
		protected IEnumerable<LinqDataTypes> Types
		{
			get
			{
				if (_types == null)
					using (new DisableLogging())
					using (var db = new TestDataConnection())
						_types = db.Types.ToList();

				return _types;
			}
		}

		private   List<LinqDataTypes2>? _types2;
		protected List<LinqDataTypes2>   Types2
		{
			get
			{
				if (_types2 == null)
					using (new DisableLogging())
					using (var db = new TestDataConnection())
						_types2 = db.Types2.ToList();

				return _types2;
			}
		}

		protected internal const int MaxPersonID = 4;

		private   List<Person>?       _person;
		protected IEnumerable<Person>  Person
		{
			get
			{
				if (_person == null)
				{
					using (new DisableLogging())
					using (var db = new TestDataConnection())
						_person = db.Person.ToList();

					foreach (var p in _person)
						p.Patient = Patient.SingleOrDefault(ps => p.ID == ps.PersonID);
				}

				return _person;
			}
		}

		private   List<Patient>? _patient;
		protected List<Patient>   Patient
		{
			get
			{
				if (_patient == null)
				{
					using (new DisableLogging())
					using (var db = new TestDataConnection())
						_patient = db.Patient.ToList();

					foreach (var p in _patient)
						p.Person = Person.Single(ps => ps.ID == p.PersonID);
				}

				return _patient;
			}
		}

		private   List<Doctor>? _doctor;
		protected List<Doctor>   Doctor
		{
			get
			{
				if (_doctor == null)
				{
					using (new DisableLogging())
					using (var db = new TestDataConnection())
						_doctor = db.Doctor.ToList();
				}

				return _doctor;
			}
		}

		#region Parent/Child Model

		private   List<Parent>?      _parent;
		protected IEnumerable<Parent> Parent
		{
			get
			{
				if (_parent == null)
					using (new DisableLogging())
					using (var db = new TestDataConnection())
					{
						_parent = db.Parent.ToList();
						db.Close();

						foreach (var p in _parent)
						{
							p.ParentTest    = p;
							p.Children      = Child         .Where(c => c.ParentID == p.ParentID).ToList();
							p.GrandChildren = GrandChild    .Where(c => c.ParentID == p.ParentID).ToList();
							p.Types         = Types.FirstOrDefault(t => t.ID == p.ParentID);
						}
					}

				return _parent;
			}
		}

		private   List<Parent1>?      _parent1;
		protected IEnumerable<Parent1> Parent1
		{
			get
			{
				if (_parent1 == null)
					_parent1 = Parent.Select(p => new Parent1 { ParentID = p.ParentID, Value1 = p.Value1 }).ToList();

				return _parent1;
			}
		}

		private   List<Parent4>? _parent4;
		protected List<Parent4> Parent4 => _parent4 ??= Parent.Select(p => new Parent4 { ParentID = p.ParentID, Value1 = ConvertTo<TypeValue>.From(p.Value1) }).ToList();

		private   List<Parent5>? _parent5;
		protected List<Parent5>   Parent5
		{
			get
			{
				if (_parent5 == null)
				{
					_parent5 = Parent.Select(p => new Parent5 { ParentID = p.ParentID, Value1 = p.Value1 }).ToList();

					foreach (var p in _parent5)
						p.Children = _parent5.Where(c => c.Value1 == p.ParentID).ToList();
				}

				return _parent5;
			}
		}

		private   List<ParentInheritanceBase>?      _parentInheritance;
		protected IEnumerable<ParentInheritanceBase> ParentInheritance
		{
			get
			{
				if (_parentInheritance == null)
					_parentInheritance = Parent.Select(p =>
						p.Value1 == null ? new ParentInheritanceNull { ParentID = p.ParentID } :
						p.Value1.Value == 1 ? new ParentInheritance1 { ParentID = p.ParentID, Value1 = p.Value1.Value } :
						 (ParentInheritanceBase)new ParentInheritanceValue { ParentID = p.ParentID, Value1 = p.Value1.Value }
					).ToList();

				return _parentInheritance;
			}
		}

		private   List<ParentInheritanceValue>? _parentInheritanceValue;
		protected List<ParentInheritanceValue> ParentInheritanceValue => _parentInheritanceValue ??=
					ParentInheritance.Where(p => p is ParentInheritanceValue).Cast<ParentInheritanceValue>().ToList();

		private   List<ParentInheritance1>? _parentInheritance1;
		protected List<ParentInheritance1> ParentInheritance1 =>
			_parentInheritance1 ??=
				ParentInheritance.Where(p => p is ParentInheritance1).Cast<ParentInheritance1>().ToList();

		private   List<ParentInheritanceBase4>? _parentInheritance4;
		protected List<ParentInheritanceBase4> ParentInheritance4 =>
			_parentInheritance4 ??= Parent
					.Where(p => p.Value1.HasValue && (new[] { 1, 2 }.Contains(p.Value1.Value)))
					.Select(p => p.Value1 == 1 ?
						(ParentInheritanceBase4)new ParentInheritance14 { ParentID = p.ParentID } :
												new ParentInheritance24 { ParentID = p.ParentID }
				).ToList();

		protected List<Child>?      _child;
		protected IEnumerable<Child> Child
		{
			get
			{
				if (_child == null)
					using (new DisableLogging())
					using (var db = new TestDataConnection())
					{
						db.Child.Delete(c => c.ParentID >= 1000);
						_child = db.Child.ToList();
						db.Close();

						foreach (var ch in _child)
						{
							ch.Parent        = Parent .Single(p => p.ParentID == ch.ParentID);
							ch.Parent1       = Parent1.Single(p => p.ParentID == ch.ParentID);
							ch.ParentID2     = new Parent3 { ParentID2 = ch.Parent.ParentID, Value1 = ch.Parent.Value1 };
							ch.GrandChildren = GrandChild.Where(c => c.ParentID == ch.ParentID && c.ChildID == ch.ChildID).ToList();
						}
					}

				foreach (var item in _child)
					yield return item;
			}
		}

		private   List<GrandChild>?      _grandChild;
		protected IEnumerable<GrandChild> GrandChild
		{
			get
			{
				if (_grandChild == null)
					using (new DisableLogging())
					using (var db = new TestDataConnection())
					{
						_grandChild = db.GrandChild.ToList();
						db.Close();

						foreach (var ch in _grandChild)
							ch.Child = Child.Single(c => c.ParentID == ch.ParentID && c.ChildID == ch.ChildID);
					}

				return _grandChild;
			}
		}

		private   List<GrandChild1>?      _grandChild1;
		protected IEnumerable<GrandChild1> GrandChild1
		{
			get
			{
				if (_grandChild1 == null)
					using (new DisableLogging())
					using (var db = new TestDataConnection())
					{
						_grandChild1 = db.GrandChild1.ToList();

						foreach (var ch in _grandChild1)
						{
							ch.Parent = Parent1.Single(p => p.ParentID == ch.ParentID);
							ch.Child  = Child  .Single(c => c.ParentID == ch.ParentID && c.ChildID == ch.ChildID);
						}
					}

				return _grandChild1;
			}
		}

		#endregion

		#region Inheritance Parent/Child Model

		private   List<InheritanceParentBase>? _inheritanceParent;
		protected List<InheritanceParentBase>   InheritanceParent
		{
			get
			{
				if (_inheritanceParent == null)
				{
					using (new DisableLogging())
					using (var db = new TestDataConnection())
						_inheritanceParent = db.InheritanceParent.ToList();
				}

				return _inheritanceParent;
			}
		}

		private   List<InheritanceChildBase>? _inheritanceChild;
		protected List<InheritanceChildBase>   InheritanceChild
		{
			get
			{
				if (_inheritanceChild == null)
				{
					using (new DisableLogging())
					using (var db = new TestDataConnection())
						_inheritanceChild = db.InheritanceChild.LoadWith(_ => _.Parent).ToList();
				}

				return _inheritanceChild;
			}
		}

		#endregion

		#region Northwind

		public TestBaseNorthwind GetNorthwindAsList(string context)
		{
			return new TestBaseNorthwind(context);
		}

		public class TestBaseNorthwind
		{
			private string _context;

			public TestBaseNorthwind(string context)
			{
				_context = context;
			}

			private List<Northwind.Category>? _category;
			public List<Northwind.Category>    Category
			{
				get
				{
					if (_category == null)
						using (new DisableLogging())
						using (var db = new NorthwindDB(_context))
							_category = db.Category.ToList();
					return _category;
				}
			}

			private List<Northwind.Customer>? _customer;
			public List<Northwind.Customer>    Customer
			{
				get
				{
					if (_customer == null)
					{
						using (new DisableLogging())
						using (var db = new NorthwindDB(_context))
							_customer = db.Customer.ToList();

						foreach (var c in _customer)
							c.Orders = (from o in Order where o.CustomerID == c.CustomerID select o).ToList();
					}

					return _customer;
				}
			}

			private List<Northwind.Employee>? _employee;
			public List<Northwind.Employee>    Employee
			{
				get
				{
					if (_employee == null)
					{
						using (new DisableLogging())
						using (var db = new NorthwindDB(_context))
						{
							_employee = db.Employee.ToList();

							foreach (var employee in _employee)
							{
								employee.Employees         = (from e in _employee where e.ReportsTo == employee.EmployeeID select e).ToList();
								employee.ReportsToEmployee = (from e in _employee where e.EmployeeID == employee.ReportsTo select e).SingleOrDefault();
							}
						}
					}

					return _employee;
				}
			}

			private List<Northwind.EmployeeTerritory>? _employeeTerritory;
			public List<Northwind.EmployeeTerritory>    EmployeeTerritory
			{
				get
				{
					if (_employeeTerritory == null)
						using (new DisableLogging())
						using (var db = new NorthwindDB(_context))
							_employeeTerritory = db.EmployeeTerritory.ToList();
					return _employeeTerritory;
				}
			}

			private List<Northwind.OrderDetail>? _orderDetail;
			public List<Northwind.OrderDetail>    OrderDetail
			{
				get
				{
					if (_orderDetail == null)
						using (new DisableLogging())
						using (var db = new NorthwindDB(_context))
							_orderDetail = db.OrderDetail.ToList();
					return _orderDetail;
				}
			}

			private List<Northwind.Order>? _order;
			public List<Northwind.Order>    Order
			{
				get
				{
					if (_order == null)
					{
						using (new DisableLogging())
						using (var db = new NorthwindDB(_context))
							_order = db.Order.ToList();

						foreach (var o in _order)
						{
							o.Customer = Customer.Single(c => o.CustomerID == c.CustomerID);
							o.Employee = Employee.Single(e => o.EmployeeID == e.EmployeeID);
						}
					}

					return _order;
				}
			}

			private IEnumerable<Northwind.Product>? _product;
			public IEnumerable<Northwind.Product>    Product
			{
				get
				{
					if (_product == null)
						using (new DisableLogging())
						using (var db = new NorthwindDB(_context))
							_product = db.Product.ToList();

					foreach (var product in _product)
						yield return product;
				}
			}

			private List<Northwind.ActiveProduct>? _activeProduct;
			public List<Northwind.ActiveProduct> ActiveProduct => _activeProduct ??= Product.OfType<Northwind.ActiveProduct>().ToList();

			public IEnumerable<Northwind.DiscontinuedProduct> DiscontinuedProduct => Product.OfType<Northwind.DiscontinuedProduct>();

			private List<Northwind.Region>? _region;
			public List<Northwind.Region>    Region
			{
				get
				{
					if (_region == null)
						using (new DisableLogging())
						using (var db = new NorthwindDB(_context))
							_region = db.Region.ToList();
					return _region;
				}
			}

			private List<Northwind.Shipper>? _shipper;
			public List<Northwind.Shipper>    Shipper
			{
				get
				{
					if (_shipper == null)
						using (new DisableLogging())
						using (var db = new NorthwindDB(_context))
							_shipper = db.Shipper.ToList();
					return _shipper;
				}
			}

			private List<Northwind.Supplier>? _supplier;
			public List<Northwind.Supplier>    Supplier
			{
				get
				{
					if (_supplier == null)
						using (new DisableLogging())
						using (var db = new NorthwindDB(_context))
							_supplier = db.Supplier.ToList();
					return _supplier;
				}
			}

			private List<Northwind.Territory>? _territory;
			public List<Northwind.Territory>    Territory
			{
				get
				{
					if (_territory == null)
						using (new DisableLogging())
						using (var db = new NorthwindDB(_context))
							_territory = db.Territory.ToList();
					return _territory;
				}
			}
		}

		#endregion

		protected IEnumerable<LinqDataTypes2> AdjustExpectedData(ITestDataContext db, IEnumerable<LinqDataTypes2> data)
		{
			if (db.ProviderNeedsTimeFix(db.ContextID))
			{
				var adjusted = new List<LinqDataTypes2>();
				foreach (var record in data)
				{
					var copy = new LinqDataTypes2()
					{
						ID             = record.ID,
						MoneyValue     = record.MoneyValue,
						DateTimeValue  = record.DateTimeValue,
						DateTimeValue2 = record.DateTimeValue2,
						BoolValue      = record.BoolValue,
						GuidValue      = record.GuidValue,
						SmallIntValue  = record.SmallIntValue,
						IntValue       = record.IntValue,
						BigIntValue    = record.BigIntValue,
						StringValue    = record.StringValue
					};

					if (copy.DateTimeValue != null)
					{
						copy.DateTimeValue = copy.DateTimeValue.Value.AddMilliseconds(-copy.DateTimeValue.Value.Millisecond);
					}

					adjusted.Add(copy);
				}

				return adjusted;
			}

			return data;
		}

		protected bool IsCaseSensitiveDB(string context)
		{
			// we intentionally configure Sql Server 2019 test database to be case-sensitive to test
			// linq2db support for this configuration
			// on CI we test two configurations:
			// linux/mac: db is case sensitive, catalog is case insensitive
			// windows: both db and catalog are case sensitive
			return GetProviderName(context, out var _) == TestProvName.SqlServer2019;
		}

		/// <summary>
		/// Returns case-sensitivity of string comparison (e.g. using LIKE) without explicit collation specified.
		/// Depends on database implementation or database collation.
		/// </summary>
		protected bool IsCaseSensitiveComparison(string context)
		{
			var provider = GetProviderName(context, out var _);

			// we intentionally configure Sql Server 2019 test database to be case-sensitive to test
			// linq2db support for this configuration
			// on CI we test two configurations:
			// linux/mac: db is case sensitive, catalog is case insensitive
			// windows: both db and catalog are case sensitive
			return provider == TestProvName.SqlServer2019
				|| provider == ProviderName.DB2
				|| provider.StartsWith(ProviderName.Firebird)
				|| provider.StartsWith(ProviderName.Informix)
				|| provider.StartsWith(ProviderName.Oracle)
				|| provider.StartsWith(ProviderName.PostgreSQL)
				|| provider.StartsWith(ProviderName.SapHana)
				|| provider.StartsWith(ProviderName.Sybase)
				;
		}

		/// <summary>
		/// Returns status of test CollatedTable - wether it is configured to have proper column collations or
		/// use database defaults (<see cref="IsCaseSensitiveComparison"/>).
		/// </summary>
		protected bool IsCollatedTableConfigured(string context)
		{
			var provider = GetProviderName(context, out var _);

			// unconfigured providers (some could be configured in theory):
			// Access : no such concept as collation on column level (db-only)
			// DB2
			// Informix
			// Oracle (in theory v12 has collations, but to enable them you need to complete quite a quest...)
			// PostgreSQL (v12 + custom collation required (no default CI collations))
			// SAP HANA
			// SQL CE
			// Sybase ASE
			return provider == TestProvName.SqlAzure
				|| provider == TestProvName.MariaDB
				|| provider == TestProvName.AllOracleNative
				|| provider.StartsWith(ProviderName.SqlServer)
				|| provider.StartsWith(ProviderName.Firebird)
				|| provider.StartsWith(ProviderName.MySql)
				// while it is configured, LIKE in SQLite is case-insensitive (for ASCII only though)
				//|| provider.StartsWith(ProviderName.SQLite)
				;
		}

		protected void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> result, bool allowEmpty = false)
		{
			AreEqual(t => t, expected, result, EqualityComparer<T>.Default, allowEmpty);
		}

		protected void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> result, Func<IEnumerable<T>, IEnumerable<T>> sort)
		{
			AreEqual(t => t, expected, result, EqualityComparer<T>.Default, sort);
		}

		protected void AreEqualWithComparer<T>(IEnumerable<T> expected, IEnumerable<T> result)
		{
			AreEqual(t => t, expected, result, ComparerBuilder.GetEqualityComparer<T>());
		}

		protected void AreEqualWithComparer<T>(IEnumerable<T> expected, IEnumerable<T> result, Func<MemberAccessor,bool> memberPredicate)
		{
			AreEqual(t => t, expected, result, ComparerBuilder.GetEqualityComparer<T>(memberPredicate));
		}

		protected void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> result, IEqualityComparer<T> comparer)
		{
			AreEqual(t => t, expected, result, comparer);
		}

		protected void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> result, IEqualityComparer<T> comparer, Func<IEnumerable<T>, IEnumerable<T>> sort)
		{
			AreEqual(t => t, expected, result, comparer, sort);
		}

		protected void AreEqual<T>(Func<T, T> fixSelector, IEnumerable<T> expected, IEnumerable<T> result)
		{
			AreEqual(fixSelector, expected, result, EqualityComparer<T>.Default);
		}

		protected void AreEqual<T>(Func<T, T> fixSelector, IEnumerable<T> expected, IEnumerable<T> result, IEqualityComparer<T> comparer, bool allowEmpty = false)
		{
			AreEqual(fixSelector, expected, result, comparer, null, allowEmpty);
		}

		protected void AreEqual<T>(
			Func<T, T> fixSelector,
			IEnumerable<T> expected,
			IEnumerable<T> result,
			IEqualityComparer<T> comparer,
			Func<IEnumerable<T>, IEnumerable<T>>? sort,
			bool allowEmpty = false)
		{
			var resultList   = result.  Select(fixSelector).ToList();
			var expectedList = expected.Select(fixSelector).ToList();

			if (sort != null)
			{
				resultList   = sort(resultList).  ToList();
				expectedList = sort(expectedList).ToList();
			}

			if (!allowEmpty)
				Assert.AreNotEqual(0, expectedList.Count, "Expected list cannot be empty.");
			Assert.AreEqual(expectedList.Count, resultList.Count, "Expected and result lists are different. Length: ");

			var exceptExpectedList = resultList.  Except(expectedList, comparer).ToList();
			var exceptResultList   = expectedList.Except(resultList,   comparer).ToList();

			var exceptExpected = exceptExpectedList.Count;
			var exceptResult   = exceptResultList.  Count;
			var message        = new StringBuilder();

			if (exceptResult != 0 || exceptExpected != 0)
			{
				Debug.WriteLine(resultList.  ToDiagnosticString());
				Debug.WriteLine(expectedList.ToDiagnosticString());

				for (var i = 0; i < resultList.Count; i++)
				{
					var equals = comparer.Equals(expectedList[i], resultList[i]);
					Debug.  WriteLine   ("{0} {1} {3} {2}", equals ? " " : "!", expectedList[i], resultList[i], equals ? "==" : "<>");
					message.AppendFormat("{0} {1} {3} {2}", equals ? " " : "!", expectedList[i], resultList[i], equals ? "==" : "<>");
					message.AppendLine();
				}
			}

			Assert.AreEqual(0, exceptExpected, $"Expected Was{Environment.NewLine}{message}");
			Assert.AreEqual(0, exceptResult  , $"Expect Result{Environment.NewLine}{message}");
		}

		protected void AreEqual<T>(IEnumerable<IEnumerable<T>> expected, IEnumerable<IEnumerable<T>> result)
		{
			var resultList   = result.ToList();
			var expectedList = expected.ToList();

			Assert.AreNotEqual(0, expectedList.Count);
			Assert.AreEqual(expectedList.Count, resultList.Count, "Expected and result lists are different. Length: ");

			for (var i = 0; i < resultList.Count; i++)
			{
				var elist = expectedList[i].ToList();
				var rlist = resultList[i].ToList();

				if (elist.Count > 0 || rlist.Count > 0)
					AreEqual(elist, rlist);
			}
		}

		protected void AreSame<T>(IEnumerable<T> expected, IEnumerable<T> result)
		{
			var resultList   = result.ToList();
			var expectedList = expected.ToList();

			Assert.AreNotEqual(0, expectedList.Count);
			Assert.AreEqual(expectedList.Count, resultList.Count);

			var b = expectedList.SequenceEqual(resultList);

			if (!b)
				for (var i = 0; i < resultList.Count; i++)
					Debug.WriteLine("{0} {1} --- {2}", Equals(expectedList[i], resultList[i]) ? " " : "-", expectedList[i], resultList[i]);

			Assert.IsTrue(b);
		}

		public static Expression CheckForNull(Expression? expression)
		{
			if (expression != null && expression.NodeType.NotIn(ExpressionType.Lambda, ExpressionType.Quote) &&
			    (expression.Type.IsClass || expression.Type.IsInterface) &&
			    !typeof(IQueryable<>).IsSameOrParentOf(expression.Type))
			{
				var test = Expression.ReferenceEqual(expression,
					Expression.Constant(null, expression.Type));
				return Expression.Condition(test,
					Expression.Constant(DefaultValue.GetValue(expression.Type), expression.Type), expression);
			}

			return expression;
		}

		public static Expression CheckForNull(Expression? expression, Expression trueExpression, Expression falseExpression)
		{
			if (expression is MemberExpression me)
				expression = CheckForNull(me);

			if (expression != null && expression.NodeType.NotIn(ExpressionType.Lambda, ExpressionType.Quote) &&
			    (expression.Type.IsClass || expression.Type.IsInterface) &&
			    !typeof(IQueryable<>).IsSameOrParentOf(expression.Type))
			{
				var test = Expression.ReferenceEqual(expression,
					Expression.Constant(null, expression.Type));
				return Expression.Condition(test, trueExpression, falseExpression);
			}

			return expression;
		}

		public static List<Expression> GetMemberPath(Expression? expression)
		{
			var result = new List<Expression>();

			var current = expression;
			while (current != null)
			{
				var prev = current;
				switch (current.NodeType)
				{
					case ExpressionType.MemberAccess:
					{
						result.Add(current);
						current = ((MemberExpression)current).Expression;
						break;
					}
				}

				if (prev == current)
				{
					result.Add(current);
					break;
				}
			}

			result.Reverse();
			return result;
		}


		public static Expression CheckForNull(MemberExpression expr)
		{
			Expression? test = null;

			var path = GetMemberPath(expr);

			for (int i = 0; i < path.Count - 1; i++)
			{
				var objExpr = path[i];
				if (objExpr != null && (objExpr.Type.IsClass || objExpr.Type.IsInterface))
				{
					var currentTest = Expression.ReferenceEqual(objExpr,
						Expression.Constant(null, objExpr.Type));
					if (test == null)
						test = currentTest;
					else
					{
						test = Expression.OrElse(test, currentTest);
					}
				}
			}

			if (test == null)
				return expr;

			return Expression.Condition(test,
				Expression.Constant(DefaultValue.GetValue(expr.Type), expr.Type), expr);
		}

		static Expression ApplyNullCheck(Expression expr)
		{
			var newExpr = expr.Transform(e =>
			{
				if (e.NodeType == ExpressionType.Call)
				{
					var mc = (MethodCallExpression)e;

					if (!mc.Method.IsStatic)
					{
						var checkedMethod = CheckForNull(mc.Object, Expression.Constant(DefaultValue.GetValue(mc.Method.ReturnType), mc.Method.ReturnType), mc);
						return new TransformInfo(checkedMethod);
					}
				}
				else if (e.NodeType == ExpressionType.MemberAccess)
				{
					var ma = (MemberExpression)e;

					return new TransformInfo(CheckForNull(ma));
				}

				return new TransformInfo(e);
			})!;

			return newExpr;
		}

		public T[] AssertQuery<T>(IQueryable<T> query)
		{
			var expr   = query.Expression;
			var actual = query.ToArray();

			var loaded = new Dictionary<Type, Expression>();

			Expression RegisterLoaded(Type eType, Expression tableExpression)
			{
				if (!loaded.TryGetValue(eType, out var itemsExpression))
				{
					var newCall = LinqToDB.Common.TypeHelper.MakeMethodCall(Methods.Queryable.ToArray, tableExpression);
					using (new DisableLogging())
					{
						var items = newCall.EvaluateExpression();
						itemsExpression = Expression.Constant(items, eType.MakeArrayType());
						loaded.Add(eType, itemsExpression);
					}
				}

				var queryCall =
					LinqToDB.Common.TypeHelper.MakeMethodCall(Methods.Enumerable.AsQueryable,
						itemsExpression);

				return queryCall;
			}

			var newExpr = expr.Transform(e =>
			{
				if (e.NodeType == ExpressionType.Call)
				{
					var mc = (MethodCallExpression)e;

					if (mc.IsSameGenericMethod(Methods.LinqToDB.AsSubQuery))
						return new TransformInfo(mc.Arguments[0], false, true);

					if (typeof(ITable<>).IsSameOrParentOf(mc.Type))
					{
						var entityType = mc.Method.ReturnType.GetGenericArguments()[0];

						if (entityType != null)
						{
							var itemsExpression = RegisterLoaded(entityType, mc);
							return new TransformInfo(itemsExpression);
						}
					}

					if (mc.IsQueryable("InnerJoin"))
					{
						mc = TypeHelper.MakeMethodCall(Methods.Queryable.Where, mc.Arguments.ToArray());
					}

					return new TransformInfo(mc, false, true);
					//return mc.Update(CheckForNull(mc.Object), mc.Arguments.Select(CheckForNull));
				}
				else if (e.NodeType == ExpressionType.MemberAccess)
				{
					if (typeof(ITable<>).IsSameOrParentOf(e.Type))
					{
						var entityType = e.Type.GetGenericArguments()[0];
						var items = RegisterLoaded(entityType, e);
						return new TransformInfo(items);
					}
				}

				return new TransformInfo(e);
			})!;


			newExpr = ApplyNullCheck(newExpr);

			var empty = LinqToDB.Common.Tools.CreateEmptyQuery<T>();
			T[]? expected;

			expected = empty.Provider.CreateQuery<T>(newExpr).ToArray();

			if (actual.Length > 0 || expected.Length > 0)
				AreEqual(expected, actual, ComparerBuilder.GetEqualityComparer<T>());

			return actual;
		}

		protected void CompareSql(string expected, string result)
		{
			Assert.AreEqual(normalize(expected), normalize(result));

			static string normalize(string sql)
			{
				var lines = sql.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				return string.Join("\n", lines.Where(l => !l.StartsWith("-- ")).Select(l => l.TrimStart('\t', ' ')));
			}
		}

		protected List<LinqDataTypes> GetTypes(string context)
		{
			return DataCache<LinqDataTypes>.Get(context);
		}

		public static TempTable<T> CreateTempTable<T>(IDataContext db, string tableName, string context)
			where T : notnull
		{
			return TempTable.Create<T>(db, GetTempTableName(tableName, context));
		}

		public static string GetTempTableName(string tableName, string context)
		{
			var finalTableName = tableName;
			switch (GetProviderName(context, out var _))
			{
				case TestProvName.SqlAzure                           :
				case ProviderName.SqlServer                          :
				case ProviderName.SqlServer2000                      :
				case ProviderName.SqlServer2005                      :
				case ProviderName.SqlServer2008                      :
				case ProviderName.SqlServer2012                      :
				case ProviderName.SqlServer2014                      :
				case ProviderName.SqlServer2016                      :
				case ProviderName.SqlServer2017                      :
				case TestProvName.SqlServer2019                      :
				case TestProvName.SqlServer2019SequentialAccess      :
				case TestProvName.SqlServer2019FastExpressionCompiler:
				case TestProvName.SqlServerContained                 :
					{
					if (!tableName.StartsWith("#"))
						finalTableName = "#" + tableName;
					break;
				}
				default:
					throw new NotImplementedException();
			}

			return finalTableName;
		}

		protected static string GetProviderName(string context, out bool isLinqService)
		{
			isLinqService = context.EndsWith(".LinqService");
			return context.Replace(".LinqService", "");
		}

		[SetUp]
		public virtual void OnBeforeTest()
		{
			// SequentialAccess-enabled provider setup
			var (provider, _) = NUnitUtils.GetContext(TestExecutionContext.CurrentContext.CurrentTest);
			if (provider == TestProvName.SqlServer2019SequentialAccess)
			{
				Configuration.OptimizeForSequentialAccess = true;
				DbCommandProcessorExtensions.Instance = new SequentialAccessCommandProcessor();
			}
			else if (provider == TestProvName.SqlServer2019FastExpressionCompiler)
			{
				//Compilation.SetExpressionCompiler(_ => ExpressionCompiler.CompileFast(_, true));
			}
		}

		[TearDown]
		public virtual void OnAfterTest()
		{
			// SequentialAccess-enabled provider cleanup
			var (provider, _) = NUnitUtils.GetContext(TestExecutionContext.CurrentContext.CurrentTest);
			if (provider == TestProvName.SqlServer2019SequentialAccess)
			{
				Configuration.OptimizeForSequentialAccess = false;
				DbCommandProcessorExtensions.Instance = null;
			}
			if (provider == TestProvName.SqlServer2019FastExpressionCompiler)
			{
				//Compilation.SetExpressionCompiler(null);
			}

			if (provider?.Contains("SapHana") == true)
			{
				using (new DisableLogging())
				using (new DisableBaseline("isn't baseline query"))
				using (var db = new TestDataConnection(provider))
				{
					// release memory
					db.Execute("ALTER SYSTEM CLEAR SQL PLAN CACHE");
				}
			}

			// dump baselines
			var ctx = CustomTestContext.Get();

			if (_baselinesPath != null)
			{
				var baseline = ctx.Get<StringBuilder>(CustomTestContext.BASELINE);
				if (baseline != null)
					BaselinesWriter.Write(_baselinesPath, baseline.ToString());
			}

			var trace = ctx.Get<StringBuilder>(CustomTestContext.TRACE);
			if (trace != null && TestContext.CurrentContext.Result.FailCount > 0 && ctx.Get<bool>(CustomTestContext.LIMITED))
			{
				// we need to set ErrorInfo.Message element text
				// because Azure displays only ErrorInfo node data
				TestExecutionContext.CurrentContext.CurrentResult.SetResult(
					TestExecutionContext.CurrentContext.CurrentResult.ResultState,
					TestExecutionContext.CurrentContext.CurrentResult.Message + "\r\n" + trace.ToString(),
					TestExecutionContext.CurrentContext.CurrentResult.StackTrace);
			}

			CustomTestContext.Release();
		}

		protected string GetCurrentBaselines()
		{
			return CustomTestContext.Get().Get<StringBuilder>(CustomTestContext.BASELINE)?.ToString() ?? string.Empty;
		}

		protected static bool IsIDSProvider(string context)
		{
			if (!context.Contains("Informix"))
				return false;
			var providerName = GetProviderName(context, out var _);
			if (providerName == ProviderName.InformixDB2)
				return true;

			using (DataConnection dc = new TestDataConnection(GetProviderName(context, out var _)))
				return ((InformixDataProvider)dc.DataProvider).Adapter.IsIDSProvider;
		}
	}

	static class DataCache<T>
		where T : class
	{
		static readonly Dictionary<string,List<T>> _dic = new ();
		public static List<T> Get(string context)
		{
			lock (_dic)
			{
				context = context.Replace(".LinqService", "");

				if (!_dic.TryGetValue(context, out var list))
				{
					using (new DisableLogging())
					using (var db = new DataConnection(context))
					{
						list = db.GetTable<T>().ToList();
						_dic.Add(context, list);
					}
				}

				return list;
			}
		}

		public static void Clear()
		{
			_dic.Clear();
		}
	}

	public static class Helpers
	{
		public static string ToInvariantString<T>(this T data)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}", data)
				.Replace(',', '.').Trim(' ', '.', '0');
		}
	}

	public class GuardGrouping : IDisposable
	{
		private readonly bool _oldValue = Configuration.Linq.GuardGrouping;

		public GuardGrouping(bool enable)
		{
			Configuration.Linq.GuardGrouping = enable;
		}

		public void Dispose()
		{
			Configuration.Linq.GuardGrouping = _oldValue;
		}
	}

	public class ParameterizeTakeSkip : IDisposable
	{
		private readonly bool _oldValue = Configuration.Linq.ParameterizeTakeSkip;

		public ParameterizeTakeSkip(bool enable)
		{
			Configuration.Linq.ParameterizeTakeSkip = enable;
		}

		public void Dispose()
		{
			Configuration.Linq.ParameterizeTakeSkip = _oldValue;
		}
	}

	public class PreloadGroups : IDisposable
	{
		private readonly bool _oldValue = Configuration.Linq.PreloadGroups;

		public PreloadGroups(bool enable)
		{
			Configuration.Linq.PreloadGroups = enable;
		}

		public void Dispose()
		{
			Configuration.Linq.PreloadGroups = _oldValue;
		}
	}

	public class GenerateExpressionTest : IDisposable
	{
		private readonly bool _oldValue = Configuration.Linq.GenerateExpressionTest;

		public GenerateExpressionTest(bool enable)
		{
			Configuration.Linq.GenerateExpressionTest = enable;
		}

		public void Dispose()
		{
			Configuration.Linq.GenerateExpressionTest = _oldValue;
		}
	}

	public class DoNotClearOrderBys : IDisposable
	{
		private readonly bool _oldValue = Configuration.Linq.DoNotClearOrderBys;

		public DoNotClearOrderBys(bool enable)
		{
			Configuration.Linq.DoNotClearOrderBys = enable;
		}

		public void Dispose()
		{
			Configuration.Linq.DoNotClearOrderBys = _oldValue;
		}
	}

	public class GenerateFinalAliases : IDisposable
	{
		private readonly bool _oldValue = Configuration.Sql.GenerateFinalAliases;

		public GenerateFinalAliases(bool enable)
		{
			Configuration.Sql.GenerateFinalAliases = enable;
		}

		public void Dispose()
		{
			Configuration.Sql.GenerateFinalAliases = _oldValue;
		}
	}

	public class SerializeAssemblyQualifiedName : IDisposable
	{
		private readonly bool _oldValue = Configuration.LinqService.SerializeAssemblyQualifiedName;

		public SerializeAssemblyQualifiedName(bool enable)
		{
			Configuration.LinqService.SerializeAssemblyQualifiedName = enable;
		}

		public void Dispose()
		{
			Configuration.LinqService.SerializeAssemblyQualifiedName = _oldValue;
		}
	}

	public class DisableLogging : IDisposable
	{
		private readonly CustomTestContext _ctx;
		private readonly bool _oldState;

		public DisableLogging()
		{
			_ctx   = CustomTestContext.Get();
			_oldState = _ctx.Get<bool>(CustomTestContext.TRACE_DISABLED);
			_ctx.Set(CustomTestContext.TRACE_DISABLED, true);
		}

		public void Dispose()
		{
			_ctx.Set(CustomTestContext.TRACE_DISABLED, _oldState);
		}
	}

	public class DisableBaseline : IDisposable
	{
		private readonly CustomTestContext _ctx;
		private readonly bool _oldState;

		public DisableBaseline(string reason, bool disable = true)
		{
			_ctx = CustomTestContext.Get();
			_oldState = _ctx.Get<bool>(CustomTestContext.BASELINE_DISABLED);
			_ctx.Set(CustomTestContext.BASELINE_DISABLED, disable);
		}

		public void Dispose()
		{
			_ctx.Set(CustomTestContext.BASELINE_DISABLED, _oldState);
		}
	}

	public class DisableQueryCache : IDisposable
	{
		private readonly bool _oldValue = Configuration.Linq.DisableQueryCache;

		public DisableQueryCache(bool value = true)
		{
			Configuration.Linq.DisableQueryCache = value;
		}

		public void Dispose()
		{
			Configuration.Linq.DisableQueryCache = _oldValue;
		}
	}

	public class WithoutJoinOptimization : IDisposable
	{
		public WithoutJoinOptimization(bool opimizerSwitch = false)
		{
			Configuration.Linq.OptimizeJoins = opimizerSwitch;
			Query.ClearCaches();
		}

		public void Dispose()
		{
			Configuration.Linq.OptimizeJoins = true;
		}
	}

	public class DeletePerson : IDisposable
	{
		readonly IDataContext _db;

		public DeletePerson(IDataContext db)
		{
			_db = db;
			Delete(_db);
		}

		public void Dispose()
		{
			Delete(_db);
		}

		readonly Func<IDataContext,int> Delete =
			CompiledQuery.Compile<IDataContext, int>(db => db.GetTable<Person>().Delete(_ => _.ID > TestBase.MaxPersonID));

	}

	public class WithoutComparisonNullCheck : IDisposable
	{
		public WithoutComparisonNullCheck()
		{
			Configuration.Linq.CompareNullsAsValues = false;
		}

		public void Dispose()
		{
			Configuration.Linq.CompareNullsAsValues = true;
			Query.ClearCaches();
		}
	}

	public static class TestExternals
	{
		public static string? LogFilePath;
		public static bool    IsParallelRun;
		public static int     RunID;
		public static string? Configuration;

		static StreamWriter? _logWriter;

		public static void Log(string text)
		{
			if (LogFilePath != null)
			{
				if (_logWriter == null)
					_logWriter = File.CreateText(LogFilePath);

				_logWriter.WriteLine(text);
				_logWriter.Flush();
			}
		}
	}

	public class CustomCommandProcessor : IDisposable
	{
		private readonly IDbCommandProcessor? _original = DbCommandProcessorExtensions.Instance;
		public CustomCommandProcessor(IDbCommandProcessor? processor)
		{
			DbCommandProcessorExtensions.Instance = processor;
		}

		public void Dispose()
		{
			DbCommandProcessorExtensions.Instance = _original;
		}
	}

	public class OptimizeForSequentialAccess : IDisposable
	{
		private readonly bool _original = Configuration.OptimizeForSequentialAccess;
		public OptimizeForSequentialAccess(bool enable)
		{
			Configuration.OptimizeForSequentialAccess = enable;
		}

		public void Dispose()
		{
			Configuration.OptimizeForSequentialAccess = _original;
		}
	}
}
