﻿namespace LinqToDB.Schema
{
	using SqlQuery;

	// TODO: add sequence load to schema API
	// TODO: add min/max/start/step data
	/// <summary>
	/// Sequence definition.
	/// </summary>
	/// <param name="Name">Optional sequence name.</param>
	public sealed record Sequence(SqlObjectName? Name)
	{
		public override string ToString() => Name?.ToString() ?? "<unnamed sequence>";
	}
}
