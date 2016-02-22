namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public interface IExressionBuilderBLL
    {
        Expression BuildExpression<T1, T2>(string leftPropertyName,
            Operator ruleOperator, string rightPropertyName,
            ParameterExpression parameterExpressionLeft,
            ParameterExpression parameterExpressionRight);

        Expression BuildSubExpression(Operator ruleOperator,
            Expression leftExpression,
            Expression rightExpression);

        Func<T1, T2, bool> CompileRule<T1, T2>(int investorId, long cashRequestID, Dictionary<int, InvestorRule> rulesDict);

        Expression BuildRecursiveExpression<T1, T2>(int investorId,
            long cashRequestID,
            Dictionary<int, InvestorRule> ruleDict,
            InvestorRule rule,
            ParameterExpression parameterExpressionLeft,
            ParameterExpression parameterExpressionRight);
    }
}