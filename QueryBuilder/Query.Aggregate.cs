using System.Linq;

namespace SqlKata.QueryBuilder
{
    public static class QueryAggregate
    {
        public static Query AsAggregate(this Query query, string type, params string[] columns)
        {

            query.Method = "aggregate";

            return query.ClearComponent("aggregate")
            .AddComponent("aggregate", new AggregateClause
            {
                Type = type,
                Columns = columns.ToList()
            });

        }

        public static Query AsCount(this Query query, params string[] columns)
        {
            var cols = columns.ToList();

            if (!cols.Any())
            {
                cols.Add("*");
            }

            return query.AsAggregate("count", cols.ToArray());
        }

        public static Query AsAvg(this Query query, string column)
        {
            return query.AsAggregate("avg", column);
        }
        public static Query AsAverage(this Query query, string column)
        {
            return query.AsAvg(column);
        }

        public static Query AsSum(this Query query, string column)
        {
            return query.AsAggregate("sum", column);
        }

        public static Query AsMax(this Query query, string column)
        {
            return query.AsAggregate("max", column);
        }

        public static Query AsMin(this Query query, string column)
        {
            return query.AsAggregate("min", column);
        }
    }
}