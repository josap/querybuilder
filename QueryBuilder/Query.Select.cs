using System;
using System.Linq;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public static class QuerySelect
    {
        public static Query Select(this Query query, Expression expression, string alias = null)
        {
            query.Method = "select";

            return query.AddComponent("columns", new SelectColumnClause
            {
                Expression = expression,
                Alias = alias,
            });
        }

        public static Query Select(this Query query, params string[] columns)
        {

            foreach (var column in columns)
            {
                query.Select(new IdentifierExpression(column), column);
            }

            return query;
        }

        /// <summary>
        /// Add a new "raw" select expression to the query.
        /// </summary>
        /// <returns></returns>
        public static Query SelectRaw(this Query query, string expression, params object[] bindings)
        {
            return query.Select(new RawExpression(expression, bindings.ToList()));
        }

        public static Query Select(this Query query, Query fromQuery, string alias = null)
        {
            if (alias != null)
            {
                fromQuery.As(alias);
            }

            var expression = new QueryExpression
            {
                Query = fromQuery,
            };

            return query.Select(expression, alias);
        }

        public static Query Select(this Query query, Func<Query, Query> callback, string alias = null)
        {
            return Select(callback(new Query()), alias);
        }
    }
}