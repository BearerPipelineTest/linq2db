﻿using System.Text;

namespace LinqToDB.SqlQuery
{
	public class SqlAliasPlaceholder : ISqlExpression
	{
		public QueryElementType ElementType => QueryElementType.SqlAliasPlaceholder;

		public StringBuilder ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
		{
			return sb.Append("%ts%");
		}

		public bool Equals(ISqlExpression? other)
		{
			return other != null && other.GetType() == GetType();
		}

		public ISqlExpression Walk<TContext>(WalkOptions options, TContext context, Func<TContext, ISqlExpression, ISqlExpression> func)
		{
			return this;
		}

		public bool Equals(ISqlExpression other, Func<ISqlExpression, ISqlExpression, bool> comparer)
		{
			return comparer(this, other);
		}

		public bool CanBeNull => false;
		public int Precedence => SqlQuery.Precedence.Primary;
		public Type SystemType => typeof(object);
	}
}
