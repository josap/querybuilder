using System;
using System.Linq;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public static class QueryCte
    {
        public static Query With(this Query query, string alias, Expression expression)
        {
            return query.AddComponent("cte", new FromClause
            {
                Expression = expression,
                Alias = alias,
            });
        }

        public static Query With(this Query query, string alias, Query cte)
        {
            var expression = new QueryExpression
            {
                Query = cte
            };

            return query.With(alias, expression);
        }

        public static Query With(this Query query, string alias, Func<Query, Query> fn)
        {
            return query.With(alias, fn(new Query()));
        }

        public static Query WithRaw(this Query query, string alias, string expression, params object[] bindings)
        {
            return query.With(alias, new RawExpression(expression, bindings.ToList()));
        }

    }
}