using System;
using System.Collections.Generic;
using System.Linq;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public class ConditionBuilder
    {
        private Expression current = null;

        private bool _isOr = false;
        private bool _isNot = false;
        private bool isOr
        {
            get
            {
                var val = _isOr;
                _isOr = false;
                return val;
            }
        }

        private bool isNot
        {
            get
            {
                var val = _isNot;
                _isNot = false;
                return val;
            }
        }

        private Dictionary<string, string> reversableOperator = new Dictionary<string, string>
        {
            {"in", "not in"},
            {"between", "not between"},
            {"is", "is not"},
            {"exists", "not exists"},
        };

        public ConditionBuilder Or()
        {
            _isOr = true;
            return this;
        }

        public ConditionBuilder Not()
        {
            _isNot = true;
            return this;
        }
        public Expression Evaluate()
        {
            return this.current;
        }

        protected ConditionBuilder Where(Expression expression)
        {


            if (isNot)
            {
                // try to reverse the expression operator if possible to avoid long not expressions
                // i.e. "col is not null" instead of "not (col is null)"
                if (expression is BooleanExpression expr && reversableOperator.ContainsKey(expr.Operator))
                {
                    expr.Operator = reversableOperator[expr.Operator];
                    expression = expr;
                }
                else
                {
                    expression = Conditions.Not(expression);
                }
            }

            if (current is null)
            {
                current = expression;
            }
            else
            {
                current = isOr ? Conditions.Or(current, expression) : Conditions.And(current, expression);
            }

            return this;
        }

        public ConditionBuilder Where(string column, string op, object value)
        {
            if (value is null)
            {
                return WhereNull(column);
            }

            return Where(Conditions.Boolean(column, op, value));
        }

        public ConditionBuilder OrWhere(string column, string op, object value)
        {
            return Or().Where(column, op, value);
        }

        public ConditionBuilder WhereNot(string column, string op, object value)
        {
            return Not().Where(column, op, value);
        }

        public ConditionBuilder OrWhereNot(string column, string op, object value)
        {
            return Or().Not().Where(column, op, value);
        }

        public ConditionBuilder Where(ConditionBuilder nested)
        {
            return Where(nested.Evaluate());
        }

        public ConditionBuilder OrWhere(ConditionBuilder nested)
        {
            return Or().Where(nested);
        }

        public ConditionBuilder WhereNot(ConditionBuilder nested)
        {
            return Not().Where(nested);
        }

        public ConditionBuilder OrWhereNot(ConditionBuilder nested)
        {
            return Or().Not().Where(nested);
        }

        public ConditionBuilder Where(Action<ConditionBuilder> callback)
        {
            var nested = new ConditionBuilder();

            callback(nested);

            return Where(nested);
        }

        public ConditionBuilder OrWhere(Action<ConditionBuilder> callback)
        {
            return Or().Where(callback);
        }

        public ConditionBuilder WhereNot(Action<ConditionBuilder> callback)
        {
            return Not().Where(callback);
        }

        public ConditionBuilder OrWhereNot(Action<ConditionBuilder> callback)
        {
            return Or().Not().Where(callback);
        }

        public ConditionBuilder WhereNull(string column)
        {
            return Where(new BooleanExpression
            {
                Left = new IdentifierExpression(column),
                Operator = "is",
                Right = new ConstantExpression { Value = "null" }
            });
        }

        public ConditionBuilder OrWhereNull(string column)
        {
            return Or().WhereNull(column);
        }

        public ConditionBuilder WhereNotNull(string column)
        {
            return Not().WhereNull(column);
        }

        public ConditionBuilder OrWhereNotNull(string column)
        {
            return Or().Not().WhereNull(column);
        }

        public ConditionBuilder WhereColumns(string left, string op, string right)
        {
            return Where(new BooleanExpression
            {
                Left = new IdentifierExpression(left),
                Operator = op,
                Right = new IdentifierExpression(right),
            });
        }

        public ConditionBuilder OrWhereColumns(string left, string op, string right)
        {
            return Or().WhereColumns(left, op, right);
        }

        public ConditionBuilder WhereNotColumns(string left, string op, string right)
        {
            return Not().WhereColumns(left, op, right);
        }

        public ConditionBuilder OrWhereNotColumns(string left, string op, string right)
        {
            return Or().Not().WhereColumns(left, op, right);
        }

        public ConditionBuilder WhereBetween(string column, object lower, object upper)
        {
            return Where(new BooleanExpression
            {
                Left = new IdentifierExpression(column),
                Operator = "between",
                Right = new BooleanExpression
                {
                    Left = new LiteralExpression(lower),
                    Operator = "and",
                    Right = new LiteralExpression(upper)
                }
            });
        }

        public ConditionBuilder WhereNotBetween(string column, object lower, object upper)
        {
            return Not().WhereBetween(column, lower, upper);
        }

        public ConditionBuilder OrWhereBetween(string column, object lower, object upper)
        {
            return Or().WhereBetween(column, lower, upper);
        }

        public ConditionBuilder OrWhereNotBetween(string column, object lower, object upper)
        {
            return Or().Not().WhereBetween(column, lower, upper);
        }

        public ConditionBuilder WhereIn<T>(string column, IEnumerable<T> values)
        {
            // transform it to falsy expression in case no elements were provided
            if (!values.Any())
            {
                return Where(new BooleanExpression
                {
                    Left = new ConstantExpression("1"),
                    Operator = "=",
                    Right = new ConstantExpression("0")
                });
            }

            return Where(new BooleanExpression
            {
                Left = new IdentifierExpression(column),
                Operator = "in",
                Right = new ListExpression
                {
                    Expressions = values.Select(v => new LiteralExpression(v)).ToList()
                }
            });
        }

        public ConditionBuilder OrWhereIn<T>(string column, IEnumerable<T> values)
        {
            return Or().WhereIn(column, values);
        }

        public ConditionBuilder WhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Not().WhereIn(column, values);
        }

        public ConditionBuilder OrWhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Or().Not().WhereIn(column, values);
        }

        public ConditionBuilder WhereIn(string column, Query query)
        {
            return Where(new BooleanExpression
            {
                Left = new IdentifierExpression(column),
                Operator = "in",
                Right = new QueryExpression
                {
                    Query = query
                }
            });
        }

        public ConditionBuilder WhereNot(string column, Query query)
        {
            return Not().WhereIn(column, query);
        }

        public ConditionBuilder OrWhereIn(string column, Query query)
        {
            return Or().WhereIn(column, query);
        }

        public ConditionBuilder OrWhereNotIn(string column, Query query)
        {
            return Or().Not().WhereIn(column, query);
        }

        public ConditionBuilder WhereExists(string column, Query query)
        {
            return Where(new BooleanExpression
            {
                Left = new IdentifierExpression(column),
                Operator = "exists",
                Right = new QueryExpression
                {
                    Query = query
                }
            });
        }

        public ConditionBuilder WhereNotExists(string column, Query query)
        {
            return Not().WhereExists(column, query);
        }

        public ConditionBuilder OrWhereExists(string column, Query query)
        {
            return Or().WhereExists(column, query);
        }

        public ConditionBuilder OrWhereNotExists(string column, Query query)
        {
            return Or().Not().WhereExists(column, query);
        }

        public ConditionBuilder Where(Func<ConditionBuilder, ConditionBuilder> callback)
        {
            return Where(new NestedExpression
            {
                Expression = callback(new ConditionBuilder()).Evaluate()
            });
        }

        public ConditionBuilder WhereNot(Func<ConditionBuilder, ConditionBuilder> callback)
        {
            return Not().Where(callback);
        }

        public ConditionBuilder OrWhere(Func<ConditionBuilder, ConditionBuilder> callback)
        {
            return Or().Where(callback);
        }

        public ConditionBuilder OrWhereNot(Func<ConditionBuilder, ConditionBuilder> callback)
        {
            return Or().Not().Where(callback);
        }

    }
}