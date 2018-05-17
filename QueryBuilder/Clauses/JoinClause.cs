using System;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public class JoinClause : AbstractClause
    {
        public string Type { get; set; }
        public Expression Expression { get; set; }
        public Expression Conditions { get; set; }

        public override AbstractClause Clone()
        {
            return new JoinClause
            {
                Engine = Engine,
                Component = Component,
                Type = Type,
                Expression = Expression.Clone(),
                Conditions = Conditions,
            };
        }
    }

}