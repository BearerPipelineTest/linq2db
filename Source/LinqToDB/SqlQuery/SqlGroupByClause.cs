﻿using System.Text;

namespace LinqToDB.SqlQuery
{
	public enum GroupingType
	{
		Default,
		GroupBySets,
		Rollup,
		Cube
	}

	public class SqlGroupByClause : ClauseBase, IQueryElement, ISqlExpressionWalkable
	{

		internal SqlGroupByClause(SelectQuery selectQuery) : base(selectQuery)
		{
		}

		internal SqlGroupByClause(GroupingType groupingType, IEnumerable<ISqlExpression> items) : base(null)
		{
			GroupingType = groupingType;
			Items.AddRange(items);
		}

		public SqlGroupByClause Expr(ISqlExpression expr)
		{
			Add(expr);
			return this;
		}

		public SqlGroupByClause Field(SqlField field)
		{
			return Expr(field);
		}

		void Add(ISqlExpression expr)
		{
			foreach (var e in Items)
				if (e.Equals(expr))
					return;

			Items.Add(expr);
		}

		public GroupingType GroupingType  { get; set; } = GroupingType.Default;
		public List<ISqlExpression> Items { get; } = new List<ISqlExpression>();

		public bool IsEmpty => Items.Count == 0;

		public IEnumerable<ISqlExpression> EnumItems()
		{
			foreach (var item in Items)
			{
				if (item is SqlGroupingSet groupingSet)
				{
					foreach (var gropingSetItem in groupingSet.Items)
					{
						yield return gropingSetItem;
					}
				}
				else
				{
					yield return item;
				}
			}
		}

#if OVERRIDETOSTRING

		public override string ToString()
		{
			return ((IQueryElement)this).ToString(new StringBuilder(), new Dictionary<IQueryElement,IQueryElement>()).ToString();
		}

#endif

		#region ISqlExpressionWalkable Members

		ISqlExpression? ISqlExpressionWalkable.Walk<TContext>(WalkOptions options, TContext context, Func<TContext, ISqlExpression, ISqlExpression> func)
		{
			for (var i = 0; i < Items.Count; i++)
				Items[i] = Items[i].Walk(options, context, func)!;

			return null;
		}

		#endregion

		#region IQueryElement Members

		public QueryElementType ElementType => QueryElementType.GroupByClause;

		StringBuilder IQueryElement.ToString(StringBuilder sb, Dictionary<IQueryElement,IQueryElement> dic)
		{
			if (Items.Count == 0)
				return sb;

			sb.Append(" \nGROUP BY");
			sb.Append(
				GroupingType switch
				{
					GroupingType.Default     => "\n",
					GroupingType.GroupBySets => " GROUPING SETS (\n",
					GroupingType.Rollup      => " ROLLUP (\n",
					GroupingType.Cube        => " CUBE (\n",
					_ => ThrowHelper.ThrowInvalidOperationException<string>($"Unexpected grouping type: {GroupingType}"),
				});

			foreach (var item in Items)
			{
				sb.Append('\t');
				item.ToString(sb, dic);
				sb.Append(',');
			}

			sb.Length--;

			if (GroupingType != GroupingType.Default)
				sb.Append(')');

			return sb;
		}

		#endregion
	}
}
