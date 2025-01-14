﻿namespace LinqToDB.DataProvider.Informix
{
	using System.Collections.Generic;
	using Configuration;

	[UsedImplicitly]
	class InformixFactory : IDataProviderFactory
	{
		IDataProvider IDataProviderFactory.GetDataProvider(IEnumerable<NamedValue> attributes)
		{
			return InformixTools.GetDataProvider();
		}
	}
}
