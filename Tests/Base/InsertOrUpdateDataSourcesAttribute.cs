﻿namespace Tests
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class InsertOrUpdateDataSourcesAttribute : DataSourcesAttribute
	{
		public static List<string> Unsupported = new List<string>
		{
			TestProvName.AllClickHouse
		}.SelectMany(_ => _.Split(',')).ToList();

		public InsertOrUpdateDataSourcesAttribute(params string[] except)
			: base(true, Unsupported.Concat(except.SelectMany(_ => _.Split(','))).ToArray())
		{
		}

		public InsertOrUpdateDataSourcesAttribute(bool includeLinqService, params string[] except)
			: base(includeLinqService, Unsupported.Concat(except.SelectMany(_ => _.Split(','))).ToArray())
		{
		}
	}
}
