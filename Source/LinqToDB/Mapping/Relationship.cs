﻿namespace LinqToDB.Mapping
{
	/// <summary>
	/// Defines relationship types for associations.
	/// See <see cref="AssociationAttribute.Relationship"/> for more details.
	/// </summary>
	[Obsolete("This enum is not used by linq2db and will be removed in future")]
	public enum Relationship
	{
		/// <summary>
		/// One-to-one relationship.
		/// </summary>
		OneToOne,

		/// <summary>
		/// One-to-many relationship.
		/// </summary>
		OneToMany,

		/// <summary>
		/// Many-to-one relationship.
		/// </summary>
		ManyToOne,
	}
}
