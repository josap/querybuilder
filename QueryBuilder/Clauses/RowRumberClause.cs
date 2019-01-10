namespace SqlKata {
    public abstract class AbstractRowRumber : AbstractClause
    {

    }

    //New 2019-01-10
    public class RowRumber : AbstractRowRumber
    {
        public string Column { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RowRumber {
                Engine = Engine,
                Component = Component,
                Column = Column,
            };
        }
    }

}