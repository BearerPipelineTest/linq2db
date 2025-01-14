﻿using System.Linq.Expressions;
using LinqToDB.Expressions;

namespace LinqToDB.Linq.Builder
{
	using SqlQuery;

	internal partial class MergeBuilder
	{
		private class MergeContext : SequenceContextBase
		{
			private readonly ISet<Expression> _sourceParameters = new HashSet<Expression>();
			private readonly ISet<Expression> _targetParameters = new HashSet<Expression>();

			public void AddSourceParameter(Expression param)
			{
				_sourceParameters.Add(param);
			}

			public void AddTargetParameter(Expression param)
			{
				_targetParameters.Add(param);
			}

			public MergeContext(SqlMergeStatement merge, IBuildContext target)
				: base(null, target, null)
			{
				Statement = merge;
			}

			public MergeContext(SqlMergeStatement merge, IBuildContext target, TableLikeQueryContext source)
				: base(null, new[] { target, source }, null)
			{
				Statement    = merge;
				merge.Source = source.Source;
			}

			public SqlMergeStatement Merge => (SqlMergeStatement)Statement!;

			public IBuildContext           TargetContext => Sequence;
			public TableLikeQueryContext SourceContext => (TableLikeQueryContext)Sequences[1];

			public override void BuildQuery<T>(Query<T> query, ParameterExpression queryParameter)
			{
				QueryRunner.SetNonQueryQuery(query);
			}

			public override Expression BuildExpression(Expression? expression, int level, bool enforceServerSide)
			{
				return ThrowHelper.ThrowNotImplementedException<Expression>();
			}

			public override SqlInfo[] ConvertToIndex(Expression? expression, int level, ConvertFlags flags)
			{
				return ThrowHelper.ThrowNotImplementedException<SqlInfo[]>();
			}

			public override SqlInfo[] ConvertToSql(Expression? expression, int level, ConvertFlags flags)
			{
				if (expression != null)
				{
					switch (flags)
					{
						case ConvertFlags.Field:
						{
							var root = Builder.GetRootObject(expression);

							if (root.NodeType == ExpressionType.Parameter)
							{
								if (_sourceParameters.Contains(root))
									return SourceContext.ConvertToSql(expression, level, flags);

								return TargetContext.ConvertToSql(expression, level, flags);
							}

							if (root is ContextRefExpression contextRef)
							{
								return contextRef.BuildContext.ConvertToSql(expression, level, flags);
							}

							break;
						}
					}
				}

				return ThrowHelper.ThrowLinqException<SqlInfo[]>($"'{expression}' cannot be converted to SQL.");
			}

			public override IBuildContext GetContext(Expression? expression, int level, BuildInfo buildInfo)
			{
				return ThrowHelper.ThrowNotImplementedException<IBuildContext>();
			}

			public override IsExpressionResult IsExpression(Expression? expression, int level, RequestFor requestFlag)
			{
				return SourceContext.IsExpression(expression, level, requestFlag);
			}
		}
	}
}
