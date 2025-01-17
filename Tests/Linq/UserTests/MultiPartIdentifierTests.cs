﻿using LinqToDB;
using LinqToDB.Mapping;

using NUnit.Framework;

// ReSharper disable ClassNeverInstantiated.Local

namespace Tests.UserTests
{
	[TestFixture]
	public class MultiPartIdentifierTests : TestBase
	{
		[Table]
		class Table1
		{
			[Column]
			public long Field1;

			[Column]
			public long Field2;

			[Column]
			public int? Field3;

			[Association(ThisKey = "Field2", OtherKey = "Field2", CanBeNull = false)]
			public Table2 Table2Ref { get; set; } = null!;

			[Association(ThisKey = "Field3", OtherKey = "Field3", CanBeNull = true)]
			public Table4? Table4Ref { get; set; }
		}

		[Table]
		class Table2
		{
			[Column]
			public long Field2 { get; set; }

			[Column]
			public int  Field4 { get; set; }

			[Association(ThisKey = "Field2", OtherKey = "Field2", CanBeNull = false)]
			public List<Table1> Table1s { get; set; } = null!;

			[Association(ThisKey="Field4", OtherKey="Field4", CanBeNull=false)]
			public Table3 Table3Ref { get; set; } = null!;
		}

		[Table]
		class Table3
		{
			[Column]
			public int Field4;

			[Association(ThisKey="Field4", OtherKey="Field4", CanBeNull=true)]
			public List<Table2> Table2s { get; set; } = null!;
		}

		[Table]
		class Table4
		{
			[Column]
			public int Field3 { get; set; }

			[Column]
			public int Field4 { get; set; }

			[Association(ThisKey = "Field3", OtherKey = "Field3", CanBeNull = true)]
			public List<Table1> Table1s { get; set; } = null!;

			[Association(ThisKey="Field4", OtherKey="ProblematicalField", CanBeNull=false)]
			public Table5 Table5Ref { get; set; } = null!;
		}

		[Table]
		class Table5
		{
			[Column]
			public int? Field5;

			[Column]
			public int  ProblematicalField;

			[Association(ThisKey = "Field5", OtherKey = "ProblematicalField", CanBeNull = true)]
			public Table5? Table5Ref { get; set; }

			[Association(ThisKey = "ProblematicalField", OtherKey = "Field4", CanBeNull = true)]
			public List<Table4> Table4s { get; set; } = null!;
		}

		[Test]
		public void Test([IncludeDataSources(TestProvName.AllSqlServer2008Plus)]
			string context)
		{
			using (var db = GetDataContext(context))
			using (db.CreateLocalTable<Table1>())
			using (db.CreateLocalTable<Table2>())
			using (db.CreateLocalTable<Table3>())
			using (db.CreateLocalTable<Table4>())
			using (db.CreateLocalTable<Table5>())
			{
				var q =
					from t1 in db.GetTable<Table5>()
					from t2 in
						(from t3 in t1.Table4s.SelectMany(x => x.Table1s)
						 from t4 in
							from t5 in t3.Table4Ref!.Table5Ref.Table5Ref!.Table4s
							from t6 in t5.Table1s
							select t6
						 select t4.Field1)
					from t7 in
						(from t8 in t1.Table5Ref!.Table4s.SelectMany(x => x.Table1s)
						 from t9 in
							from t10 in t8.Table2Ref.Table3Ref.Table2s
							from t11 in t10.Table1s
							select t11
						 select t9.Field1)
					where t2 == t7
					select t7;

				var result = q.ToArray();

			}
		}
	}
}
