using System;
using System.Linq;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public static class QueryOrder
    {
        public static Query OrderBy(this Query query, Expression expression, bool descending = false)
        {
            return query.AddComponent("order", new OrderByClause
            {
                Expression = expression,
                Descending = descending,
            });
        }

        public static Query OrderBy(this Query query, params string[] columns)
        {
            foreach (var column in columns)
            {
                query.OrderBy(new IdentifierExpression(column));
            }

            return query;
        }

        public static Query OrderByDesc(this Query query, params string[] columns)
        {
            foreach (var column in columns)
            {
                query.OrderBy(new IdentifierExpression(column), true);
            }

            return query;
        }

        public static Query OrderByRaw(this Query query, string expression, params object[] bindings)
        {
            return query.OrderBy(new RawExpression(expression, bindings.ToList()));
        }

        public static Query OrderByRandom(this Query query, string seed)
        {
            return query.OrderBy(new FunctionExpression("random", new ConstantExpression(seed)));
        }

    }
}