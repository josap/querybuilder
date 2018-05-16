using System;
using System.Linq;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public static class QueryFrom
    {
        public static Query From(this Query query, Expression expression, string alias = null)
        {
            return query.ClearComponent("from").AddComponent("from", new FromClause
            {
                Expression = expression,
                Alias = alias,
            });
        }
        /// <summary>
        /// Add a from Clause
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static Query From(this Query query, string table)
        {
            return query.From(new IdentifierExpression(table), table);
        }

        public static Query From(this Query query, Query fromQuery, string alias = null)
        {
            if (alias != null)
            {
                fromQuery.As(alias);
            };

            return query.From(new QueryExpression { Query = fromQuery });
        }

        public static Query From(this Query query, Func<Query, Query> callback, string alias = null)
        {
            return query.From(callback(new Query()), alias);
        }

        public static Query FromRaw(this Query query, string expression, params object[] bindings)
        {
            return query.From(new RawExpression(expression, bindings.ToList()));
        }



    }
}