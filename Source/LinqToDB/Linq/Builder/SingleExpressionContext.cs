﻿using System.Linq.Expressions;

namespace LinqToDB.Linq.Builder
{
	using SqlQuery;

	class SingleExpressionContext : IBuildContext
	{
		public SingleExpressionContext(IBuildContext? parent, ExpressionBuilder builder, SqlField sqlExpression, SelectQuery selectQuery)
		{
			Parent        = parent;
			Builder       = builder;
			SqlExpression = sqlExpression;
			SelectQuery   = selectQuery;

			Builder.Contexts.Add(this);
		}

#if DEBUG
		public string SqlQueryText => SelectQuery?.SqlText ?? "";
		public string Path         => this.GetPath();
#endif

		public IBuildContext?     Parent        { get; set; }
		public ExpressionBuilder  Builder       { get; set; }
		public ISqlExpression     SqlExpression { get; }
		public SelectQuery        SelectQuery   { get; set; }
		public SqlStatement?      Statement     { get; set; }
		Expression? IBuildContext.Expression    => null;

		public virtual void BuildQuery<T>(Query<T> query, ParameterExpression queryParameter)
		{
			var expr   = BuildExpression(null, 0, false);
			var mapper = Builder.BuildMapper<T>(expr);

			QueryRunner.SetRunQuery(query, mapper);
		}

		public Expression BuildExpression(Expression? expression, int level, bool enforceServerSide)
		{
			var info = ConvertToIndex(null, 0, ConvertFlags.All);
			if (info.Length != 1)
				ThrowHelper.ThrowInvalidOperationException();

			var parentIndex = ConvertToParentIndex(info[0].Index, this);
			return Builder.BuildSql(SqlExpression.SystemType ?? typeof(object), parentIndex, info[0].Sql);
		}

		public SqlInfo[] ConvertToSql(Expression? expression, int level, ConvertFlags flags)
		{
			return new[] { new SqlInfo(SqlExpression) };
		}

		public SqlInfo[] ConvertToIndex (Expression? expression, int level, ConvertFlags flags)
		{
			var idx = SelectQuery.Select.Add(SqlExpression);

			return new SqlInfo[] { new SqlInfo(SqlExpression, SelectQuery, idx) };
		}

		public IsExpressionResult IsExpression(Expression? expression, int level, RequestFor requestFlag)
		{
			if (requestFlag != RequestFor.Field)
			{
				return IsExpressionResult.False;
			}

			if (expression != null)
			{
				if (expression is ParameterExpression)
				{
					ThrowHelper.ThrowNotImplementedException();
				}
			}

			return ThrowHelper.ThrowNotImplementedException<IsExpressionResult>();
		}

		public IBuildContext? GetContext     (Expression? expression, int level, BuildInfo buildInfo)
		{
			return null;
		}

		public virtual SqlStatement GetResultStatement()
		{
			return ThrowHelper.ThrowNotImplementedException<SqlStatement>();
		}

		public void CompleteColumns()
		{
		}

		public virtual int ConvertToParentIndex(int index, IBuildContext context)
		{
			return Parent?.ConvertToParentIndex(index, this) ?? index;
		}

		public virtual void SetAlias(string? alias)
		{
		}

		public virtual ISqlExpression? GetSubQuery(IBuildContext context)
		{
			return null;
		}
	}
}
