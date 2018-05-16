using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public class SelectColumnClause : AbstractClause
    {
        public Expression Expression { get; set; }
        public string Alias { get; set; }

        public override AbstractClause Clone()
        {
            return new SelectColumnClause
            {
                Engine = Engine,
                Component = Component,
                Expression = Expression.Clone(),
                Alias = Alias,
            };
        }
    }
}