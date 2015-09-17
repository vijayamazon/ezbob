namespace RulesEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Ezbob.Backend.ModelsWithDB.Investor;

    public class ExressionBuilder
    {

        public static Func<T1, T2, bool> CompileRule<T1, T2>(Rule rule)
        {
            var expLeft = Expression.Parameter(typeof(T1));
            var expRight = Expression.Parameter(typeof(T2));

            Expression binaryExpression = BuildExpression<T1, T2>(rule.MemberNameSource,
                                                            rule.Operator,
                                                            rule.MemberNameTarget,
                                                            expLeft,
                                                            expRight);

            Func<T1, T2, bool> func = Expression.Lambda<Func<T1, T2, bool>>(binaryExpression, expLeft, expRight).Compile();
            return func;
        }

        public static Expression BuildExpression<T1, T2>(string leftPropertyName,
                                                                Operator ruleOperator, string rightPropertyName,
                                                                ParameterExpression parameterExpressionLeft,
                                                                ParameterExpression parameterExpressionRight)
        {
            var leftOperand = MemberExpression.Property(parameterExpressionLeft, leftPropertyName);
            var rightOperand = MemberExpression.Property(parameterExpressionRight, rightPropertyName);

            return BuildSubExpression(ruleOperator, leftOperand, rightOperand);
        }


        public static Expression BuildSubExpression(Operator ruleOperator,
                                                                Expression leftExpression,
                                                                Expression rightExpression)
        {
            ExpressionType expressionType = new ExpressionType();
            FieldInfo fieldInfo = expressionType.GetType().GetField(Enum.GetName(typeof(Operator), ruleOperator));
            var expressionTypeValue = (ExpressionType)fieldInfo.GetValue(ruleOperator);
            if (ruleOperator == Operator.Not) 
                return Expression.Not(leftExpression);
            return Expression.MakeBinary(expressionTypeValue, leftExpression, rightExpression);
        }

        public static Func<T1, T2, bool> CompileRule<T1, T2>(int investorId,
                                                             Dictionary<int, Rule> rulesDict)
        {

            var expLeft = Expression.Parameter(typeof(T1));
            var expRight = Expression.Parameter(typeof(T2));

            var rootRule = rulesDict.FirstOrDefault(x => ((x.Value.InvestorID == investorId) && (x.Value.IsRoot)));

            var binaryExpression = BuildRecursiveExpression<T1, T2>(investorId,
                                                                                rulesDict,
                                                                                rootRule.Value,
                                                                                expLeft,
                                                                                expRight);

            Func<T1, T2, bool> func = Expression.Lambda<Func<T1, T2, bool>>(binaryExpression, expLeft, expRight).Compile();

            return func;
        }

        private static Expression BuildRecursiveExpression<T1, T2>(int investorId,
            Dictionary<int, Rule> ruleDict,
            Rule rule,
            ParameterExpression parameterExpressionLeft,
            ParameterExpression parameterExpressionRight)
        {

            if (!string.IsNullOrEmpty(rule.MemberNameSource) &&
                !string.IsNullOrEmpty(rule.MemberNameTarget))
            {
                return BuildExpression<T1, T2>(rule.MemberNameSource,
                    rule.Operator,
                    rule.MemberNameTarget,
                    parameterExpressionLeft,
                    parameterExpressionRight);
            }

            Rule leftRule = ruleDict.FirstOrDefault(x => x.Value.RuleID == rule.LeftParamID).Value;
            var exLeft = BuildRecursiveExpression<T1, T2>(investorId, ruleDict, leftRule, parameterExpressionLeft, parameterExpressionRight);

            var rightRule = ruleDict.FirstOrDefault(x => x.Value.RuleID == rule.RightParamID).Value;
            var exRight = BuildRecursiveExpression<T1, T2>(investorId, ruleDict, rightRule, parameterExpressionLeft, parameterExpressionRight);

            return BuildSubExpression(rule.Operator, exLeft, exRight);
        }

    }
}