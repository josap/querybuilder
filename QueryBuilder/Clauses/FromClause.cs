using System;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public class FromClause : AbstractClause
    {
        public string Alias { get; set; }
        public Expression Expression { get; set; }

        public override AbstractClause Clone()
        {
            return new FromClause
            {
                Component = Component,
                Engine = Engine,
                Alias = Alias,
                Expression = Expression.Clone()
            };
        }
    }
}