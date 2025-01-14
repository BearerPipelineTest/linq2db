﻿namespace LinqToDB.DataProvider.Access
{
	using Configuration;

	[UsedImplicitly]
	class AccessFactory : IDataProviderFactory
	{
		IDataProvider IDataProviderFactory.GetDataProvider(IEnumerable<NamedValue> attributes)
		{
			return AccessTools.GetDataProvider();
		}
	}
}
