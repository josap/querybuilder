using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata.QueryBuilder
{
    public static class QueryInsert
    {
        public static Query AsInsert(this Query query, IEnumerable<string> columns, IEnumerable<object> values)
        {
            var columnsList = columns?.ToList();
            var valuesList = values?.Select(Query.BackupNullValues).ToList();

            if ((columnsList?.Count ?? 0) == 0 || (valuesList?.Count ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and Values cannot be null or empty");
            }

            if (columnsList.Count != valuesList.Count)
            {
                throw new InvalidOperationException("Columns count should be equal to Values count");
            }

            query.Method = "insert";

            return query.ClearComponent("insert").AddComponent("insert", new InsertClause
            {
                Columns = columnsList,
                Values = valuesList
            });

        }

        public static Query AsInsert(this Query query, IReadOnlyDictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
            {
                throw new InvalidOperationException("Values dictionary cannot be null or empty");
            }

            query.Method = "insert";

            return query.ClearComponent("insert").AddComponent("insert", new InsertClause
            {
                Columns = data.Keys.ToList(),
                Values = data.Values.Select(Query.BackupNullValues).ToList()
            });
        }

        /// <summary>
        /// Produces insert multi records
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="valuesCollection"></param>
        /// <returns></returns>
        public static Query AsInsert(this Query query, IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection)
        {
            var columnsList = columns?.ToList();
            var valuesCollectionList = valuesCollection?.ToList();

            if ((columnsList?.Count ?? 0) == 0 || (valuesCollectionList?.Count ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and valuesCollection cannot be null or empty");
            }

            query.Method = "insert";

            query.ClearComponent("insert");

            foreach (var values in valuesCollectionList)
            {
                var valuesList = values.Select(Query.BackupNullValues).ToList();
                if (columnsList.Count != valuesList.Count)
                {
                    throw new InvalidOperationException("Columns count should be equal to each Values count");
                }

                query.AddComponent("insert", new InsertClause
                {
                    Columns = columnsList,
                    Values = valuesList
                });
            }

            return query;
        }

        /// <summary>
        /// Produces insert from subquery
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Query AsInsert(this Query query, IEnumerable<string> columns, Query fromQuery)
        {
            query.Method = "insert";

            return query.ClearComponent("insert").AddComponent("insert", new InsertQueryClause
            {
                Columns = columns.ToList(),
                Query = fromQuery
            });

        }

    }
}