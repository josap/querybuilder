using System.Collections.Generic;
using System.Linq;
using SqlKata.QueryBuilder;

namespace SqlKata.Expressions
{
    public abstract class Expression
    {
        public abstract Expression Clone();
        // public abstract Accept(IVisitor v);
    }

    /// <summary>
    /// Represents a parameter that should passed to the bindings
    /// </summary>
    public class LiteralExpression : Expression
    {
        public object Value { get; set; }
        public LiteralExpression() { }
        public LiteralExpression(object value)
        {
            this.Value = value;
        }
        public override Expression Clone()
        {
            return new LiteralExpression
            {
                Value = Value
            };
        }
    }

    /// <summary>
    /// Represents a constant that should be included directly in the SQL statement
    /// </summary>
    public class ConstantExpression : Expression
    {
        public string Value { get; set; }
        public ConstantExpression() { }
        public ConstantExpression(string value)
        {
            this.Value = value;
        }
        public override Expression Clone()
        {
            return new ConstantExpression
            {
                Value = Value
            };
        }
    }

    /// <summary>
    /// Represents an identifier like a column/alias/table
    /// that should be wrapped by the engine specific quotes
    /// </summary>
    public class IdentifierExpression : Expression
    {
        public IdentifierExpression() { }
        public IdentifierExpression(string value)
        {
            this.Value = value;
        }

        public string Value { get; set; }

        public override Expression Clone()
        {
            return new IdentifierExpression
            {
                Value = Value
            };
        }
    }

    public class BooleanExpression : Expression
    {
        public Expression Left { get; set; }
        public string Operator { get; set; }
        public Expression Right { get; set; }

        public override Expression Clone()
        {
            return new BooleanExpression
            {
                Left = Left.Clone(),
                Operator = Operator,
                Right = Right.Clone(),
            };
        }
    }

    public class FalseExpression : Expression
    {
        public override Expression Clone()
        {
            return new FalseExpression();
        }
    }

    public class TrueExpression : Expression
    {
        public override Expression Clone()
        {
            return new TrueExpression();
        }
    }

    public class NullExpression : Expression
    {
        public override Expression Clone()
        {
            return new NullExpression();
        }
    }

    public class NotExpression : Expression
    {
        public Expression Expression { get; set; }

        public override Expression Clone()
        {
            return new NotExpression();
        }
    }

    public class QueryExpression : Expression
    {
        public Query Query { get; set; }

        public override Expression Clone()
        {
            return new QueryExpression
            {
                Query = Query,
            };
        }
    }

    public class RawExpression : Expression
    {
        public RawExpression() { }

        public RawExpression(string expression, List<object> bindings = null)
        {
            Expression = expression;
            Bindings = bindings;
        }

        public string Expression { get; set; }
        public List<object> Bindings { get; set; }
        public override Expression Clone()
        {
            return new RawExpression
            {
                Expression = Expression,
                Bindings = Bindings,
            };
        }
    }

    /// <summary>
    /// Represents an SQL function
    /// </summary>
    public class FunctionExpression : Expression
    {
        public string Name { get; set; }
        public List<Expression> Arguments { get; set; }

        public FunctionExpression() { }
        public FunctionExpression(string name, params Expression[] arguments)
        {
            Name = name;
            Arguments = arguments.ToList();
        }

        public override Expression Clone()
        {
            return new FunctionExpression
            {
                Name = Name,
                Arguments = Arguments == null ?
                    null : Arguments.Select(x => x.Clone()).ToList(),
            };
        }
    }

    public class NestedExpression : Expression
    {
        public Expression Expression { get; set; }
        public override Expression Clone()
        {
            return new NestedExpression
            {
                Expression = Expression.Clone()
            };
        }
    }

    public class ListExpression : Expression
    {
        public List<Expression> Expressions { get; set; }
        public override Expression Clone()
        {
            return new ListExpression
            {
                Expressions = Expressions == null ? null : Expressions.Select(x => x.Clone()).ToList()
            };
        }
    }

}