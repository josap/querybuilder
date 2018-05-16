using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata.QueryBuilder
{
    public static class QueryWhere
    {
        public static Query Where(this Query query, string column, string op, object value)
        {
            return query.Where(q => q.Where(column, op, value));
        }

        public static Query Where(this Query query, string column, object value)
        {
            return query.Where(column, "=", value);
        }

        public static Query WhereNot(this Query query, string column, string op, object value)
        {
            return query.Where(q => q.WhereNot(column, op, value));
        }

        public static Query WhereNot(this Query query, string column, object value)
        {
            return query.WhereNot(column, "=", value);
        }

        public static Query OrWhere(this Query query, string column, string op, object value)
        {
            return query.Where(q => q.OrWhere(column, op, value));
        }

        public static Query OrWhere(this Query query, string column, object value)
        {
            return query.OrWhere(column, "=", value);
        }

        public static Query OrWhereNot(this Query query, string column, string op, object value)
        {
            return query.Where(q => q.OrWhereNot(column, op, value));
        }

        public static Query OrWhereNot(this Query query, string column, object value)
        {
            return query.OrWhereNot(column, "=", value);
        }

        public static Query WhereNull(this Query query, string column)
        {
            return query.Where(q => q.WhereNull(column));
        }

        public static Query WhereNotNull(this Query query, string column)
        {
            return query.Where(q => q.WhereNotNull(column));
        }

        public static Query OrWhereNull(this Query query, string column)
        {
            return query.Where(q => q.OrWhereNull(column));
        }
        public static Query OrWhereNotNull(this Query query, string column)
        {
            return query.Where(q => q.OrWhereNotNull(column));
        }
    }
}