namespace SqlKata.QueryBuilder
{
    public static class QueryHaving
    {
        public static Query Having(this Query query, string column, string op, object value)
        {
            return query.Having(q => q.Where(column, op, value));
        }

        public static Query Having(this Query query, string column, object value)
        {
            return query.Having(column, "=", value);
        }

        public static Query HavingNot(this Query query, string column, string op, object value)
        {
            return query.Having(q => q.WhereNot(column, op, value));
        }

        public static Query HavingNot(this Query query, string column, object value)
        {
            return query.HavingNot(column, "=", value);
        }

        public static Query OrHaving(this Query query, string column, string op, object value)
        {
            return query.Having(q => q.OrWhere(column, op, value));
        }

        public static Query OrHaving(this Query query, string column, object value)
        {
            return query.OrHaving(column, "=", value);
        }

        public static Query OrHavingNot(this Query query, string column, string op, object value)
        {
            return query.Having(q => q.OrWhereNot(column, op, value));
        }

        public static Query OrHavingNot(this Query query, string column, object value)
        {
            return query.OrWhereNot(column, "=", value);
        }

        public static Query HavingNull(this Query query, string column)
        {
            return query.Having(q => q.WhereNull(column));
        }

        public static Query HavingNotNull(this Query query, string column)
        {
            return query.Having(q => q.WhereNotNull(column));
        }

        public static Query OrHavingNull(this Query query, string column)
        {
            return query.Having(q => q.OrWhereNull(column));
        }
        public static Query OrHavingNotNull(this Query query, string column)
        {
            return query.Having(q => q.OrWhereNotNull(column));
        }
    }
}