namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using log4net;
    using StructureMap.Attributes;

    public class ExressionBuilderBLL : IExressionBuilderBLL {

        [SetterProperty]
        public IGenericRulesBLL GenericRules { get; set; }

        public Expression BuildExpression<T1, T2>(string leftPropertyName,
                                                                Operator ruleOperator, string rightPropertyName,
                                                                ParameterExpression parameterExpressionLeft,
                                                                ParameterExpression parameterExpressionRight) {
            var leftOperand = Expression.Property(parameterExpressionLeft, leftPropertyName);
            var rightOperand = Expression.Property(parameterExpressionRight, rightPropertyName);

            return BuildSubExpression(ruleOperator, leftOperand, rightOperand);
        }


        public Expression BuildSubExpression(Operator ruleOperator,
                                                                Expression leftExpression,
                                                                Expression rightExpression) {
            ExpressionType expressionType = new ExpressionType();
            FieldInfo fieldInfo = expressionType.GetType().GetField(Enum.GetName(typeof(Operator), ruleOperator));
            var expressionTypeValue = (ExpressionType)fieldInfo.GetValue(ruleOperator);
            if (ruleOperator == Operator.Not)
                return Expression.Not(leftExpression);
            if (ruleOperator == Operator.IsTrue)
                return Expression.IsTrue(leftExpression);
            return Expression.MakeBinary(expressionTypeValue, leftExpression, rightExpression);
        }

        public Func<T1, T2, bool> CompileRule<T1, T2>(int investorId, long cashRequestID, Dictionary<int, InvestorRule> rulesDict) {

            var expLeft = Expression.Parameter(typeof(T1));
            var expRight = Expression.Parameter(typeof(T2));

            var rootRule = rulesDict.FirstOrDefault(x => (x.Value.IsRoot));

            var binaryExpression = BuildRecursiveExpression<T1, T2>(investorId,
                                                                    cashRequestID,
                                                                    rulesDict,
                                                                    rootRule.Value,
                                                                    expLeft,
                                                                    expRight);
			
            Func<T1, T2, bool> func = Expression.Lambda<Func<T1, T2, bool>>(binaryExpression, expLeft, expRight).Compile();
			Log.InfoFormat("built expression for investorID {0} crID {1} : {2}", investorId, cashRequestID, binaryExpression);
            return func;
        }


        public Expression BuildRecursiveExpression<T1, T2>(int investorId, long cashRequestID, Dictionary<int, InvestorRule> ruleDict, InvestorRule rule, ParameterExpression parameterExpressionLeft, ParameterExpression parameterExpressionRight) {
            if (!string.IsNullOrEmpty(rule.FuncName)) {
                var result = InvokeGenericRule(investorId, cashRequestID,rule.RuleType, rule.FuncName);
                return Expression.IsTrue(Expression.Constant(result));
            }

            if (!string.IsNullOrEmpty(rule.MemberNameSource) &&
                !string.IsNullOrEmpty(rule.MemberNameTarget)) {
                return BuildExpression<T1, T2>(rule.MemberNameSource,
                    (Operator)rule.Operator,
                    rule.MemberNameTarget,
                    parameterExpressionLeft,
                    parameterExpressionRight);
            }

            InvestorRule leftRule = ruleDict.FirstOrDefault(x => x.Value.RuleID == rule.LeftParamID).Value;
            var exLeft = BuildRecursiveExpression<T1, T2>(investorId, cashRequestID, ruleDict, leftRule, parameterExpressionLeft, parameterExpressionRight);

            var rightRule = ruleDict.FirstOrDefault(x => x.Value.RuleID == rule.RightParamID).Value;
            var exRight = BuildRecursiveExpression<T1, T2>(investorId, cashRequestID, ruleDict, rightRule, parameterExpressionLeft, parameterExpressionRight);

            return BuildSubExpression((Operator)rule.Operator, exLeft, exRight);
        }

        public bool InvokeGenericRule(int InvestorId, long CashRequestId, int ruleType, string methodName) {
            bool result = false;
            Assembly genericRulesAssembly = GenericRules.GetType().Assembly;
            Type myType = genericRulesAssembly.GetTypes()
                .FirstOrDefault(x => x.FullName.Contains("GenericRulesBLL"));
            if (myType != null) {
                MethodInfo Method = myType.GetMethod(methodName);
                object myInstance = GenericRules;
                result = (bool)Method.Invoke(myInstance, BindingFlags.InvokeMethod | BindingFlags.Public, null, new object[] {
                    InvestorId, CashRequestId,ruleType
                }, CultureInfo.CurrentCulture);
            }
            return result;
        }

		protected static ILog Log = LogManager.GetLogger(typeof(InvestorParametersBLL));

    }
}