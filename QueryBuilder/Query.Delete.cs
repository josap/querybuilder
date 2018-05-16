namespace SqlKata.QueryBuilder
{
    public static class QueryDelete
    {
        public static Query AsDelete(this Query query)
        {
            query.Method = "delete";
            return query;
        }

    }
}