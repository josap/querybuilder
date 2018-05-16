using System;

namespace SqlKata.QueryBuilder
{
    public static class QueryCombine
    {

        public static Query Combine(this Query query, string operation, bool all, Query target)
        {
            if (query.Method != "select" || target.Method != "select")
            {
                throw new InvalidOperationException("Only select queries can be combined.");
            }

            return query.AddComponent("combine", new Combine
            {
                Query = target,
                Operation = operation,
                All = all,
            });
        }

        public static Query CombineRaw(this Query query, string sql, params object[] bindings)
        {
            if (query.Method != "select")
            {
                throw new InvalidOperationException("Only select queries can be combined.");
            }

            return query.AddComponent("combine", new RawCombine
            {
                Expression = sql,
                Bindings = Helper.Flatten(bindings).ToArray(),
            });
        }

        public static Query Union(this Query query, Query target, bool all = false)
        {
            return query.Combine("union", all, target);
        }

        public static Query UnionAll(this Query query, Query target)
        {
            return query.Union(target, true);
        }

        public static Query Union(this Query query, Func<Query, Query> callback, bool all = false)
        {
            var target = callback.Invoke(new Query());
            return query.Union(target, all);
        }

        public static Query UnionAll(this Query query, Func<Query, Query> callback)
        {
            return query.Union(callback, true);
        }

        public static Query UnionRaw(this Query query, string sql, params object[] bindings) => query.CombineRaw(sql, bindings);

        public static Query Except(this Query query, Query target, bool all = false)
        {
            return query.Combine("except", all, target);
        }

        public static Query ExceptAll(this Query query, Query target)
        {
            return query.Except(target, true);
        }

        public static Query Except(this Query query, Func<Query, Query> callback, bool all = false)
        {
            var target = callback.Invoke(new Query());
            return query.Except(target, all);
        }

        public static Query ExceptAll(this Query query, Func<Query, Query> callback)
        {
            return query.Except(callback, true);
        }
        public static Query ExceptRaw(this Query query, string sql, params object[] bindings) => query.CombineRaw(sql, bindings);

        public static Query Intersect(this Query query, Query target, bool all = false)
        {
            return query.Combine("intersect", all, target);
        }

        public static Query IntersectAll(this Query query, Query target)
        {
            return query.Intersect(target, true);
        }

        public static Query Intersect(this Query query, Func<Query, Query> callback, bool all = false)
        {
            var target = callback.Invoke(new Query());
            return query.Intersect(query, all);
        }

        public static Query IntersectAll(this Query query, Func<Query, Query> callback)
        {
            return query.Intersect(callback, true);
        }
        public static Query IntersectRaw(this Query query, string sql, params object[] bindings) => query.CombineRaw(sql, bindings);

    }
}