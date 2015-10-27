namespace Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.Contracts
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using Ezbob.Backend.ModelsWithDB.Investor;
	using Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.Models;

	public interface IExressionBuilder
    {
        Expression BuildExpression<T1, T2>(string leftPropertyName,
            Operator ruleOperator, string rightPropertyName,
            ParameterExpression parameterExpressionLeft,
            ParameterExpression parameterExpressionRight);

        Expression BuildSubExpression(Operator ruleOperator,
            Expression leftExpression,
            Expression rightExpression);

        Func<T1, T2, bool> CompileRule<T1, T2>(int investorId,
            Dictionary<int, Rule> rulesDict);

        Expression BuildRecursiveExpression<T1, T2>(int investorId,
            Dictionary<int, Rule> ruleDict,
            Rule rule,
            ParameterExpression parameterExpressionLeft,
            ParameterExpression parameterExpressionRight);
    }
}