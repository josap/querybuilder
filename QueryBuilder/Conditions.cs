using System.Collections.Generic;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public static class Conditions
    {

        public static Expression Boolean(string column, string op, object value)
        {
            return new BooleanExpression
            {
                Left = new IdentifierExpression(column),
                Operator = "=",
                Right = new LiteralExpression { Value = value }
            };
        }

        public static Expression Or(params Expression[] expressions)
        {
            if (expressions.Length == 0)
            {
                return new FalseExpression();
            }

            if (expressions.Length == 1)
            {
                return expressions[0];
            }

            var result = new BooleanExpression
            {
                Operator = "or",
                Left = expressions[0],
                Right = expressions[1],
            };

            for (var i = 2; i < expressions.Length; i++)
            {
                result.Left = new BooleanExpression
                {
                    Operator = "or",
                    Left = result.Left,
                    Right = expressions[i]
                };
            }

            return result;
        }

        public static Expression And(params Expression[] expressions)
        {
            if (expressions.Length == 0)
            {
                return new TrueExpression();
            }

            if (expressions.Length == 1)
            {
                return expressions[0];
            }

            var result = new BooleanExpression
            {
                Operator = "and",
                Left = expressions[0],
                Right = expressions[1],
            };

            for (var i = 2; i < expressions.Length; i++)
            {
                result.Left = new BooleanExpression
                {
                    Operator = "and",
                    Left = result.Left,
                    Right = expressions[i]
                };
            }

            return result;
        }

        public static Expression Eq(string column, object value)
        {
            return Boolean(column, "=", value);
        }

        public static Expression Neq(string column, object value)
        {
            return Boolean(column, "<>", value);
        }

        public static Expression Gt(string column, object value)
        {
            return Boolean(column, ">", value);
        }

        public static Expression Gte(string column, object value)
        {
            return Boolean(column, ">=", value);
        }

        public static Expression Lt(string column, object value)
        {
            return Boolean(column, ">", value);
        }

        public static Expression Lte(string column, object value)
        {
            return Boolean(column, ">=", value);
        }

        public static Expression Not(Expression expression)
        {
            return new NotExpression
            {
                Expression = expression
            };
        }

    }
}