using System.Collections.Generic;
using SqlKata.Expressions;
using SqlKata.QueryBuilder;

public static class Program
{
    public static void Main()
    {
        var expressions = new List<Expression>();

        expressions.Add(Conditions.Or(Conditions.Eq("Id", 1), Conditions.Lte("Score", 10)));

        var builder = new ConditionBuilder().Where("Id", "=", 2).Or().Where(x => x.Where("Id", "=", 2));

        // var query = new Query().Join("Users", j => j.Where())

    }
}