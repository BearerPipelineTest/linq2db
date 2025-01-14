// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.Default.MariaDB
{
	public partial class TestDataDB : DataConnection
	{
		public TestDataDB()
		{
			InitDataContext();
		}

		public TestDataDB(string configuration)
			: base(configuration)
		{
			InitDataContext();
		}

		public TestDataDB(LinqToDBConnectionOptions<TestDataDB> options)
			: base(options)
		{
			InitDataContext();
		}

		partial void InitDataContext();

		public ITable<Alltype>           Alltypes            => this.GetTable<Alltype>();
		public ITable<Alltypesnoyear>    Alltypesnoyears     => this.GetTable<Alltypesnoyear>();
		public ITable<Child>             Children            => this.GetTable<Child>();
		public ITable<Collatedtable>     Collatedtables      => this.GetTable<Collatedtable>();
		public ITable<Datatypetest>      Datatypetests       => this.GetTable<Datatypetest>();
		public ITable<Doctor>            Doctors             => this.GetTable<Doctor>();
		public ITable<Fulltextindextest> Fulltextindextests  => this.GetTable<Fulltextindextest>();
		public ITable<Grandchild>        Grandchildren       => this.GetTable<Grandchild>();
		public ITable<Inheritancechild>  Inheritancechildren => this.GetTable<Inheritancechild>();
		public ITable<Inheritanceparent> Inheritanceparents  => this.GetTable<Inheritanceparent>();
		public ITable<Issue1993>         Issue1993           => this.GetTable<Issue1993>();
		public ITable<Linqdatatype>      Linqdatatypes       => this.GetTable<Linqdatatype>();
		public ITable<Parent>            Parents             => this.GetTable<Parent>();
		public ITable<Patient>           Patients            => this.GetTable<Patient>();
		public ITable<Person>            People              => this.GetTable<Person>();
		public ITable<Testidentity>      Testidentities      => this.GetTable<Testidentity>();
		public ITable<Testmerge1>        Testmerge1          => this.GetTable<Testmerge1>();
		public ITable<Testmerge2>        Testmerge2          => this.GetTable<Testmerge2>();
		public ITable<Testsamename>      Testsamenames       => this.GetTable<Testsamename>();
		/// <summary>
		/// VIEW
		/// </summary>
		public ITable<Personview>        Personviews         => this.GetTable<Personview>();
	}

	public static partial class ExtensionMethods
	{
		#region Table Extensions
		public static Alltype? Find(this ITable<Alltype> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<Alltype?> FindAsync(this ITable<Alltype> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static Alltypesnoyear? Find(this ITable<Alltypesnoyear> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<Alltypesnoyear?> FindAsync(this ITable<Alltypesnoyear> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static Datatypetest? Find(this ITable<Datatypetest> table, int dataTypeId)
		{
			return table.FirstOrDefault(e => e.DataTypeId == dataTypeId);
		}

		public static Task<Datatypetest?> FindAsync(this ITable<Datatypetest> table, int dataTypeId, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.DataTypeId == dataTypeId, cancellationToken);
		}

		public static Doctor? Find(this ITable<Doctor> table, int personId)
		{
			return table.FirstOrDefault(e => e.PersonId == personId);
		}

		public static Task<Doctor?> FindAsync(this ITable<Doctor> table, int personId, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.PersonId == personId, cancellationToken);
		}

		public static Fulltextindextest? Find(this ITable<Fulltextindextest> table, uint id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<Fulltextindextest?> FindAsync(this ITable<Fulltextindextest> table, uint id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static Inheritancechild? Find(this ITable<Inheritancechild> table, int inheritanceChildId)
		{
			return table.FirstOrDefault(e => e.InheritanceChildId == inheritanceChildId);
		}

		public static Task<Inheritancechild?> FindAsync(this ITable<Inheritancechild> table, int inheritanceChildId, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.InheritanceChildId == inheritanceChildId, cancellationToken);
		}

		public static Inheritanceparent? Find(this ITable<Inheritanceparent> table, int inheritanceParentId)
		{
			return table.FirstOrDefault(e => e.InheritanceParentId == inheritanceParentId);
		}

		public static Task<Inheritanceparent?> FindAsync(this ITable<Inheritanceparent> table, int inheritanceParentId, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.InheritanceParentId == inheritanceParentId, cancellationToken);
		}

		public static Issue1993? Find(this ITable<Issue1993> table, uint id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<Issue1993?> FindAsync(this ITable<Issue1993> table, uint id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static Patient? Find(this ITable<Patient> table, int personId)
		{
			return table.FirstOrDefault(e => e.PersonId == personId);
		}

		public static Task<Patient?> FindAsync(this ITable<Patient> table, int personId, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.PersonId == personId, cancellationToken);
		}

		public static Person? Find(this ITable<Person> table, int personId)
		{
			return table.FirstOrDefault(e => e.PersonId == personId);
		}

		public static Task<Person?> FindAsync(this ITable<Person> table, int personId, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.PersonId == personId, cancellationToken);
		}

		public static Testidentity? Find(this ITable<Testidentity> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<Testidentity?> FindAsync(this ITable<Testidentity> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static Testmerge1? Find(this ITable<Testmerge1> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<Testmerge1?> FindAsync(this ITable<Testmerge1> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static Testmerge2? Find(this ITable<Testmerge2> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<Testmerge2?> FindAsync(this ITable<Testmerge2> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}

		public static Testsamename? Find(this ITable<Testsamename> table, int id)
		{
			return table.FirstOrDefault(e => e.Id == id);
		}

		public static Task<Testsamename?> FindAsync(this ITable<Testsamename> table, int id, CancellationToken cancellationToken = default)
		{
			return table.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		}
		#endregion
	}
}
