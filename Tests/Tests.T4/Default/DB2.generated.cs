﻿//---------------------------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated by T4Model template for T4 (https://github.com/linq2db/linq2db).
//    Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------------------------------------------------

#pragma warning disable 1573, 1591
#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Configuration;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Default.DB2
{
	public partial class TestDataDB : LinqToDB.Data.DataConnection
	{
		public ITable<DB2ADMIN_ALLTYPE>                ALLTYPES                { get { return this.GetTable<DB2ADMIN_ALLTYPE>(); } }
		public ITable<DB2ADMIN_Child>                  Children                { get { return this.GetTable<DB2ADMIN_Child>(); } }
		public ITable<DB2ADMIN_CollatedTable>          CollatedTables          { get { return this.GetTable<DB2ADMIN_CollatedTable>(); } }
		public ITable<DB2ADMIN_CreateIfNotExistsTable> CreateIfNotExistsTables { get { return this.GetTable<DB2ADMIN_CreateIfNotExistsTable>(); } }
		public ITable<DB2ADMIN_Doctor>                 Doctors                 { get { return this.GetTable<DB2ADMIN_Doctor>(); } }
		public ITable<DB2ADMIN_GrandChild>             GrandChildren           { get { return this.GetTable<DB2ADMIN_GrandChild>(); } }
		public ITable<DB2ADMIN_InheritanceChild>       InheritanceChildren     { get { return this.GetTable<DB2ADMIN_InheritanceChild>(); } }
		public ITable<DB2ADMIN_InheritanceParent>      InheritanceParents      { get { return this.GetTable<DB2ADMIN_InheritanceParent>(); } }
		public ITable<DB2ADMIN_Int>                    Ints                    { get { return this.GetTable<DB2ADMIN_Int>(); } }
		public ITable<DB2ADMIN_KeepIdentityTest>       KeepIdentityTests       { get { return this.GetTable<DB2ADMIN_KeepIdentityTest>(); } }
		public ITable<DB2ADMIN_LinqDataType>           LinqDataTypes           { get { return this.GetTable<DB2ADMIN_LinqDataType>(); } }
		public ITable<DB2ADMIN_MASTERTABLE>            Mastertables            { get { return this.GetTable<DB2ADMIN_MASTERTABLE>(); } }
		public ITable<DB2ADMIN_Parent>                 Parents                 { get { return this.GetTable<DB2ADMIN_Parent>(); } }
		public ITable<DB2ADMIN_Patient>                Patients                { get { return this.GetTable<DB2ADMIN_Patient>(); } }
		public ITable<DB2ADMIN_Person>                 People                  { get { return this.GetTable<DB2ADMIN_Person>(); } }
		public ITable<DB2ADMIN_PERSONVIEW>             Personviews             { get { return this.GetTable<DB2ADMIN_PERSONVIEW>(); } }
		public ITable<DB2ADMIN_SLAVETABLE>             Slavetables             { get { return this.GetTable<DB2ADMIN_SLAVETABLE>(); } }
		public ITable<DB2ADMIN_TagTestTable>           TagTestTables           { get { return this.GetTable<DB2ADMIN_TagTestTable>(); } }
		public ITable<DB2ADMIN_Test>                   Tests                   { get { return this.GetTable<DB2ADMIN_Test>(); } }
		public ITable<DB2ADMIN_TestIdentity>           TestIdentities          { get { return this.GetTable<DB2ADMIN_TestIdentity>(); } }
		public ITable<DB2ADMIN_Testmerge1>             Testmerge1              { get { return this.GetTable<DB2ADMIN_Testmerge1>(); } }
		public ITable<DB2ADMIN_TestMerge1>             TestMerge1              { get { return this.GetTable<DB2ADMIN_TestMerge1>(); } }
		public ITable<DB2ADMIN_Testmerge2>             Testmerge2              { get { return this.GetTable<DB2ADMIN_Testmerge2>(); } }
		public ITable<DB2ADMIN_TestMerge2>             TestMerge2              { get { return this.GetTable<DB2ADMIN_TestMerge2>(); } }

		public TestDataDB()
		{
			InitDataContext();
			InitMappingSchema();
		}

		public TestDataDB(string configuration)
			: base(configuration)
		{
			InitDataContext();
			InitMappingSchema();
		}

		public TestDataDB(LinqToDBConnectionOptions options)
			: base(options)
		{
			InitDataContext();
			InitMappingSchema();
		}

		public TestDataDB(LinqToDBConnectionOptions<TestDataDB> options)
			: base(options)
		{
			InitDataContext();
			InitMappingSchema();
		}

		partial void InitDataContext  ();
		partial void InitMappingSchema();

		#region Table Functions

		#region TestTableFunction

		[Sql.TableFunction(Schema="DB2ADMIN", Name="TEST_TABLE_FUNCTION")]
		public ITable<TestTableFUNCTIONResult> TestTableFunction(int? I)
		{
			return this.GetTable<TestTableFUNCTIONResult>(this, (MethodInfo)MethodBase.GetCurrentMethod()!,
				I);
		}

		public partial class TestTableFUNCTIONResult
		{
			public int? O { get; set; }
		}

		#endregion

		#endregion
	}

	[Table(Schema="DB2ADMIN", Name="ALLTYPES")]
	public partial class DB2ADMIN_ALLTYPE
	{
		[PrimaryKey, Identity] public int       ID                { get; set; } // INTEGER
		[Column,     Nullable] public long?     BIGINTDATATYPE    { get; set; } // BIGINT
		[Column,     Nullable] public int?      INTDATATYPE       { get; set; } // INTEGER
		[Column,     Nullable] public short?    SMALLINTDATATYPE  { get; set; } // SMALLINT
		[Column,     Nullable] public decimal?  DECIMALDATATYPE   { get; set; } // DECIMAL
		[Column,     Nullable] public decimal?  DECFLOATDATATYPE  { get; set; } // DECFLOAT(16)
		[Column,     Nullable] public float?    REALDATATYPE      { get; set; } // REAL
		[Column,     Nullable] public double?   DOUBLEDATATYPE    { get; set; } // DOUBLE
		[Column,     Nullable] public char?     CHARDATATYPE      { get; set; } // CHARACTER(1)
		[Column,     Nullable] public string?   CHAR20DATATYPE    { get; set; } // CHARACTER(20)
		[Column,     Nullable] public string?   VARCHARDATATYPE   { get; set; } // VARCHAR(20)
		[Column,     Nullable] public string?   CLOBDATATYPE      { get; set; } // CLOB(1048576)
		[Column,     Nullable] public string?   DBCLOBDATATYPE    { get; set; } // DBCLOB(100)
		[Column,     Nullable] public byte[]?   BINARYDATATYPE    { get; set; } // CHAR (5) FOR BIT DATA
		[Column,     Nullable] public byte[]?   VARBINARYDATATYPE { get; set; } // VARCHAR (5) FOR BIT DATA
		[Column,     Nullable] public byte[]?   BLOBDATATYPE      { get; set; } // BLOB(1048576)
		[Column,     Nullable] public string?   GRAPHICDATATYPE   { get; set; } // GRAPHIC(10)
		[Column,     Nullable] public DateTime? DATEDATATYPE      { get; set; } // DATE
		[Column,     Nullable] public TimeSpan? TIMEDATATYPE      { get; set; } // TIME
		[Column,     Nullable] public DateTime? TIMESTAMPDATATYPE { get; set; } // TIMESTAMP
		[Column,     Nullable] public string?   XMLDATATYPE       { get; set; } // XML
	}

	[Table(Schema="DB2ADMIN", Name="Child")]
	public partial class DB2ADMIN_Child
	{
		[Column, Nullable] public int? ParentID { get; set; } // INTEGER
		[Column, Nullable] public int? ChildID  { get; set; } // INTEGER
	}

	[Table(Schema="DB2ADMIN", Name="CollatedTable")]
	public partial class DB2ADMIN_CollatedTable
	{
		[Column, NotNull] public int    Id              { get; set; } // INTEGER
		[Column, NotNull] public string CaseSensitive   { get; set; } = null!; // VARCHAR(80)
		[Column, NotNull] public string CaseInsensitive { get; set; } = null!; // VARCHAR(80)
	}

	[Table(Schema="DB2ADMIN", Name="CreateIfNotExistsTable")]
	public partial class DB2ADMIN_CreateIfNotExistsTable
	{
		[Column, NotNull] public int Id    { get; set; } // INTEGER
		[Column, NotNull] public int Value { get; set; } // INTEGER
	}

	[Table(Schema="DB2ADMIN", Name="Doctor")]
	public partial class DB2ADMIN_Doctor
	{
		[PrimaryKey, NotNull] public int    PersonID { get; set; } // INTEGER
		[Column,     NotNull] public string Taxonomy { get; set; } = null!; // VARCHAR(50)

		#region Associations

		/// <summary>
		/// FK_Doctor_Person (DB2ADMIN.Person)
		/// </summary>
		[Association(ThisKey="PersonID", OtherKey="PersonID", CanBeNull=false)]
		public DB2ADMIN_Person Person { get; set; } = null!;

		#endregion
	}

	[Table(Schema="DB2ADMIN", Name="GrandChild")]
	public partial class DB2ADMIN_GrandChild
	{
		[Column, Nullable] public int? ParentID     { get; set; } // INTEGER
		[Column, Nullable] public int? ChildID      { get; set; } // INTEGER
		[Column, Nullable] public int? GrandChildID { get; set; } // INTEGER
	}

	[Table(Schema="DB2ADMIN", Name="InheritanceChild")]
	public partial class DB2ADMIN_InheritanceChild
	{
		[PrimaryKey, NotNull    ] public int     InheritanceChildId  { get; set; } // INTEGER
		[Column,     NotNull    ] public int     InheritanceParentId { get; set; } // INTEGER
		[Column,        Nullable] public int?    TypeDiscriminator   { get; set; } // INTEGER
		[Column,        Nullable] public string? Name                { get; set; } // VARCHAR(50)
	}

	[Table(Schema="DB2ADMIN", Name="InheritanceParent")]
	public partial class DB2ADMIN_InheritanceParent
	{
		[PrimaryKey, NotNull    ] public int     InheritanceParentId { get; set; } // INTEGER
		[Column,        Nullable] public int?    TypeDiscriminator   { get; set; } // INTEGER
		[Column,        Nullable] public string? Name                { get; set; } // VARCHAR(50)
	}

	[Table(Schema="DB2ADMIN", Name="Ints")]
	public partial class DB2ADMIN_Int
	{
		[Column, NotNull    ] public int  One   { get; set; } // INTEGER
		[Column, NotNull    ] public int  Two   { get; set; } // INTEGER
		[Column, NotNull    ] public int  Three { get; set; } // INTEGER
		[Column, NotNull    ] public int  Four  { get; set; } // INTEGER
		[Column, NotNull    ] public int  Five  { get; set; } // INTEGER
		[Column,    Nullable] public int? Nil   { get; set; } // INTEGER
	}

	[Table(Schema="DB2ADMIN", Name="KeepIdentityTest")]
	public partial class DB2ADMIN_KeepIdentityTest
	{
		[PrimaryKey, Identity] public int  ID    { get; set; } // INTEGER
		[Column,     Nullable] public int? Value { get; set; } // INTEGER
	}

	[Table(Schema="DB2ADMIN", Name="LinqDataTypes")]
	public partial class DB2ADMIN_LinqDataType
	{
		[Column, Nullable] public int?      ID             { get; set; } // INTEGER
		[Column, Nullable] public decimal?  MoneyValue     { get; set; } // DECIMAL(10,4)
		[Column, Nullable] public DateTime? DateTimeValue  { get; set; } // TIMESTAMP
		[Column, Nullable] public DateTime? DateTimeValue2 { get; set; } // TIMESTAMP
		[Column, Nullable] public short?    BoolValue      { get; set; } // SMALLINT
		[Column, Nullable] public byte[]?   GuidValue      { get; set; } // CHAR (16) FOR BIT DATA
		[Column, Nullable] public byte[]?   BinaryValue    { get; set; } // BLOB(5000)
		[Column, Nullable] public short?    SmallIntValue  { get; set; } // SMALLINT
		[Column, Nullable] public int?      IntValue       { get; set; } // INTEGER
		[Column, Nullable] public long?     BigIntValue    { get; set; } // BIGINT
		[Column, Nullable] public string?   StringValue    { get; set; } // VARCHAR(50)
	}

	[Table(Schema="DB2ADMIN", Name="MASTERTABLE")]
	public partial class DB2ADMIN_MASTERTABLE
	{
		[PrimaryKey(0), NotNull] public int ID1 { get; set; } // INTEGER
		[PrimaryKey(1), NotNull] public int ID2 { get; set; } // INTEGER

		#region Associations

		/// <summary>
		/// FK_SLAVETABLE_MASTERTABLE_BackReference (DB2ADMIN.SLAVETABLE)
		/// </summary>
		[Association(ThisKey="ID1, ID2", OtherKey="ID222222222222222222222222, ID1", CanBeNull=true)]
		public IEnumerable<DB2ADMIN_SLAVETABLE> Slavetables { get; set; } = null!;

		#endregion
	}

	[Table(Schema="DB2ADMIN", Name="Parent")]
	public partial class DB2ADMIN_Parent
	{
		[Column, Nullable] public int? ParentID { get; set; } // INTEGER
		[Column, Nullable] public int? Value1   { get; set; } // INTEGER
	}

	[Table(Schema="DB2ADMIN", Name="Patient")]
	public partial class DB2ADMIN_Patient
	{
		[PrimaryKey, NotNull] public int    PersonID  { get; set; } // INTEGER
		[Column,     NotNull] public string Diagnosis { get; set; } = null!; // VARCHAR(256)

		#region Associations

		/// <summary>
		/// FK_Patient_Person (DB2ADMIN.Person)
		/// </summary>
		[Association(ThisKey="PersonID", OtherKey="PersonID", CanBeNull=false)]
		public DB2ADMIN_Person Person { get; set; } = null!;

		#endregion
	}

	[Table(Schema="DB2ADMIN", Name="Person")]
	public partial class DB2ADMIN_Person
	{
		[PrimaryKey, Identity   ] public int     PersonID   { get; set; } // INTEGER
		[Column,     NotNull    ] public string  FirstName  { get; set; } = null!; // VARCHAR(50)
		[Column,     NotNull    ] public string  LastName   { get; set; } = null!; // VARCHAR(50)
		[Column,        Nullable] public string? MiddleName { get; set; } // VARCHAR(50)
		[Column,     NotNull    ] public char    Gender     { get; set; } // CHARACTER(1)

		#region Associations

		/// <summary>
		/// FK_Doctor_Person_BackReference (DB2ADMIN.Doctor)
		/// </summary>
		[Association(ThisKey="PersonID", OtherKey="PersonID", CanBeNull=true)]
		public DB2ADMIN_Doctor? Doctor { get; set; }

		/// <summary>
		/// FK_Patient_Person_BackReference (DB2ADMIN.Patient)
		/// </summary>
		[Association(ThisKey="PersonID", OtherKey="PersonID", CanBeNull=true)]
		public DB2ADMIN_Patient? Patient { get; set; }

		#endregion
	}

	[Table(Schema="DB2ADMIN", Name="PERSONVIEW", IsView=true)]
	public partial class DB2ADMIN_PERSONVIEW
	{
		[Column, NotNull    ] public int     PersonID   { get; set; } // INTEGER
		[Column, NotNull    ] public string  FirstName  { get; set; } = null!; // VARCHAR(50)
		[Column, NotNull    ] public string  LastName   { get; set; } = null!; // VARCHAR(50)
		[Column,    Nullable] public string? MiddleName { get; set; } // VARCHAR(50)
		[Column, NotNull    ] public char    Gender     { get; set; } // CHARACTER(1)
	}

	[Table(Schema="DB2ADMIN", Name="SLAVETABLE")]
	public partial class DB2ADMIN_SLAVETABLE
	{
		[Column(),                                NotNull] public int ID1                        { get; set; } // INTEGER
		[Column("ID 2222222222222222222222  22"), NotNull] public int ID222222222222222222222222 { get; set; } // INTEGER
		[Column("ID 2222222222222222"),           NotNull] public int ID2222222222222222         { get; set; } // INTEGER

		#region Associations

		/// <summary>
		/// FK_SLAVETABLE_MASTERTABLE (DB2ADMIN.MASTERTABLE)
		/// </summary>
		[Association(ThisKey="ID222222222222222222222222, ID1", OtherKey="ID1, ID2", CanBeNull=false)]
		public DB2ADMIN_MASTERTABLE MASTERTABLE { get; set; } = null!;

		#endregion
	}

	[Table(Schema="DB2ADMIN", Name="TagTestTable")]
	public partial class DB2ADMIN_TagTestTable
	{
		[Column, NotNull    ] public int     ID   { get; set; } // INTEGER
		[Column,    Nullable] public string? Name { get; set; } // VARCHAR(1020)
	}

	[Table(Schema="DB2ADMIN", Name="Test")]
	public partial class DB2ADMIN_Test
	{
		[Column, NotNull    ] public int  Id           { get; set; } // INTEGER
		[Column,    Nullable] public int? TestAnimalId { get; set; } // INTEGER
	}

	[Table(Schema="DB2ADMIN", Name="TestIdentity")]
	public partial class DB2ADMIN_TestIdentity
	{
		[PrimaryKey, Identity] public int ID { get; set; } // INTEGER
	}

	[Table(Schema="DB2ADMIN", Name="testmerge1")]
	public partial class DB2ADMIN_Testmerge1
	{
		[Column("id"),     PrimaryKey,  NotNull] public int  Id     { get; set; } // INTEGER
		[Column("field1"),    Nullable         ] public int? Field1 { get; set; } // INTEGER
		[Column("field2"),    Nullable         ] public int? Field2 { get; set; } // INTEGER
		[Column("field3"),    Nullable         ] public int? Field3 { get; set; } // INTEGER
		[Column("field4"),    Nullable         ] public int? Field4 { get; set; } // INTEGER
		[Column("field5"),    Nullable         ] public int? Field5 { get; set; } // INTEGER
	}

	[Table(Schema="DB2ADMIN", Name="TestMerge1")]
	public partial class DB2ADMIN_TestMerge1
	{
		[PrimaryKey, NotNull    ] public int       Id              { get; set; } // INTEGER
		[Column,        Nullable] public int?      Field1          { get; set; } // INTEGER
		[Column,        Nullable] public int?      Field2          { get; set; } // INTEGER
		[Column,        Nullable] public int?      Field3          { get; set; } // INTEGER
		[Column,        Nullable] public int?      Field4          { get; set; } // INTEGER
		[Column,        Nullable] public int?      Field5          { get; set; } // INTEGER
		[Column,        Nullable] public long?     FieldInt64      { get; set; } // BIGINT
		[Column,        Nullable] public short?    FieldBoolean    { get; set; } // SMALLINT
		[Column,        Nullable] public string?   FieldString     { get; set; } // VARCHAR(20)
		[Column,        Nullable] public string?   FieldNString    { get; set; } // VARCHAR(80)
		[Column,        Nullable] public char?     FieldChar       { get; set; } // CHARACTER(1)
		[Column,        Nullable] public string?   FieldNChar      { get; set; } // CHARACTER(4)
		[Column,        Nullable] public float?    FieldFloat      { get; set; } // REAL
		[Column,        Nullable] public double?   FieldDouble     { get; set; } // DOUBLE
		[Column,        Nullable] public DateTime? FieldDateTime   { get; set; } // TIMESTAMP
		[Column,        Nullable] public byte[]?   FieldBinary     { get; set; } // VARCHAR (20) FOR BIT DATA
		[Column,        Nullable] public byte[]?   FieldGuid       { get; set; } // CHAR (16) FOR BIT DATA
		[Column,        Nullable] public decimal?  FieldDecimal    { get; set; } // DECIMAL(24,10)
		[Column,        Nullable] public DateTime? FieldDate       { get; set; } // DATE
		[Column,        Nullable] public TimeSpan? FieldTime       { get; set; } // TIME
		[Column,        Nullable] public string?   FieldEnumString { get; set; } // VARCHAR(20)
		[Column,        Nullable] public int?      FieldEnumNumber { get; set; } // INTEGER
	}

	[Table(Schema="DB2ADMIN", Name="testmerge2")]
	public partial class DB2ADMIN_Testmerge2
	{
		[Column("id"),     PrimaryKey,  NotNull] public int  Id     { get; set; } // INTEGER
		[Column("field1"),    Nullable         ] public int? Field1 { get; set; } // INTEGER
		[Column("field2"),    Nullable         ] public int? Field2 { get; set; } // INTEGER
		[Column("field3"),    Nullable         ] public int? Field3 { get; set; } // INTEGER
		[Column("field4"),    Nullable         ] public int? Field4 { get; set; } // INTEGER
		[Column("field5"),    Nullable         ] public int? Field5 { get; set; } // INTEGER
	}

	[Table(Schema="DB2ADMIN", Name="TestMerge2")]
	public partial class DB2ADMIN_TestMerge2
	{
		[PrimaryKey, NotNull    ] public int       Id              { get; set; } // INTEGER
		[Column,        Nullable] public int?      Field1          { get; set; } // INTEGER
		[Column,        Nullable] public int?      Field2          { get; set; } // INTEGER
		[Column,        Nullable] public int?      Field3          { get; set; } // INTEGER
		[Column,        Nullable] public int?      Field4          { get; set; } // INTEGER
		[Column,        Nullable] public int?      Field5          { get; set; } // INTEGER
		[Column,        Nullable] public long?     FieldInt64      { get; set; } // BIGINT
		[Column,        Nullable] public short?    FieldBoolean    { get; set; } // SMALLINT
		[Column,        Nullable] public string?   FieldString     { get; set; } // VARCHAR(20)
		[Column,        Nullable] public string?   FieldNString    { get; set; } // VARCHAR(80)
		[Column,        Nullable] public char?     FieldChar       { get; set; } // CHARACTER(1)
		[Column,        Nullable] public string?   FieldNChar      { get; set; } // CHARACTER(4)
		[Column,        Nullable] public float?    FieldFloat      { get; set; } // REAL
		[Column,        Nullable] public double?   FieldDouble     { get; set; } // DOUBLE
		[Column,        Nullable] public DateTime? FieldDateTime   { get; set; } // TIMESTAMP
		[Column,        Nullable] public byte[]?   FieldBinary     { get; set; } // VARCHAR (20) FOR BIT DATA
		[Column,        Nullable] public byte[]?   FieldGuid       { get; set; } // CHAR (16) FOR BIT DATA
		[Column,        Nullable] public decimal?  FieldDecimal    { get; set; } // DECIMAL(24,10)
		[Column,        Nullable] public DateTime? FieldDate       { get; set; } // DATE
		[Column,        Nullable] public TimeSpan? FieldTime       { get; set; } // TIME
		[Column,        Nullable] public string?   FieldEnumString { get; set; } // VARCHAR(20)
		[Column,        Nullable] public int?      FieldEnumNumber { get; set; } // INTEGER
	}

	public static partial class TestDataDBStoredProcedures
	{
		#region TestMODULE1TestProcedure

		public static int TestMODULE1TestProcedure(this TestDataDB dataConnection, int? I)
		{
			var parameters = new []
			{
				new DataParameter("I", I, LinqToDB.DataType.Int32)
			};

			return dataConnection.ExecuteProc("DB2ADMIN.TEST_MODULE1.TEST_PROCEDURE", parameters);
		}

		#endregion

		#region TestMODULE2TestProcedure

		public static int TestMODULE2TestProcedure(this TestDataDB dataConnection, int? I)
		{
			var parameters = new []
			{
				new DataParameter("I", I, LinqToDB.DataType.Int32)
			};

			return dataConnection.ExecuteProc("DB2ADMIN.TEST_MODULE2.TEST_PROCEDURE", parameters);
		}

		#endregion

		#region ADDISSUE792RECORD

		public static int ADDISSUE792RECORD(this TestDataDB dataConnection)
		{
			return dataConnection.ExecuteProc("DB2ADMIN.ADDISSUE792RECORD");
		}

		#endregion

		#region PersonSelectbykey

		public static int PersonSelectbykey(this TestDataDB dataConnection, int? ID)
		{
			var parameters = new []
			{
				new DataParameter("ID", ID, LinqToDB.DataType.Int32)
			};

			return dataConnection.ExecuteProc("DB2ADMIN.PERSON_SELECTBYKEY", parameters);
		}

		#endregion

		#region TestProcedure

		public static int TestProcedure(this TestDataDB dataConnection, int? I)
		{
			var parameters = new []
			{
				new DataParameter("I", I, LinqToDB.DataType.Int32)
			};

			return dataConnection.ExecuteProc("DB2ADMIN.TEST_PROCEDURE", parameters);
		}

		#endregion
	}

	public static partial class SqlFunctions
	{
		#region TestMODULE1TestFunction

		[Sql.Function(Name="DB2ADMIN.TEST_MODULE1.TEST_FUNCTION", ServerSideOnly=true)]
		public static int? TestMODULE1TestFunction(int? I)
		{
			throw new InvalidOperationException();
		}

		#endregion

		#region TestMODULE2TestFunction

		[Sql.Function(Name="DB2ADMIN.TEST_MODULE2.TEST_FUNCTION", ServerSideOnly=true)]
		public static int? TestMODULE2TestFunction(int? I)
		{
			throw new InvalidOperationException();
		}

		#endregion

		#region TestFunction

		[Sql.Function(Name="DB2ADMIN.TEST_FUNCTION", ServerSideOnly=true)]
		public static int? TestFunction(int? I)
		{
			throw new InvalidOperationException();
		}

		#endregion
	}

	public static partial class TableExtensions
	{
		public static DB2ADMIN_ALLTYPE? Find(this ITable<DB2ADMIN_ALLTYPE> table, int ID)
		{
			return table.FirstOrDefault(t =>
				t.ID == ID);
		}

		public static DB2ADMIN_Doctor? Find(this ITable<DB2ADMIN_Doctor> table, int PersonID)
		{
			return table.FirstOrDefault(t =>
				t.PersonID == PersonID);
		}

		public static DB2ADMIN_InheritanceChild? Find(this ITable<DB2ADMIN_InheritanceChild> table, int InheritanceChildId)
		{
			return table.FirstOrDefault(t =>
				t.InheritanceChildId == InheritanceChildId);
		}

		public static DB2ADMIN_InheritanceParent? Find(this ITable<DB2ADMIN_InheritanceParent> table, int InheritanceParentId)
		{
			return table.FirstOrDefault(t =>
				t.InheritanceParentId == InheritanceParentId);
		}

		public static DB2ADMIN_KeepIdentityTest? Find(this ITable<DB2ADMIN_KeepIdentityTest> table, int ID)
		{
			return table.FirstOrDefault(t =>
				t.ID == ID);
		}

		public static DB2ADMIN_MASTERTABLE? Find(this ITable<DB2ADMIN_MASTERTABLE> table, int ID1, int ID2)
		{
			return table.FirstOrDefault(t =>
				t.ID1 == ID1 &&
				t.ID2 == ID2);
		}

		public static DB2ADMIN_Patient? Find(this ITable<DB2ADMIN_Patient> table, int PersonID)
		{
			return table.FirstOrDefault(t =>
				t.PersonID == PersonID);
		}

		public static DB2ADMIN_Person? Find(this ITable<DB2ADMIN_Person> table, int PersonID)
		{
			return table.FirstOrDefault(t =>
				t.PersonID == PersonID);
		}

		public static DB2ADMIN_TestIdentity? Find(this ITable<DB2ADMIN_TestIdentity> table, int ID)
		{
			return table.FirstOrDefault(t =>
				t.ID == ID);
		}

		public static DB2ADMIN_Testmerge1? Find(this ITable<DB2ADMIN_Testmerge1> table, int Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static DB2ADMIN_TestMerge1? Find(this ITable<DB2ADMIN_TestMerge1> table, int Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static DB2ADMIN_Testmerge2? Find(this ITable<DB2ADMIN_Testmerge2> table, int Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static DB2ADMIN_TestMerge2? Find(this ITable<DB2ADMIN_TestMerge2> table, int Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}
	}
}
