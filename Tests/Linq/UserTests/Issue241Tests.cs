﻿using NUnit.Framework;

using Tests.Model;

namespace Tests.UserTests
{
	[TestFixture]
	public class Issue241Tests : TestBase
	{
		[Test]
		public void Test([NorthwindDataContext] string context)
		{
			using (new GuardGrouping(false))
			using (var db = new NorthwindDB(context))
			{
				var jj  = from o in db.Customer select o;
				jj      = jj.Where(x => x.CompanyName.Contains("t"));
				var t1g = jj.GroupBy(_ => _).ToDictionary(_ => _.Key, _ => _.ToList());

				Assert.GreaterOrEqual(t1g.Count, 1);
			}
		}
	}
}
