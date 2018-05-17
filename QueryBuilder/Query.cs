using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public class Query
    {
        public bool IsDistinct { get; set; } = false;
        public string QueryAlias { get; set; }
        public string Method { get; set; } = "select";
        public List<AbstractClause> Clauses { get; set; } = new List<AbstractClause>();
        public string EngineScope = null;

        public Query SetEngineScope(string engine)
        {
            this.EngineScope = engine;

            // this.Clauses = this.Clauses.Select(x =>
            // {
            //     x.Engine = engine;
            //     return x;
            // }).ToList();

            return this;
        }

        /// <summary>
        /// Add a component clause to the query.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="clause"></param>
        /// <returns></returns>
        public Query AddComponent(string component, AbstractClause clause, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            clause.Engine = engineCode;
            clause.Component = component;
            Clauses.Add(clause);

            return this;
        }

        /// <summary>
        /// Get the list of clauses for a component.
        /// </summary>
        /// <returns></returns>
        public List<C> GetComponents<C>(string component, string engineCode = null) where C : AbstractClause
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            var clauses = Clauses
                .Where(x => x.Component == component)
                .Where(x => engineCode == null || x.Engine == null || engineCode == x.Engine)
                .Cast<C>();

            return clauses.ToList();
        }

        /// <summary>
        /// Get the list of clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public List<AbstractClause> GetComponents(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetComponents<AbstractClause>(component, engineCode);
        }

        /// <summary>
        /// Get a single component clause from the query.
        /// </summary>
        /// <returns></returns>
        public C GetOneComponent<C>(string component, string engineCode = null) where C : AbstractClause
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetComponents<C>(component, engineCode)
            .FirstOrDefault();
        }

        /// <summary>
        /// Get a single component clause from the query.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public AbstractClause GetOneComponent(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetOneComponent<AbstractClause>(component, engineCode);
        }

        /// <summary>
        /// Return wether the query has clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public bool HasComponent(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetComponents(component, engineCode).Any();
        }

        /// <summary>
        /// Remove all clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public Query ClearComponent(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            Clauses = Clauses
                .Where(x => !(x.Component == component && (engineCode == null || x.Engine == null || engineCode == x.Engine)))
                .ToList();

            return this;
        }

        public static object BackupNullValues(object x)
        {
            return x ?? new NullValue();
        }

        public static object RestoreNullValues(object x)
        {
            return x is NullValue ? null : x;
        }

        public List<string> operators = new List<string> {
            "=", "<", ">", "<=", ">=", "<>", "!=",
            "like", "like binary", "not like", "between", "ilike",
            "&", "|", "^", "<<", ">>",
            "rlike", "regexp", "not regexp",
            "~", "~*", "!~", "!~*", "similar to",
            "not similar to", "not ilike", "~~*", "!~~*",
        };

        public Query()
        {
        }

        public Query(string table)
        {
            this.From(table);
        }

        public Query Clone()
        {
            var clone = new Query().SetEngineScope(EngineScope);

            clone.Clauses = this.Clauses.Select(x => x.Clone()).ToList();

            clone.QueryAlias = QueryAlias;
            clone.IsDistinct = IsDistinct;
            clone.Method = Method;
            return clone;
        }

        public Query As(string alias)
        {
            QueryAlias = alias;
            return this;
        }

        public Query For(string engine, Func<Query, Query> fn)
        {
            EngineScope = engine;

            var result = fn.Invoke(this);

            // reset the engine
            EngineScope = null;

            return result;
        }

        public Query Limit(int value)
        {
            var clause = GetOneComponent("limit", EngineScope) as LimitOffset;

            if (clause != null)
            {
                clause.Limit = value;
                return this;
            }

            return AddComponent("limit", new LimitOffset
            {
                Limit = value
            });
        }

        public Query Offset(int value)
        {
            var clause = GetOneComponent("limit", EngineScope) as LimitOffset;

            if (clause != null)
            {
                clause.Offset = value;
                return this;
            }

            return AddComponent("limit", new LimitOffset
            {
                Offset = value
            });
        }

        /// <summary>
        /// Alias for Limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Query Take(int limit)
        {
            return Limit(limit);
        }

        /// <summary>
        /// Alias for Offset
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Query Skip(int offset)
        {
            return Offset(offset);
        }

        /// <summary>
        /// Set the limit and offset for a given page.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public Query ForPage(int page, int perPage = 15)
        {
            return Skip((page - 1) * perPage).Take(perPage);
        }

        public Query Distinct()
        {
            IsDistinct = true;
            return this;
        }

        /// <summary>
        /// Apply the callback's query changes if the given "condition" is true.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Query When(bool condition,
            Func<Query, Query> callback = null,
            Func<Query, Query> fallback = null)
        {
            var fn = condition ? callback : fallback;

            return fn is null ? this : fn(this);
        }

        /// <summary>
        /// Apply the callback's query changes if the given "condition" is false.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Query WhenNot(bool condition, Func<Query, Query> callback)
        {
            return When(condition, null, callback);
        }

    }
}
