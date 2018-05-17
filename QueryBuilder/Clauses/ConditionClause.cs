using System.Collections.Generic;
using System.Linq;
using SqlKata.Expressions;

namespace SqlKata.QueryBuilder
{
    public class ConditionClause : AbstractClause
    {
        public Expression Expression { get; set; }
        public string Boolean { get; set; } = "and";

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new ConditionClause
            {
                Boolean = Boolean,
                Engine = Engine,
                Component = Component,
                Expression = Expression.Clone()
            };
        }
    }

    public class BasicStringCondition : AbstractClause
    {
        public bool CaseSensitive { get; set; } = false;

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BasicStringCondition
            {
                Engine = Engine,
                // Column = Column,
                // Operator = Operator,
                // Value = Value,
                CaseSensitive = CaseSensitive,
                Component = Component,
            };
        }
    }

    public class BasicDateCondition : AbstractClause
    {
        public string Part { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new BasicDateCondition
            {
                Engine = Engine,
                // Column = Column,
                // Operator = Operator,
                // Value = Value,
                Part = Part,
                Component = Component,
            };
        }
    }
}