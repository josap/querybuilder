using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder.Compilers
{

    public partial class Compiler
    {
        public string EngineCode;

        /// The list of bindings for the current compilation
        protected List<object> bindings = new List<object>();

        protected string OpeningIdentifier = "\"";
        protected string ClosingIdentifier = "\"";

        public Compiler()
        {
        }

        public virtual string CompileFalse()
        {
            return "false";
        }

        public virtual string CompileTrue()
        {
            return "true";
        }

        public virtual string CompileExpression(Expression expression)
        {
            if (expression is LiteralExpression literal)
            {
                bindings.Add(literal.Value);
                return "?";
            }

            if (expression is ConstantExpression constant)
            {
                return constant.Value;
            }

            if (expression is IdentifierExpression identifier)
            {
                return WrapIdentifier(identifier.Value);
            }

            if (expression is BooleanExpression boolExpr)
            {
                return $"{CompileExpression(boolExpr.Left)} {boolExpr.Operator} {CompileExpression(boolExpr.Right)}";
            }

            if (expression is NestedExpression nested)
            {
                return $"({CompileExpression(nested.Expression)})";
            }

            if (expression is FalseExpression)
            {
                return CompileFalse();
            }

            if (expression is TrueExpression)
            {
                return CompileTrue();
            }

            if (expression is NullExpression)
            {
                return "null";
            }

            if (expression is NotExpression notExpr)
            {
                return $"not {CompileExpression(notExpr.Expression)}";
            }

            if (expression is QueryExpression queryExpr)
            {
                return ""; //Compile(queryExpr.Query);
            }

            if (expression is RawExpression raw)
            {
                if (raw.Bindings != null)
                {
                    bindings.AddRange(raw.Bindings);
                }

                return raw.Expression;
            }

            if (expression is FunctionExpression func)
            {
                var args = func.Arguments.
                    Select(x => CompileExpression(x));

                return $"{func.Name}(${string.Join(", ", args)})";
            }

            throw new InvalidOperationException("No compiler found for expression of type " + expression.GetType().FullName);
        }

        public virtual string Compile(Query query)
        {

            string sql = "";

            // Handle CTEs
            if (query.HasComponent("cte", EngineCode))
            {
                sql += CompileCte(query) + "\n";
            }

            if (query.Method == "insert")
            {
                // sql += CompileInsert(query);
            }
            else if (query.Method == "delete")
            {
                // sql += CompileDelete(query);
            }
            else if (query.Method == "update")
            {
                // sql += CompileUpdate(query);
            }
            else if (query.Method == "aggregate")
            {
                query.ClearComponent("limit")
                    .ClearComponent("select")
                    .ClearComponent("group")
                    .ClearComponent("order");

                sql += CompileSelect(query);
            }
            else
            {
                sql += CompileSelect(query);
            }

            return sql;
        }

        protected virtual Query OnBeforeCompile(Query query)
        {
            return query;
        }

        public virtual string OnAfterCompile(string sql, List<object> bindings)
        {
            return sql;
        }

        public virtual string CompileSelect(Query query)
        {

            var results = new[] {
                    this.CompileColumns(query),
                    this.CompileFrom(query),
                    // this.CompileJoins(query),
                    this.CompileWheres(query),
                    this.CompileGroups(query),
                    this.CompileHavings(query),
                    this.CompileOrders(query),
                    this.CompileLimit(query),
                    this.CompileOffset(query),
                    this.CompileLock(query),
                }
               .Where(x => x != null)
               .Select(x => x.Trim())
               .Where(x => !string.IsNullOrEmpty(x))
               .ToList();

            string sql = "select " + string.Join(" ", results);

            // Handle UNION, EXCEPT and INTERSECT
            if (query.GetComponents("combine", EngineCode).Any())
            {
                var combinedQueries = new List<string>();

                var clauses = query.GetComponents<AbstractCombine>("combine", EngineCode);

                combinedQueries.Add("(" + sql + ")");

                foreach (var clause in clauses)
                {
                    if (clause is Combine combineClause)
                    {
                        var combineOperator = combineClause.Operation.ToUpper() + " " + (combineClause.All ? "ALL " : "");

                        var compiled = CompileSelect(combineClause.Query);

                        combinedQueries.Add($"{combineOperator}({compiled})");
                    }
                    else
                    {
                        var combineRawClause = clause as RawCombine;
                        combinedQueries.Add(WrapIdentifier(combineRawClause.Expression));
                    }
                }

                sql = string.Join(" ", combinedQueries);

            }

            return sql;
        }

        /// <summary>
        /// Compile a single column clause
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public virtual string CompileColumn(SelectColumnClause column)
        {
            return $"{CompileExpression(column.Expression)} as {column.Alias}";
        }

        protected virtual string CompileColumns(Query query)
        {
            var columns = query.GetComponents<SelectColumnClause>("columns", EngineCode);

            if (!columns.Any())
            {
                return query.IsDistinct ? "distinct *" : "*";
            }

            var sql = columns.Select(x => CompileColumn(x));

            return (query.IsDistinct ? "distinct " : "") + sql;
        }

        public virtual string CompileFrom(Query query)
        {
            if (!query.HasComponent("from", EngineCode))
            {
                return null;
            }

            var expression = query.GetOneComponent<FromClause>("from", EngineCode);

            var table = CompileExpression(expression.Expression);
            var alias = WrapIdentifier(expression.Alias);

            if (table != alias)
            {
                return "from " + table + $" as {alias}";
            }

            return "from " + table;
        }

        public virtual string CompileConditions(IEnumerable<ConditionClause> conditions)
        {
            var compiled = conditions.Select((x, i) =>
            {
                return (i > 0 ? x.Boolean + " " : "") + CompileExpression(x.Expression);
            });

            return string.Join(" ", compiled);
        }

        public virtual string CompileWheres(Query query)
        {
            var wheres = query.GetComponents<ConditionClause>("where", EngineCode);

            if (!wheres.Any())
            {
                return null;
            }

            var conditions = CompileConditions(wheres);

            return $"where {string.Join(" ", conditions)}";
        }

        public virtual string CompileGroups(Query query)
        {

            if (!query.HasComponent("group", EngineCode))
            {
                return null;
            }

            var columns = query.GetComponents<GroupClause>("group", EngineCode)
            .Select(x => CompileExpression(x.Expression));

            return "group by " + string.Join(", ", columns);
        }

        public virtual string CompileOrders(Query query)
        {
            if (!query.HasComponent("order", EngineCode))
            {
                return null;
            }

            var orders = query.GetComponents<OrderByClause>("order", EngineCode);

            var columns = orders.Select(x =>
            {
                var desc = x.Descending ? " desc" : "";
                return CompileExpression(x.Expression) + desc;
            });

            return "order by " + string.Join(", ", columns);
        }

        public string CompileHavings(Query query)
        {
            var clauses = query.GetComponents<ConditionClause>("having", EngineCode);

            if (!clauses.Any())
            {
                return null;
            }

            var conditions = CompileConditions(clauses);

            return $"having {string.Join(" ", conditions)}";
        }

        public virtual string CompileLimit(Query query)
        {
            if (query.GetOneComponent("limit", EngineCode) is LimitOffset limitOffset && limitOffset.HasLimit())
            {
                bindings.Add(limitOffset.Limit);
                return "limit ?";
            }

            return "";
        }

        public virtual string CompileOffset(Query query)
        {
            if (query.GetOneComponent("limit", EngineCode) is LimitOffset limitOffset && limitOffset.HasOffset())
            {
                bindings.Add(limitOffset.Offset);
                return "offset ?";
            }

            return "";
        }

        public virtual string CompileLock(Query query)
        {
            // throw new NotImplementedException();
            return null;
        }

        /// <summary>
        /// Wrap a single string in keyword identifiers.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string WrapIdentifier(string value)
        {
            if (value == "*") return value;

            var opening = this.OpeningIdentifier;
            var closing = this.ClosingIdentifier;

            return opening + value.Replace(closing, closing + closing) + closing;
        }


        public virtual string ReplaceIdentifier(string input)
        {
            return input

                // deprecated
                .Replace("{", this.OpeningIdentifier)
                .Replace("}", this.ClosingIdentifier)

                .Replace("[", this.OpeningIdentifier)
                .Replace("]", this.ClosingIdentifier);
        }


        public virtual string CompileCte(Query query)
        {
            var clauses = query.GetComponents<FromClause>("cte", EngineCode);

            if (!clauses.Any())
            {
                return null;
            }

            var sql = new List<string>();

            foreach (var cte in clauses)
            {
                if (cte is FromClause from)
                {
                    sql.Add($"{CompileExpression(from.Expression)} as {WrapIdentifier(from.Alias)}");
                }
            }

            return "with " + string.Join(", ", sql) + " ";
        }

    }

}