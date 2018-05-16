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

    public class DeepJoin : JoinClause
    {
        public string Type { get; set; }
        public string Expression { get; set; }
        public string SourceKeySuffix { get; set; }
        public string TargetKey { get; set; }
        public Func<string, string> SourceKeyGenerator { get; set; }
        public Func<string, string> TargetKeyGenerator { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new DeepJoin
            {
                Engine = Engine,
                Component = Component,
                Type = Type,
                Expression = Expression,
                SourceKeySuffix = SourceKeySuffix,
                TargetKey = TargetKey,
                SourceKeyGenerator = SourceKeyGenerator,
                TargetKeyGenerator = TargetKeyGenerator,
            };
        }
    }
}