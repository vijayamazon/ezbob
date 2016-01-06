namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;

    public interface IExressionBuilder
    {
        Expression BuildExpression<T1, T2>(string leftPropertyName,
            Operator ruleOperator, string rightPropertyName,
            ParameterExpression parameterExpressionLeft,
            ParameterExpression parameterExpressionRight);

        Expression BuildSubExpression(Operator ruleOperator,
            Expression leftExpression,
            Expression rightExpression);

        Func<T1, T2, bool> CompileRule<T1, T2>(int investorId, long cashRequestID, Dictionary<int, Rule> rulesDict);

        Expression BuildRecursiveExpression<T1, T2>(int investorId,
            long cashRequestID,
            Dictionary<int, Rule> ruleDict,
            Rule rule,
            ParameterExpression parameterExpressionLeft,
            ParameterExpression parameterExpressionRight);
    }
}