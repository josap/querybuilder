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
                return WrapIdentifiers(identifier.Value);
            }

            if (expression is BooleanExpression boolExpr)
            {
                return $"{CompileExpression(boolExpr.Left)} {boolExpr.Operator} {CompileExpression(boolExpr.Right)}";
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

        public virtual string CompileClause(AbstractClause clause)
        {
            throw new InvalidOperationException("No compiler found for clause of type " + clause.GetType().FullName);
        }

        /// <summary>
        /// Compile a single column clause
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public virtual string CompileColumn(SelectColumnClause column)
        {
            return $"{CompileExpression(column.Expression)} as {column.Alias}";
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
                sql += CompileInsert(query);
            }
            else if (query.Method == "delete")
            {
                sql += CompileDelete(query);
            }
            else if (query.Method == "update")
            {
                sql += CompileUpdate(query);
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

        public virtual string CompileCte(Query query)
        {
            var clauses = query.GetComponents<FromClause>("cte", EngineCode);

            if (!clauses.Any())
            {
                return "";
            }

            var sql = new List<string>();

            foreach (var cte in clauses)
            {
                if (cte is RawFromClause raw)
                {
                    bindings.AddRange(raw.Bindings);
                    sql.Add($"{WrapValue(raw.Alias)} AS ({WrapIdentifiers(raw.Expression)})");
                }
                else if (cte is QueryFromClause queryFromClause)
                {
                    sql.Add($"{WrapValue(queryFromClause.Alias)} AS ({CompileSelect(queryFromClause.Query)})");
                }
            }

            return "WITH " + string.Join(", ", sql) + " ";
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

            string sql = JoinComponents(results, "select");

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
                        combinedQueries.Add(WrapIdentifiers(combineRawClause.Expression));
                    }
                }

                sql = JoinComponents(combinedQueries, "combine");

            }

            return sql;
        }

        /// <summary>
        /// Compile INSERT into statement
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual string CompileInsert(Query query)
        {
            if (!query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table set to insert");
            }

            var from = query.GetOneComponent<FromClause>("from", EngineCode);

            if (!(from is FromClause))
            {
                throw new InvalidOperationException("Invalid table expression");
            }

            string sql;

            var inserts = query.GetComponents<AbstractInsertClause>("insert", EngineCode);

            if (inserts[0] is InsertClause insertClause)
            {
                sql = "INSERT INTO " + CompileTableExpression(from)
                    + " (" + string.Join(", ", WrapArray(insertClause.Columns)) + ") "
                    + "VALUES (" + string.Join(", ", Parameterize(insertClause.Values)) + ")";
            }
            else
            {
                var clause = inserts[0] as InsertQueryClause;

                var columns = "";

                if (clause.Columns.Any())
                {
                    columns = $"({string.Join(", ", WrapArray(clause.Columns))}) ";
                }

                sql = "INSERT INTO " + CompileTableExpression(from)
                    + " " + columns + CompileSelect(clause.Query);
            }

            if (inserts.Count > 1)
            {
                foreach (var insert in inserts.GetRange(1, inserts.Count - 1))
                {
                    var clause = insert as InsertClause;

                    sql += ", (" + string.Join(", ", Parameterize(clause.Values)) + ")";

                }
            }


            return sql;

        }

        protected virtual string CompileUpdate(Query query)
        {
            if (!query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table set to update");
            }

            var from = query.GetOneComponent<FromClause>("from", EngineCode);

            if (!(from is FromClause))
            {
                throw new InvalidOperationException("Invalid table expression");
            }

            var toUpdate = query.GetOneComponent<InsertClause>("update", EngineCode);

            var parts = new List<string>();
            string sql;

            for (var i = 0; i < toUpdate.Columns.Count; i++)
            {
                parts.Add($"{Wrap(toUpdate.Columns[i])} = ?");
            }

            bindings.AddRange(toUpdate.Values);

            var where = CompileWheres(query);

            if (!string.IsNullOrEmpty(where))
            {
                where = " " + where;
            }

            sql = "UPDATE " + CompileTableExpression(from)
                + " SET " + string.Join(", ", parts)
                + where;

            return sql;
        }

        protected virtual string CompileDelete(Query query)
        {
            if (!query.HasComponent("from", EngineCode))
            {
                throw new InvalidOperationException("No table set to delete");
            }

            var from = query.GetOneComponent<FromClause>("from", EngineCode);

            if (!(from is FromClause))
            {
                throw new InvalidOperationException("Invalid table expression");
            }

            string sql;

            var where = CompileWheres(query);

            if (!string.IsNullOrEmpty(where))
            {
                where = " " + where;
            }

            sql = "DELETE FROM " + CompileTableExpression(from) + where;

            return sql;
        }

        protected virtual string CompileColumns(Query query)
        {

            var columns = query.GetComponents<SelectColumnClause>("columns", EngineCode);


            if (!columns.Any())
            {
                return query.IsDistinct ? "distinct *" : "*";
            }



            var sql = columns.Select(x => CompileExpression(x.Expression));


        }

        public virtual string CompileAggregate(Query query)
        {

            if (!query.HasComponent("aggregate", EngineCode))
            {
                return null;
            }

            var ag = query.GetComponents("aggregate").Cast<AggregateClause>().First();

            var columns = ag.Columns
                .Select(x => new Column { Name = x })
                .Cast<SelectColumnClause>()
                .ToList();

            var cols = columns.Select(CompileColumn);

            var sql = string.Join(", ", cols);

            if (query.IsDistinct)
            {
                sql = "DISTINCT " + sql;
            }

            return "SELECT " + ag.Type.ToUpper() + "(" + sql + ") AS " + WrapValue(ag.Type);
        }

        public virtual string CompileTableExpression(FromClause from)
        {
            if (from is RawFromClause raw)
            {
                bindings.AddRange(raw.Bindings);
                return WrapIdentifiers(raw.Expression);
            }

            if (from is QueryFromClause queryFromClause)
            {
                var fromQuery = queryFromClause.Query;

                var alias = string.IsNullOrEmpty(fromQuery.QueryAlias) ? "" : " AS " + WrapValue(fromQuery.QueryAlias);

                var compiled = CompileSelect(fromQuery);

                return "(" + compiled + ")" + alias;
            }

            if (from is FromClause fromClause)
            {
                return Wrap(fromClause.Table);
            }

            throw InvalidClauseException("TableExpression", from);
        }

        public virtual string CompileFrom(Query query)
        {
            if (!query.HasComponent("from", EngineCode))
            {
                return null;
            }

            var from = query.GetOneComponent<FromClause>("from", EngineCode);

            return "FROM " + CompileTableExpression(from);
        }

        // public virtual string CompileJoins(Query query)
        // {
        //     if (!query.HasComponent("join", EngineCode))
        //     {
        //         return null;
        //     }

        //     var joins = query.GetComponents<BaseJoin>("join", EngineCode);

        //     var sql = new List<string>();

        //     foreach (var item in joins)
        //     {
        //         sql.Add(CompileJoin(item.Join));
        //     }

        //     return JoinComponents(sql, "join");
        // }

        // public virtual string CompileJoin(Join join, bool isNested = false)
        // {
        //     return "";

        //     var from = join.GetOneComponent<AbstractFrom>("from", EngineCode);
        //     var conditions = join.GetComponents<AbstractCondition>("where", EngineCode);

        //     var joinTable = CompileTableExpression(from);
        //     var constraints = CompileConditions(conditions);

        //     var onClause = conditions.Any() ? $" ON {constraints}" : "";

        //     return $"{join.Type} JOIN {joinTable}{onClause}";
        // }

        public virtual string CompileWheres(Query query)
        {
            if (!query.HasComponent("from", EngineCode) || !query.HasComponent("where", EngineCode))
            {
                return null;
            }

            var conditions = query.GetComponents<AbstractCondition>("where", EngineCode);
            var sql = CompileConditions(conditions).Trim();

            return string.IsNullOrEmpty(sql) ? null : $"WHERE {sql}";
        }

        public string CompileQuery(
                Query query,
                string joinType = "",
                bool isNested = false
        )
        {
            if (query is Query)
            {
                return CompileSelect(query as Query);
            }

            // if (query is Join)
            // {
            //     return CompileJoin((query as Join), isNested);
            // }

            return "";
        }

        public virtual string CompileGroups(Query query)
        {
            if (!query.HasComponent("group", EngineCode))
            {
                return null;
            }

            var columns = query.GetComponents<SelectColumnClause>("group", EngineCode).Select(x => CompileColumn(x));

            return "GROUP BY " + string.Join(", ", columns);
        }

        public virtual string CompileOrders(Query query)
        {
            if (!query.HasComponent("order", EngineCode))
            {
                return null;
            }

            var columns = query.GetComponents<OrderByClause>("order", EngineCode).Select(x =>
            {

                if (x is RawOrderBy raw)
                {
                    bindings.AddRange(raw.Bindings);
                    return WrapIdentifiers(raw.Expression);
                }

                var direction = (x as OrderBy).Ascending ? "" : " DESC";

                return Wrap((x as OrderBy).Column) + direction;
            });

            return "ORDER BY " + string.Join(", ", columns);
        }

        public string CompileHavings(Query query)
        {
            if (!query.HasComponent("having", EngineCode))
            {
                return null;
            }

            var sql = new List<string>();
            string boolOperator;

            var havings = query.GetComponents("having", EngineCode)
                .Cast<AbstractCondition>()
                .ToList();

            for (var i = 0; i < havings.Count; i++)
            {
                var compiled = CompileCondition(havings[i]);

                if (!string.IsNullOrEmpty(compiled))
                {
                    boolOperator = i > 0 ? havings[i].IsOr ? "OR " : "AND " : "";

                    sql.Add(boolOperator + "HAVING " + compiled);
                }
            }

            return JoinComponents(sql, "having");
        }

        public virtual string CompileLimit(Query query)
        {
            if (query.GetOneComponent("limit", EngineCode) is LimitOffset limitOffset && limitOffset.HasLimit())
            {
                bindings.Add(limitOffset.Limit);
                return "LIMIT ?";
            }

            return "";
        }

        public virtual string CompileOffset(Query query)
        {
            if (query.GetOneComponent("limit", EngineCode) is LimitOffset limitOffset && limitOffset.HasOffset())
            {
                bindings.Add(limitOffset.Offset);
                return "OFFSET ?";
            }

            return "";
        }

        public virtual string CompileLock(Query query)
        {
            // throw new NotImplementedException();
            return null;
        }

        /// <summary>
        /// Compile the random statement into SQL.
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public virtual string CompileRandom(string seed)
        {
            return "RANDOM()";
        }

        public virtual string CompileLower(string value)
        {
            return $"LOWER({value})";
        }

        public virtual string CompileUpper(string value)
        {
            return $"UPPER({value})";
        }

        private InvalidCastException InvalidClauseException(string section, AbstractClause clause)
        {
            return new InvalidCastException($"Invalid type \"{clause.GetType().Name}\" provided for the \"{section}\" clause.");
        }

        protected virtual string dynamicCompile(string name, AbstractClause clause)
        {

            MethodInfo methodInfo = this.GetType()
                .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception($"Failed to locate a compiler for {name}.");
            }

            var isGeneric = Helper.IsGenericType(clause.GetType());

            if (isGeneric && methodInfo.GetGenericArguments().Any())
            {
                var args = clause.GetType().GetGenericArguments();
                methodInfo = methodInfo.MakeGenericMethod(args);
            }

            var result = methodInfo.Invoke(this, new object[] { clause });

            return result as string;
        }

        public virtual string JoinComponents(List<string> components, string section = null)
        {
            return string.Join(" ", components);
        }

        /// <summary>
        /// Wrap a single string in a column identifier.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string Wrap(string value)
        {
            if (value.ToLower().Contains(" as "))
            {
                var segments = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                return Wrap(segments[0]) + " AS " + WrapValue(segments[2]);
            }

            if (value.Contains("."))
            {
                return string.Join(".", value.Split('.').Select((x, index) =>
                {
                    return WrapValue(x);
                }));
            }

            // If we reach here then the value does not contain an "AS" alias
            // nor dot "." expression, so wrap it as regular value.
            return WrapValue(value);
        }

        /// <summary>
        /// Wrap a single string in keyword identifiers.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string WrapValue(string value)
        {
            if (value == "*") return value;

            var opening = this.OpeningIdentifier;
            var closing = this.ClosingIdentifier;

            return opening + value.Replace(closing, closing + closing) + closing;
        }

        public virtual string Parameter<T>(T value)
        {
            bindings.Add(value);
            return "?";
        }

        /// <summary>
        /// Create query parameter place-holders for an array.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public virtual string Parameterize<T>(IEnumerable<T> values)
        {
            return string.Join(", ", values.Select(x => Parameter(x)));
        }

        /// <summary>
        /// Wrap an array of values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public virtual List<string> WrapArray(List<string> values)
        {
            return values.Select(x => Wrap(x)).ToList();
        }

        public virtual string WrapIdentifiers(string input)
        {
            return input

                // deprecated
                .Replace("{", this.OpeningIdentifier)
                .Replace("}", this.ClosingIdentifier)

                .Replace("[", this.OpeningIdentifier)
                .Replace("]", this.ClosingIdentifier);
        }

    }



}