using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public class GroupClause : AbstractClause
    {
        public Expression Expression { get; set; }
        public override AbstractClause Clone()
        {
            return new GroupClause
            {
                Engine = Engine,
                Component = Component,
                Expression = Expression.Clone(),
            };
        }
    }
}