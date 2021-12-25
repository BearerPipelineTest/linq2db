﻿using System.Data.Common;
using System.IO;
using System.Reflection;
using LinqToDB.CodeGen.CodeGeneration;
using LinqToDB.CodeGen.Configuration;
using LinqToDB.Naming;
using LinqToDB.CodeModel;
using LinqToDB.Data;
using LinqToDB.Metadata;
using LinqToDB.Scaffold;
using Microsoft.Extensions.Configuration;

namespace LinqToDB.Tools
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			Directory.CreateDirectory(@"..\..\..\Generated");

			BuildModel("sql.2017");
			BuildModel("pg10");
			
			//BuildModel("mysql55");
			//BuildModel("access.oledb");
			//BuildModel("db2");

			NameNormalizationTest.NormalizationTest();
			
			if (args.Length == 0) return 0;


			BuildModel("sqlite.classic");
			BuildModel("sqlite.ms");
			BuildModel("sqlite.nw.classic");
			BuildModel("sqlite.nw.ms");
			//var sqlce = Assembly.LoadFrom(@"c:\Program Files\Microsoft SQL Server Compact Edition\v4.0\Desktop\System.Data.SqlServerCe.dll");
			//BuildModel("sqlce");
			BuildModel("firebird25");
			BuildModel("firebird3");
			BuildModel("firebird4");
			BuildModel("sql.2005");
			BuildModel("sql.2008");
			BuildModel("sql.2012");
			BuildModel("sql.2014");
			BuildModel("sql.2017");
			BuildModel("sql.contained");
			BuildModel("sql.2019");
			BuildModel("sql.2019.ms");
			BuildModel("sql.nw");
			BuildModel("sql.azure");
			BuildModel("mysql");
			BuildModel("mysql55");
			BuildModel("mysqlconnector");
			BuildModel("mariadb");
			BuildModel("pg92");
			BuildModel("pg93");
			BuildModel("pg95");
			BuildModel("pg10");
			BuildModel("pg11");
			BuildModel("access.oledb");
			BuildModel("access.odbc");
			BuildModel("sybase.managed");
			BuildModel("db2");
			BuildModel("db2.ifx");
			BuildModel("ora11.managed");
			BuildModel("sap.odbc");

			RegisterSapHanaFactory();
			BuildModel("sap.native");

			//BuildModel("sqlite");
			//BuildModel("sql2017");
			//BuildModel("pg11");

			return 0;
		}

		private static void ReadSettings()
		{
		}

		private static void RegisterSapHanaFactory()
		{
			try
			{
				// woo-hoo, hardcoded pathes! default install location on x64 system
				var srcPath = @"c:\Program Files (x86)\sap\hdbclient\dotnetcore\v2.1\Sap.Data.Hana.Core.v2.1.dll";
				var targetPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory!, Path.GetFileName(srcPath));
				if (File.Exists(srcPath))
				{
					// original path contains spaces which breaks broken native dlls discovery logic in SAP provider
					// if you run tests from path with spaces - it will not help you
					File.Copy(srcPath, targetPath, true);
					var sapHanaAssembly = Assembly.LoadFrom(targetPath);
					DbProviderFactories.RegisterFactory("Sap.Data.Hana", sapHanaAssembly.GetType("Sap.Data.Hana.HanaFactory")!);
				}
			}
			catch { }
		}

		private static void BuildModel(string configName)
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile($"Configuration\\{configName}.json", true, false)
				.Build();

			IModelSettings settings =  new ModelSettings(config);


			using (var dc = new DataConnection(settings.Provider, settings.ConnectionString))
			{
				var codegenSettings = new CodeGenerationSettings();

				//codegenSettings.MarkAsAutoGenerated = true;
				codegenSettings.BaseContextClass = "LinqToDB.DataContext";
				codegenSettings.Namespace = $"DataModel.{configName}";
				codegenSettings.ClassPerFile = true;

				// namespace generated by naming test
				codegenSettings.ConflictingNames.Add("DataType");


				var generator = new Scaffolder(LanguageProviders.CSharp, HumanizerNameConverter.Instance, codegenSettings);
				var dataModel = generator.LoadDataModel(dc);
				var sqlBuilder = dc.DataProvider.CreateSqlBuilder(dc.MappingSchema);
				var files = generator.GenerateCodeModel(
					sqlBuilder,
					dataModel,
					MetadataBuilders.GetAttributeBasedMetadataBuilder(generator.Language, sqlBuilder),
					SqlBoolEqualityConverter.Create(generator.Language));
				var sourceCode = generator.GenerateSourceCode(files);

				var root = $@"..\..\..\Generated\{configName}";
				Directory.CreateDirectory(root);

				for (var i = 0; i < sourceCode.Length; i++)
				{
					// TODO: file name normalization/deduplication
					File.WriteAllText($@"{root}\{sourceCode[i].FileName}", sourceCode[i].Code);
				}
			}
		}
	}
}

