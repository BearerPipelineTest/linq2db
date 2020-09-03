﻿using System;
using System.Linq;

using IBM.Data.DB2;

#if !NET46
using IBM.Data.DB2.Core;
#endif

using NUnit.Framework;

namespace Tests.Linq
{
	using LinqToDB;
	using LinqToDB.Data;
	using LinqToDB.Mapping;

	[TestFixture]
	public class TableOptionsTests : TestBase
	{
		[Table(IsTemporary = true)]
		[Table(IsTemporary = true, Configuration = ProviderName.SqlServer,  Database = "TestData", Schema = "TestSchema")]
		[Table(IsTemporary = true, Configuration = ProviderName.Sybase,     Database = "TestData")]
		[Table(IsTemporary = true, Configuration = ProviderName.SQLite)]
		[Table(IsTemporary = true, Configuration = ProviderName.PostgreSQL, Database = "TestData", Schema = "test_schema")]
		[Table(IsTemporary = true, Configuration = ProviderName.DB2,                               Schema = "SESSION")]
		class IsTemporaryTable
		{
			[Column] public int Id    { get; set; }
			[Column] public int Value { get; set; }
		}

		[Test]
		public void IsTemporaryTest([DataSources(false)] string context, [Values(true)] bool firstCall)
		{
			using var db = (DataConnection)GetDataContext(context);

			try
			{
				using var table = db.CreateTempTable<IsTemporaryTable>();

				var result = table.ToArray();
			}
			catch (DB2Exception ex) when (firstCall && ex.ErrorCode == -2147467259)
			{
//				db.Execute("DROP TABLESPACE DBHOSTTEMPU_32K;");
//				db.Execute("DROP TABLESPACE DBHOSTTEMPS_32K;");
//				db.Execute("DROP TABLESPACE DBHOST_32K;");
//				db.Execute("DROP BUFFERPOOL DBHOST_32K;");

				db.Execute("CREATE BUFFERPOOL DBHOST_32K IMMEDIATE SIZE 250 AUTOMATIC PAGESIZE 32K;");
				db.Execute("CREATE LARGE TABLESPACE DBHOST_32K PAGESIZE 32K MANAGED BY AUTOMATIC STORAGE EXTENTSIZE 32 PREFETCHSIZE 32 BUFFERPOOL DBHOST_32K;");
				db.Execute("CREATE USER TEMPORARY TABLESPACE DBHOSTTEMPU_32K PAGESIZE 32K MANAGED BY AUTOMATIC STORAGE BUFFERPOOL DBHOST_32K;");
				db.Execute("CREATE SYSTEM TEMPORARY TABLESPACE DBHOSTTEMPS_32K PAGESIZE 32K MANAGED BY AUTOMATIC STORAGE BUFFERPOOL DBHOST_32K;");

				IsGlobalTemporaryTest(context, false);
			}

		}

		[Table(TableOptions = TableOptions.IsGlobalTemporary)]
		[Table(TableOptions = TableOptions.IsGlobalTemporary, Configuration = ProviderName.DB2, Schema = "SESSION")]
		class IsGlobalTemporaryTable
		{
			[Column] public int Id    { get; set; }
			[Column] public int Value { get; set; }
		}

		[Test]
		public void IsGlobalTemporaryTest([IncludeDataSources(
			ProviderName.DB2,
			ProviderName.Firebird,
			TestProvName.AllSqlServer2005Plus,
			TestProvName.AllSybase)] string context,
			[Values(true)] bool firstCall)
		{
			using var db = (DataConnection)GetDataContext(context);

			try
			{
				using var table = db.CreateTempTable<IsGlobalTemporaryTable>();

				var result = table.ToArray();
			}
			catch (DB2Exception ex) when (firstCall && ex.ErrorCode == -2147467259)
			{
//				db.Execute("DROP TABLESPACE DBHOSTTEMPU_32K;");
//				db.Execute("DROP TABLESPACE DBHOSTTEMPS_32K;");
//				db.Execute("DROP TABLESPACE DBHOST_32K;");
//				db.Execute("DROP BUFFERPOOL DBHOST_32K;");

				db.Execute("CREATE BUFFERPOOL DBHOST_32K IMMEDIATE SIZE 250 AUTOMATIC PAGESIZE 32K;");
				db.Execute("CREATE LARGE TABLESPACE DBHOST_32K PAGESIZE 32K MANAGED BY AUTOMATIC STORAGE EXTENTSIZE 32 PREFETCHSIZE 32 BUFFERPOOL DBHOST_32K;");
				db.Execute("CREATE USER TEMPORARY TABLESPACE DBHOSTTEMPU_32K PAGESIZE 32K MANAGED BY AUTOMATIC STORAGE BUFFERPOOL DBHOST_32K;");
				db.Execute("CREATE SYSTEM TEMPORARY TABLESPACE DBHOSTTEMPS_32K PAGESIZE 32K MANAGED BY AUTOMATIC STORAGE BUFFERPOOL DBHOST_32K;");

				IsGlobalTemporaryTest(context, false);
			}
		}

		[Table(TableOptions = TableOptions.CreateIfNotExists)]
		[Table(TableOptions = TableOptions.CreateIfNotExists | TableOptions.IsTemporary, Configuration = ProviderName.SqlServer2008)]
		[Table("##temp_table", TableOptions = TableOptions.CreateIfNotExists, Configuration = ProviderName.SqlServer2012)]
		class CreateIfNotExistsTable
		{
			[Column] public int Id    { get; set; }
			[Column] public int Value { get; set; }
		}

		[Test]
		public void CreateIfNotExistsTest([IncludeDataSources(
			true,
			ProviderName.DB2,
			ProviderName.Informix,
			ProviderName.Firebird,
			TestProvName.AllMySql,
			TestProvName.AllOracle,
			ProviderName.PostgreSQL,
			TestProvName.AllSQLite,
			TestProvName.AllSqlServer2005Plus,
			TestProvName.AllSybase)] string context)
		{
			if (context == "SqlServer.2008.LinqService" ||
				context == "SqlServer.2012.LinqService" ||
				context == "SqlServer.2014.LinqService")
				return;

			using var db = GetDataContext(context);

			db.DropTable<CreateIfNotExistsTable>(throwExceptionIfNotExists:false);

			using var table = db.CreateTempTable<CreateIfNotExistsTable>();

			var result = table.ToArray();

			var table1 = db.CreateTempTable<CreateIfNotExistsTable>();
		}
	}
}