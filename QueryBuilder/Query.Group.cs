using System;
using System.Linq;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public static class QueryGroup
    {
        public static Query GroupBy(this Query query, Expression expression)
        {

            query.AddComponent("group", new GroupClause
            {
                Expression = expression
            });

            return query;
        }

        public static Query GroupBy(this Query query, params string[] columns)
        {
            foreach (var column in columns)
            {
                query.GroupBy(new IdentifierExpression(column));
            }

            return query;
        }

        public static Query GroupByRaw(this Query query, string expression, params object[] bindings)
        {
            return query.GroupBy(new RawExpression(expression, bindings.ToList()));
        }
    }
}