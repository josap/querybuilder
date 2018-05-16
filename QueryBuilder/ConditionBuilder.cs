using System;
using System.Collections.Generic;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public class ConditionBuilder
    {
        private Expression current = new TrueExpression();

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
                expression = Conditions.Not(expression);
            }

            current = isOr ? Conditions.Or(current, expression) : Conditions.And(current, expression);

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
                Operator = "IS",
                Right = new ConstantExpression { Value = "NULL" }
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
            return Not().WhereNull(column);
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

    }
}