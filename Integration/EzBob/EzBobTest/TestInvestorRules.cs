namespace EzBobTest
{
    using System;
    using System.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.Investor;
    using RulesEngine;

    [TestClass]
    public class TestInvestorRules
    {

        [TestMethod]
        public void TestSimpleRule() {
            

            var rule = new Rule()
            {
                InvestorID = 1,
                UserID = 1,
                RuleID = 2,
                MemberNameSource = "Amount",
                MemberNameTarget = "DailyInvestmentAllowed",
                LeftParamID = -1,
                RightParamID = -1,
                Operator = Operator.LessThan,
                IsRoot = false
            };

            var investorParameters = new InvestorParameters() {
                DailyInvestmentAllowed = 800,
                MonthlyInvestmentAllowed = 1200,
                WeeklyInvestmentAllowed = 500000,
                GradeMin = 1,
                GradeMax =3
            };

            var loanParameters = new LoanParameters() {
                Amount = 1000,
                Grade = 1
            };

            //validate that expression (LoanParameters.Amount < InvestorParameters.DailyInvestmentAllowed)
            //arg 0 =  800
            //arge 1 = 1000
            //1000 < 800 ? => false
            var firstRuleFunc = ExressionBuilder.CompileRule<LoanParameters, InvestorParameters>(rule);
            var result = firstRuleFunc(loanParameters, investorParameters);
            Assert.IsFalse(result);

            //validate that expression (LoanParameters.Amount < InvestorParameters.DailyInvestmentAllowed)
            //arg 0 =  1200
            //arge 1 = 1000
            // 1000 < 1200 ? => true
            investorParameters.DailyInvestmentAllowed = 1200;
            result = firstRuleFunc(loanParameters, investorParameters);
            Assert.IsTrue(result);

 


        }


        [TestMethod]
        public void TestCombineRuleLevel1() {


            var loanParameters = new LoanParameters()
            {
                Amount = 1500,
                Grade = 1
            };

            var investorParameters = new InvestorParameters()
            {
                DailyInvestmentAllowed = 1000,
                MonthlyInvestmentAllowed = 5000,
                WeeklyInvestmentAllowed = 500000,
                GradeMin = 1,
                GradeMax = 3
            };

            var rulesDict = new Dictionary<int, Rule>();

            rulesDict.Add(1, new Rule()
            {
                Operator = Operator.And,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = true,
                LeftParamID = 2,
                RightParamID = 3,
                InvestorID = 1,
                RuleID = 1,
                UserID = 1
            });

            rulesDict.Add(2, new Rule()
            {
                Operator = Operator.LessThan,
                MemberNameSource = "Amount",
                MemberNameTarget = "DailyInvestmentAllowed",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 2,
                UserID = 1
            });

            rulesDict.Add(3, new Rule()
            {
                Operator = Operator.LessThan,
                MemberNameSource = "Amount",
                MemberNameTarget = "MonthlyInvestmentAllowed",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 3,
                UserID = 1
            });


            var InvestorId = 1;
            var rootRule = rulesDict.FirstOrDefault(x => x.Value.IsRoot);


            //((Amount < MonthlyInvestmentAllowed)&&(Amount < DailyInvestmentAllowed))

            //dayly = 1000, monthly = 5000, amount = 1500 => false
            var firstRuleFunc = ExressionBuilder.CompileRule<LoanParameters, InvestorParameters>(InvestorId, rulesDict);
            var result = firstRuleFunc(loanParameters, investorParameters);
            Assert.IsFalse(result);


            //dayly = 2000, monthly = 5000, amount = 1500 => true
            investorParameters.DailyInvestmentAllowed = 2000;
            result = firstRuleFunc(loanParameters, investorParameters);
            Assert.IsTrue(result);

        }

        [TestMethod]
        public void TestCombineRuleLevel2()
        {


            var rulesDict = new Dictionary<int, Rule>();

            rulesDict.Add(1, new Rule()
            {
                Operator = Operator.Or,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = true,
                LeftParamID = 2,
                RightParamID = 5,
                InvestorID = 1,
                RuleID = 1,
                UserID = 1
            }); 

            rulesDict.Add(2, new Rule()
            {
                Operator = Operator.And,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = true,
                LeftParamID = 3,
                RightParamID = 4,
                InvestorID = 1,
                RuleID = 2,
                UserID = 1
            });

            rulesDict.Add(3, new Rule()
            {
                Operator = Operator.LessThan,
                MemberNameSource = "Amount",
                MemberNameTarget = "DailyInvestmentAllowed",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 3,
                UserID = 1
            });

            rulesDict.Add(4, new Rule()
            {
                Operator = Operator.LessThan,
                MemberNameSource = "Amount",
                MemberNameTarget = "MonthlyInvestmentAllowed",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 4,
                UserID = 1
            });

            rulesDict.Add(5, new Rule()
            {
                Operator = Operator.GreaterThan,
                MemberNameSource = "Grade",
                MemberNameTarget = "GradeMin",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 5,
                UserID = 1
            });


            var loanParameters = new LoanParameters()
            {
                Amount = 2000,
                Grade = 4
            };

            var investorParameters = new InvestorParameters()
            {
                DailyInvestmentAllowed = 1000,
                MonthlyInvestmentAllowed = 5000,
                WeeklyInvestmentAllowed = 500000,
                GradeMin = 2,
                GradeMax = 3
            };

            var InvestorId = 1;
            var rootRule = rulesDict.FirstOrDefault(x => x.Value.IsRoot);


            // ((LoanParameters.Amount < InvestorParameters.DailyInvestmentAllowed) &&
            // (LoanParameters.Amount < InvestorParameters.MonthlyInvestmentAllowed))
            // || LoanParameters.Grade > InvestorParameters.GradeMin

            var firstRuleFunc = ExressionBuilder.CompileRule<LoanParameters, InvestorParameters>(InvestorId, rulesDict);
            var result = firstRuleFunc(loanParameters, investorParameters);
            Assert.IsTrue(result);
        
        }

        //[TestMethod]
        //public void TestMethod1()
        //{
        //    Dictionary<int, Rule> rules = new Dictionary<int, Rule>();

        //    rules.Add(1,new Rule()
        //    {
        //        InvestorID = 111,
        //        RuleID = 1,
        //        MemberName = "GradeMin",
        //        LeftParam = "ID:2",
        //        RightParam = "2",
        //        Operator = "Or",
        //        IsRoot = true
        //    });

        //    rules.Add(2,new Rule()
        //    {
        //        InvestorID = 111,
        //        RuleID = 2,
        //        MemberName ="DailyInvestmentAllowed",
        //        LeftParam = "100000",
        //        RightParam = null,
        //        Operator = "LessThan",
        //        IsRoot = false
        //    });

        //    var investorId = 111;
        //    var rootRule = rules.FirstOrDefault(x => (x.Value.InvestorID == investorId) && x.Value.IsRoot);

        //    ExressionBuilder.rulesData = rules;



        //    var expression = ExressionBuilder.BuildExpression<InvestorParameters>();



        //}
    }
}
