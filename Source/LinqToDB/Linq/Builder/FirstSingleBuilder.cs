﻿using System;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToDB.Linq.Builder
{
	using LinqToDB.Expressions;
	using Extensions;
	using SqlQuery;
	using Common;

	class FirstSingleBuilder : MethodCallBuilder
	{
		public  static readonly string[] MethodNames      = { "First"     , "FirstOrDefault"     , "Single"     , "SingleOrDefault"      };
		private static readonly string[] MethodNamesAsync = { "FirstAsync", "FirstOrDefaultAsync", "SingleAsync", "SingleOrDefaultAsync" };

		protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			return
				methodCall.IsQueryable     (MethodNames     ) && methodCall.Arguments.Count == 1 ||
				methodCall.IsAsyncExtension(MethodNamesAsync) && methodCall.Arguments.Count == 2;
		}

		protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
		{
			if (buildInfo.Parent != null)
			{
				builder.PushNestingQueryParent(buildInfo.Parent.SelectQuery);
			}

			var sequence = builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));

			if (buildInfo.Parent != null)
			{
				builder.PopNestingQueryParent();
			}

			var take     = 0;

			var forceOuter = buildInfo.Parent is DefaultIfEmptyBuilder.DefaultIfEmptyContext; 

			switch (methodCall.Method.Name)
			{
				case "First"                :
				case "FirstOrDefault"       :
				case "FirstAsync"           :
				case "FirstOrDefaultAsync"  :
					take = 1;
					break;

				case "Single"               :
				case "SingleOrDefault"      :
				case "SingleAsync"          :
				case "SingleOrDefaultAsync" :
					if (!buildInfo.IsSubQuery)
						if (buildInfo.SelectQuery.Select.TakeValue == null || buildInfo.SelectQuery.Select.TakeValue is SqlValue takeValue && (int)takeValue.Value! >= 2)
							take = 2;

					break;
			}

			if (take != 0)
			{
				var takeExpression = new SqlValue(take);
				builder.BuildTake(sequence, takeExpression, null);
			}

			var isOuter = false;

			if (forceOuter || methodCall.Method.Name.Contains("OrDefault"))
			{
				sequence = new DefaultIfEmptyBuilder.DefaultIfEmptyContext(buildInfo.Parent, sequence, null);
				isOuter  = true;
			}

			return new FirstSingleContext(buildInfo.Parent, sequence, methodCall, buildInfo.IsSubQuery, buildInfo.IsAssociation, isOuter);
		}

		protected override SequenceConvertInfo? Convert(
			ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo, ParameterExpression? param)
		{
			return null;
		}

		public class FirstSingleContext : SequenceContextBase
		{
			public FirstSingleContext(IBuildContext? parent, IBuildContext sequence, MethodCallExpression methodCall, bool isSubQuery, bool isAssociation, bool isOuter)
				: base(parent, sequence, null)
			{
				_methodCall   = methodCall;
				IsSubQuery    = isSubQuery;
				IsAssociation = isAssociation;
				IsOuter       = isOuter;
			}

			readonly MethodCallExpression _methodCall;

			public bool IsSubQuery    { get; }
			public bool IsAssociation { get; }
			public bool IsOuter       { get; }

			public override void BuildQuery<T>(Query<T> query, ParameterExpression queryParameter)
			{
				Sequence.BuildQuery(query, queryParameter);

				switch (_methodCall.Method.Name.Replace("Async", ""))
				{
					case "First"           : GetFirstElement          (query); break;
					case "FirstOrDefault"  : GetFirstOrDefaultElement (query); break;
					case "Single"          : GetSingleElement         (query); break;
					case "SingleOrDefault" : GetSingleOrDefaultElement(query); break;
				}
			}

			static void GetFirstElement<T>(Query<T> query)
			{
				query.GetElement      = (db, expr, ps, preambles) => query.GetIEnumerable(db, expr, ps, preambles).First();

				query.GetElementAsync = async (db, expr, ps, preambles, token) =>
				{
					var count = 0;
					var obj   = default(T)!;

					await query.GetForEachAsync(db, expr, ps, preambles,
						r => { obj = r; count++; return false; }, token).ConfigureAwait(Configuration.ContinueOnCapturedContext);

					return count > 0 ? obj : Array<T>.Empty.First();
				};
			}

			static void GetFirstOrDefaultElement<T>(Query<T> query)
			{
				query.GetElement      = (db, expr, ps, preambles) => query.GetIEnumerable(db, expr, ps, preambles).FirstOrDefault();

				query.GetElementAsync = async (db, expr, ps, preambles, token) =>
				{
					var count = 0;
					var obj   = default(T)!;

					await query.GetForEachAsync(db, expr, ps, preambles, r => { obj = r; count++; return false; }, token).ConfigureAwait(Configuration.ContinueOnCapturedContext);

					return count > 0 ? obj : Array<T>.Empty.FirstOrDefault();
				};
			}

			static void GetSingleElement<T>(Query<T> query)
			{
				query.GetElement      = (db, expr, ps, preambles) => query.GetIEnumerable(db, expr, ps, preambles).Single();

				query.GetElementAsync = async (db, expr, ps, preambles, token) =>
				{
					var count = 0;
					var obj   = default(T)!;

					await query.GetForEachAsync(db, expr, ps, preambles,
						r =>
						{
							if (count == 0)
								obj = r;
							count++;
							return count == 1;
						}, token).ConfigureAwait(Configuration.ContinueOnCapturedContext);

					return count == 1 ? obj : new T[count].Single();
				};
			}

			static void GetSingleOrDefaultElement<T>(Query<T> query)
			{
				query.GetElement      = (db, expr, ps, preambles) => query.GetIEnumerable(db, expr, ps, preambles).SingleOrDefault();

				query.GetElementAsync = async (db, expr, ps, preambles, token) =>
				{
					var count = 0;
					var obj   = default(T)!;

					await query.GetForEachAsync(db, expr, ps, preambles,
						r =>
						{
							if (count == 0)
								obj = r;
							count++;
							return count == 1;
						}, token).ConfigureAwait(Configuration.ContinueOnCapturedContext);

					return count == 1 ? obj : new T[count].SingleOrDefault();
				};
			}

			static object SequenceException()
			{
				return Array<object>.Empty.First();
			}

			bool _isJoinCreated;

			void CreateJoin()
			{
				if (!_isJoinCreated)
				{
					_isJoinCreated = true;

					var join = IsOuter ? SelectQuery.OuterApply() : SelectQuery.CrossApply();
					join.JoinedTable.IsWeak = true;

					Parent!.SelectQuery.From.Tables[0].Joins.Add(join.JoinedTable);
				}
			}

			int _checkNullIndex = -1;

			int GetCheckNullIndex()
			{
				if (_checkNullIndex < 0)
				{
					//TODO: Check maybe we have to use DefaultIfEmptyContext
					var q =
						from col in SelectQuery.Select.Columns
						where !col.CanBeNull
						select SelectQuery.Select.Columns.IndexOf(col);

					_checkNullIndex = q.DefaultIfEmpty(-1).First();

					if (_checkNullIndex < 0)
					{
						_checkNullIndex = SelectQuery.Select.Add(new SqlValue(1));
						SelectQuery.Select.Columns[_checkNullIndex].RawAlias = "is_empty";
					}

					_checkNullIndex = ConvertToParentIndex(_checkNullIndex, this);
				}

				return _checkNullIndex;
			}

			public override Expression BuildExpression(Expression? expression, int level, bool enforceServerSide)
			{
				if (expression == null || level == 0)
				{
					if (Builder.DataContext.SqlProviderFlags.IsApplyJoinSupported &&
						Parent!.SelectQuery.GroupBy.IsEmpty &&
						Parent.SelectQuery.From.Tables.Count > 0)
					{
						CreateJoin();

						var expr = Sequence.BuildExpression(expression, expression == null ? level : level + 1, enforceServerSide);

						Expression defaultValue;

						if (_methodCall.Method.Name.EndsWith("OrDefault"))
							defaultValue = Expression.Constant(expr.Type.GetDefaultValue(), expr.Type);
						else
							defaultValue = Expression.Convert(
								Expression.Call(
									null,
									MemberHelper.MethodOf(() => SequenceException())),
								expr.Type);

						expr = Expression.Condition(
							Expression.Call(
								ExpressionBuilder.DataReaderParam,
								ReflectionHelper.DataReader.IsDBNull,
								ExpressionInstances.Int32Array(GetCheckNullIndex())),
							defaultValue,
							expr);

						return expr;
					}

					if (expression == null)
					{
						if (   !Builder.DataContext.SqlProviderFlags.IsSubQueryColumnSupported 
						    || Sequence.IsExpression(null, level, RequestFor.Object).Result)
						{
							return Builder.BuildMultipleQuery(Parent!, _methodCall, ProjectFlags.SQL);
						}

						var idx = Parent!.SelectQuery.Select.Add(SelectQuery);
						    idx = Parent.ConvertToParentIndex(idx, Parent);
						return Builder.BuildSql(_methodCall.Type, idx, SelectQuery);
					}

					return null!; // ???
				}

				throw new NotImplementedException();
			}

			public override Expression MakeExpression(Expression path, ProjectFlags flags)
			{
				var projected = base.MakeExpression(path, flags);

				if (!flags.HasFlag(ProjectFlags.Test))
				{
					if (flags.HasFlag(ProjectFlags.SQL))
					{
						projected = Builder.ConvertToSqlExpr(Sequence, projected, flags);
					}

					if (IsSubQuery)
					{
						// Bad thing here. We expect that SelectQueryOptimizer will transfer OUTER APPLY to ROW_NUMBER query. We have to predict it here
						if (!Builder.DataContext.SqlProviderFlags.IsApplyJoinSupported && !Builder.DataContext.SqlProviderFlags.IsWindowFunctionsSupported)
						{
							var sqlProjected = Builder.MakeExpression(projected, ProjectFlags.Test);

							var placeholders = Builder.CollectDistinctPlaceholders(sqlProjected);

							if (placeholders.Count > 1)
							{
								if (flags.HasFlag(ProjectFlags.Expression))
									return new SqlEagerLoadExpression(this, path, Builder.GetSequenceExpression(this));
								return new SqlErrorExpression(this, path);
							}
						}

						CreateJoin();

						return projected;
					}
				}

				return projected;
			}

			public override SqlInfo[] ConvertToSql(Expression? expression, int level, ConvertFlags flags)
			{
				return Sequence.ConvertToSql(expression, level + 1, flags);
			}

			public override SqlInfo[] ConvertToIndex(Expression? expression, int level, ConvertFlags flags)
			{
				return Sequence.ConvertToIndex(expression, level, flags);
			}

			public override IsExpressionResult IsExpression(Expression? expression, int level, RequestFor requestFlag)
			{
				return Sequence.IsExpression(expression, level, requestFlag);
			}

			public override IBuildContext GetContext(Expression? expression, int level, BuildInfo buildInfo)
			{
				throw new NotImplementedException();
			}
		}
	}
}
