﻿using System.Text;

namespace LinqToDB.SqlQuery
{
	public class SqlConditionalInsertClause : IQueryElement, ISqlExpressionWalkable
	{
		public SqlInsertClause     Insert { get; }
		public SqlSearchCondition? When   { get; }

		public SqlConditionalInsertClause(SqlInsertClause insert, SqlSearchCondition? when)
		{
			Insert = insert;
			When   = when;
		}

		#region ISqlExpressionWalkable

		ISqlExpression? ISqlExpressionWalkable.Walk<TContext>(WalkOptions options, TContext context, Func<TContext, ISqlExpression, ISqlExpression> func)
		{
			((ISqlExpressionWalkable?)When)?.Walk(options, context, func);

			((ISqlExpressionWalkable)Insert).Walk(options, context, func);

			return null;
		}

		#endregion

		#region IQueryElement

		QueryElementType IQueryElement.ElementType => QueryElementType.ConditionalInsertClause;

		StringBuilder IQueryElement.ToString(StringBuilder sb, Dictionary<IQueryElement, IQueryElement> dic)
		{
			if (When != null)
			{
				sb.Append("WHEN ");
				((IQueryElement)When).ToString(sb, dic);
				sb.AppendLine(" THEN");
			}

			((IQueryElement)Insert).ToString(sb, dic);

			return sb;
		}

		#endregion
	}
}
