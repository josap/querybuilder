using System.Collections.Generic;

namespace SqlKata.Expressions
{

    public class Expression
    {

    }

    public class ScalarExpression : Expression
    {

    }

    public class ParameterExpression : ScalarExpression
    {
        public string Name { get; set; }
    }

    public class ConstantExpression : ScalarExpression
    {
        public object Value { get; set; }
    }

    public class QueryExpression : Expression
    {
        public Query Query { get; set; }
    }

    public class ScalarQueryExpression : ScalarExpression
    {

    }

    public class FunctionExpression : Expression
    {
        public string Function { get; set; }
        public List<Expression> Parameters { get; set; }

    }

    public class ScalarFunctionExpression : ScalarExpression
    {
        public string Function { get; set; }
        public IEnumerable<Expression> Parameters { get; set; }

    }

    public class BinaryExpression : Expression
    {
        public string Operator { get; set; }
        public Expression Left { get; set; }
        public Expression Right { get; set; }

    }

    public class RawExpression : Expression
    {
        public string Raw { get; set; }
    }

    public class OverByExpression : ScalarExpression
    {
        public ScalarFunctionExpression Function { get; set; }
        public IEnumerable<Expression> PartitionBy { get; set; }
        public Expression OrderBy { get; set; }

    }

    public class SelectExpression : Expression
    {
        public ScalarExpression Expression { get; set; }
        public string Alias { get; set; }
    }


    public static class Run
    {
        public static void Main()
        {
            var lessThan = new BinaryExpression
            {
                Left = new ParameterExpression
                {
                    Name = "Id"
                },
                Right = new ConstantExpression
                {
                    Value = 5
                },
                Operator = "<"
            };

            var select = new[]
            {
                new SelectExpression {
                    Expression = new ParameterExpression {
                        Name = "Id"
                    }
                },
                new SelectExpression {
                    Expression = new ScalarFunctionExpression {
                        Function = "MAX",
                        Parameters = new [] {
                            new ParameterExpression {
                                Name = "*"
                            }
                        }
                    }
                },
                new SelectExpression {
                    Expression = new OverByExpression {
                        Function = new ScalarFunctionExpression {
                            Function = "first_value",
                        },
                        PartitionBy = new Expression[] {
                            new ParameterExpression {
                                 Name = "OrderId"
                            },
                            new ParameterExpression {
                                 Name = "Date"
                            }
                        }

                    }
                }
            };


        }
    }

}
