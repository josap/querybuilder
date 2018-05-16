using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata.QueryBuilder
{
    public static class QueryUpdate
    {


        public static Query AsUpdate(this Query query, IEnumerable<string> columns, IEnumerable<object> values)
        {

            if ((columns?.Count() ?? 0) == 0 || (values?.Count() ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and Values cannot be null or empty");
            }

            if (columns.Count() != values.Count())
            {
                throw new InvalidOperationException("Columns count should be equal to Values count");
            }

            query.Method = "update";

            return query.ClearComponent("update").AddComponent("update", new InsertClause
            {
                Columns = columns.ToList(),
                Values = values.Select(Query.BackupNullValues).ToList()
            });

        }

        public static Query AsUpdate(this Query query, IReadOnlyDictionary<string, object> data)
        {

            if (data == null || data.Count == 0)
            {
                throw new InvalidOperationException("Values dictionary cannot be null or empty");
            }

            query.Method = "update";

            return query.ClearComponent("update").AddComponent("update", new InsertClause
            {
                Columns = data.Keys.ToList(),
                Values = data.Values.Select(Query.BackupNullValues).ToList(),
            });

        }

    }
}