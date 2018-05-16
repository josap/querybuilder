using System;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public static class QueryJoin
    {
        public static Query Join(this Query query, Expression expression, ConditionBuilder conditionBuilder, string type = "inner")
        {
            return query.AddComponent("join", new JoinClause
            {
                Type = type,
                Expression = expression,
                Conditions = conditionBuilder.Evaluate(),
            });
        }
        public static Query Join(this Query query, string table, ConditionBuilder conditionBuilder, string type = "inner")
        {
            return query.Join(new IdentifierExpression(table), conditionBuilder, type);
        }

        public static Query Join(this Query query, string table, Func<ConditionBuilder, ConditionBuilder> callback, string type = "inner")
        {
            return query.Join(new IdentifierExpression(table), callback(new ConditionBuilder()), type);
        }

        public static Query Join(this Query query, string table, string left, string op, string right, string type = "inner")
        {
            var conditions = new ConditionBuilder().WhereColumns(left, op, right);
            return query.Join(table, conditions, type);
        }

        public static Query Join(this Query query, string table, string left, string right, string type = "inner")
        {
            return query.Join(table, left, "=", right, type);
        }

        public static Query Join(this Query query, Query join, ConditionBuilder conditionBuilder, string type = "inner")
        {
            return query.Join(new QueryExpression { Query = query }, conditionBuilder, type);
        }
        public static Query Join(this Query query, Query join, Func<ConditionBuilder, ConditionBuilder> callback, string type = "inner")
        {
            return query.Join(new QueryExpression { Query = query }, callback(new ConditionBuilder()), type);
        }
    }
}