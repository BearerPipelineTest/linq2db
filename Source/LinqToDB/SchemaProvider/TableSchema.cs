﻿namespace LinqToDB.SchemaProvider
{
	/// <summary>
	/// Describes table-like objects such as tables, views, procedure or function results.
	/// </summary>
	public class TableSchema
	{
		/// <summary>
		/// Gets unique table identifier, based on name, schema and database names.
		/// </summary>
		public string? ID                 { get; set; }
		/// <summary>
		/// Gets table database (catalog) name.
		/// </summary>
		public string? CatalogName        { get; set; }
		/// <summary>
		/// Gets table owner/schema name.
		/// </summary>
		public string? SchemaName         { get; set; }
		/// <summary>
		/// Gets database table name.
		/// </summary>
		public string? TableName          { get; set; }
		/// <summary>
		/// Gets table description.
		/// </summary>
		public string? Description        { get; set; }
		/// <summary>
		/// Gets flag indicating that table defined with default owner/schema or not.
		/// </summary>
		public bool    IsDefaultSchema    { get; set; }
		/// <summary>
		/// Gets flag indicating that table describes view.
		/// </summary>
		public bool    IsView             { get; set; }

		/// <summary>
		/// Gets flag indicating that table describes procedure or function result set.
		/// </summary>
		public bool    IsProcedureResult  { get; set; }
		/// <summary>
		/// Gets C# friendly table name.
		/// </summary>
		public string  TypeName           { get; set; } = null!;
		/// <summary>
		/// Gets flag indicating that it is not a user-defined table.
		/// </summary>
		public bool    IsProviderSpecific { get; set; }

		/// <summary>
		/// Gets list of table columns.
		/// </summary>
		public List<ColumnSchema>     Columns     { get; set; } = null!;
		/// <summary>
		/// Gets list of table foreign keys.
		/// </summary>
		public List<ForeignKeySchema> ForeignKeys { get; set; } = null!;
		/// <summary>
		/// Gets table group name. Used by T4 templates to group tables to generate Schema Type if <c>GenerateSchemaAsType</c> is <c>true</c>.
		/// If NULL, SchemaName is used.
		/// </summary>
		public string? GroupName { get; set; }
	}
}
