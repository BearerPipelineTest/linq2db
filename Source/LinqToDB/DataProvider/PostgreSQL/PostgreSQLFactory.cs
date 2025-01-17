﻿namespace LinqToDB.DataProvider.PostgreSQL
{
	using Configuration;

	[UsedImplicitly]
	class PostgreSQLFactory : IDataProviderFactory
	{
		IDataProvider IDataProviderFactory.GetDataProvider(IEnumerable<NamedValue> attributes)
		{
			return PostgreSQLTools.GetDataProvider();
		}
	}
}
