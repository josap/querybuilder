using System;
using SqlKata.Expressions;

namespace SqlKata
{
    public abstract class AbstractExpression : AbstractClause
    {

    }

    public class ExpressionClause : AbstractExpression
    {
        public Expression Expression { get; set; }

        public override AbstractClause Clone()
        {
            return new ExpressionClause
            {
                Engine = Engine,
                Component = Component,
                Expression = Expression,
            };
        }
    }

}