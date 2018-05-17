using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata.QueryBuilder
{
    public static class QueryWhere
    {
        public static Query Where(this Query query, Func<ConditionBuilder, ConditionBuilder> callback, string boolean = "and")
        {
            return query.AddComponent("where", new ConditionClause
            {
                Expression = callback(new ConditionBuilder()).Evaluate(),
                Boolean = boolean
            });
        }

        public static Query Where(this Query query, string column, string op, object value)
        {
            return query.Where(q => q.Where(column, op, value), "and");
        }

        public static Query Where(this Query query, string column, object value)
        {
            return query.Where(q => q.Where(column, "=", value), "and");
        }

        public static Query WhereNot(this Query query, string column, string op, object value)
        {
            return query.Where(q => q.WhereNot(column, op, value), "and");
        }

        public static Query WhereNot(this Query query, string column, object value)
        {
            return query.WhereNot(column, "=", value);
        }

        public static Query OrWhere(this Query query, string column, string op, object value)
        {
            return query.Where(q => q.Where(column, op, value), "or");
        }

        public static Query OrWhere(this Query query, string column, object value)
        {
            return query.Where(q => q.Where(column, "=", value), "or");
        }

        public static Query OrWhereNot(this Query query, string column, string op, object value)
        {
            return query.Where(q => q.WhereNot(column, op, value), "or");
        }

        public static Query OrWhereNot(this Query query, string column, object value)
        {
            return query.Where(q => q.WhereNot(column, "=", value), "or");
        }

        public static Query WhereNull(this Query query, string column)
        {
            return query.Where(q => q.WhereNull(column), "and");
        }

        public static Query WhereNotNull(this Query query, string column)
        {
            return query.Where(q => q.WhereNotNull(column), "and");
        }

        public static Query OrWhereNull(this Query query, string column)
        {
            return query.Where(q => q.OrWhereNull(column), "or");
        }
        public static Query OrWhereNotNull(this Query query, string column)
        {
            return query.Where(q => q.OrWhereNotNull(column), "or");
        }

        public static Query WhereNested(this Query query, Func<ConditionBuilder, ConditionBuilder> callback)
        {
            return query.Where(q => q.Where(callback));
        }
    }
}