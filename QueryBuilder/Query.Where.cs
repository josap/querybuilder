using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata.QueryBuilder
{
    public static class QueryWhere
    {
        internal static Query addWhere(this Query query, Func<ConditionBuilder, ConditionBuilder> callback)
        {
            return query.AddComponent("where", new ConditionClause
            {
                Expression = callback(new ConditionBuilder()).Evaluate(),
                Boolean = "and"
            });
        }
        internal static Query addOrWhere(this Query query, Func<ConditionBuilder, ConditionBuilder> callback)
        {
            return query.AddComponent("where", new ConditionClause
            {
                Expression = callback(new ConditionBuilder()).Evaluate(),
                Boolean = "or"
            });
        }

        public static Query Where(this Query query, string column, string op, object value)
        {
            return query.addWhere(q => q.Where(column, op, value));
        }

        public static Query Where(this Query query, string column, object value)
        {
            return query.addWhere(q => q.Where(column, "=", value));
        }

        public static Query WhereNot(this Query query, string column, string op, object value)
        {
            return query.addWhere(q => q.WhereNot(column, op, value));
        }

        public static Query WhereNot(this Query query, string column, object value)
        {
            return query.WhereNot(column, "=", value);
        }

        public static Query OrWhere(this Query query, string column, string op, object value)
        {
            return query.addOrWhere(q => q.Where(column, op, value));
        }

        public static Query OrWhere(this Query query, string column, object value)
        {
            return query.addOrWhere(q => q.Where(column, "=", value));
        }

        public static Query OrWhereNot(this Query query, string column, string op, object value)
        {
            return query.addOrWhere(q => q.WhereNot(column, op, value));
        }

        public static Query OrWhereNot(this Query query, string column, object value)
        {
            return query.addOrWhere(q => q.WhereNot(column, "=", value));
        }

        public static Query WhereNull(this Query query, string column)
        {
            return query.addWhere(q => q.WhereNull(column));
        }

        public static Query WhereNotNull(this Query query, string column)
        {
            return query.addWhere(q => q.WhereNotNull(column));
        }

        public static Query OrWhereNull(this Query query, string column)
        {
            return query.addOrWhere(q => q.WhereNull(column));
        }
        public static Query OrWhereNotNull(this Query query, string column)
        {
            return query.addOrWhere(q => q.WhereNotNull(column));
        }

        public static Query Where(this Query query, Func<ConditionBuilder, ConditionBuilder> callback)
        {
            return query.addWhere(q => q.Where(callback));
        }

        public static Query WhereNot(this Query query, Func<ConditionBuilder, ConditionBuilder> callback)
        {
            return query.addWhere(q => q.WhereNot(callback));
        }

        public static Query OrWhere(this Query query, Func<ConditionBuilder, ConditionBuilder> callback)
        {
            return query.addOrWhere(q => q.Where(callback));
        }
        public static Query OrWhereNot(this Query query, Func<ConditionBuilder, ConditionBuilder> callback)
        {
            return query.addOrWhere(q => q.WhereNot(callback));
        }

        public static Query WhereIn<T>(this Query query, string column, IEnumerable<T> values)
        {
            return query.addWhere(q => q.WhereIn(column, values));
        }

        public static Query WhereNotIn<T>(this Query query, string column, IEnumerable<T> values)
        {
            return query.addWhere(q => q.WhereNotIn(column, values));
        }

        public static Query OrWhereIn<T>(this Query query, string column, IEnumerable<T> values)
        {
            return query.addOrWhere(q => q.WhereIn(column, values));
        }

        public static Query OrWhereNotIn<T>(this Query query, string column, IEnumerable<T> values)
        {
            return query.addOrWhere(q => q.WhereNotIn(column, values));
        }
    }
}