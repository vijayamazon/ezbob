namespace EzBobTest.OpenPlatformTests.Investor {
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Net.Security;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.BLL;
    using Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.DAL;
    using Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.Models;
    using Ezbob.Utils;
    using EzBobTest.OpenPlatformTests.Core;
    using EZBob.DatabaseLib.Model.Database;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using NHibernate.Util;

    [TestClass]
    public class TestInvestorRules : TestBase {

        [TestMethod]
        public void TestSimpleRule() {
            var rulesDict = new Dictionary<int, Rule>();

            rulesDict.Add(1, new Rule {
                InvestorID = 1,
                UserID = 1,
                RuleID = 2,
                MemberNameSource = "Amount",
                MemberNameTarget = "DailyInvestmentAllowed",
                LeftParamID = -1,
                RightParamID = -1,
                Operator = (int)Operator.LessThan,
                IsRoot = true
            });

            var investorParameters = new InvestorParameters() {
                DailyInvestmentAllowed = 800,
                MonthlyInvestmentAllowed = 1200,
                WeeklyInvestmentAllowed = 500000,
                GradeMin = 1,
                GradeMax = 3
            };

            var loanParameters = new OfferParameters() {
                Amount = 1000,
                Grade = 1
            };

            //validate that expression (LoanParameters.Amount < InvestorParameters.DailyInvestmentAllowed)
            //arg 0 =  800
            //arge 1 = 1000
            //1000 < 800 ? => false

            var InvestorId = 1;

            var container = this.InitContainer(typeof(InvestorService));
            var exressionBuilder = container.GetInstance<IExressionBuilder>();

            var firstRuleFunc = exressionBuilder.CompileRule<OfferParameters, InvestorParameters>(InvestorId, rulesDict);
            var result = firstRuleFunc(loanParameters, investorParameters);
            Assert.IsFalse(result);

            //validate that expression (OfferParameters.Amount < InvestorParameters.DailyInvestmentAllowed)
            //arg 0 =  1200
            //arge 1 = 1000
            // 1000 < 1200 ? => true
            investorParameters.DailyInvestmentAllowed = 1200;
            result = firstRuleFunc(loanParameters, investorParameters);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCombineRuleLevel1() {
            var loanParameters = new OfferParameters {
                Amount = 1500,
                Grade = 1
            };

            var investorParameters = new InvestorParameters {
                DailyInvestmentAllowed = 1000,
                MonthlyInvestmentAllowed = 5000,
                WeeklyInvestmentAllowed = 500000,
                GradeMin = 1,
                GradeMax = 3
            };

            var rulesDict = new Dictionary<int, Rule>();

            rulesDict.Add(1, new Rule {
                Operator = (int)Operator.And,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = true,
                LeftParamID = 2,
                RightParamID = 3,
                InvestorID = 1,
                RuleID = 1,
                UserID = 1
            });

            rulesDict.Add(2, new Rule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "Amount",
                MemberNameTarget = "DailyInvestmentAllowed",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 2,
                UserID = 1
            });

            rulesDict.Add(3, new Rule {
                Operator = (int)Operator.LessThan,
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
            //var rootRule = rulesDict.FirstOrDefault(x => x.Value.IsRoot);

            var container = this.InitContainer(typeof(InvestorService));
            var exressionBuilder = container.GetInstance<IExressionBuilder>();

            //((Amount < MonthlyInvestmentAllowed)&&(Amount < DailyInvestmentAllowed))

            //dayly = 1000, monthly = 5000, amount = 1500 => false
            var firstRuleFunc = exressionBuilder.CompileRule<OfferParameters, InvestorParameters>(InvestorId, rulesDict);
            var result = firstRuleFunc(loanParameters, investorParameters);
            Assert.IsFalse(result);


            //dayly = 2000, monthly = 5000, amount = 1500 => true
            investorParameters.DailyInvestmentAllowed = 2000;
            result = firstRuleFunc(loanParameters, investorParameters);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCombineRuleLevel2() {
            var rulesDict = new Dictionary<int, Rule>();

            rulesDict.Add(1, new Rule {
                Operator = (int)Operator.Or,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = true,
                LeftParamID = 2,
                RightParamID = 5,
                InvestorID = 1,
                RuleID = 1,
                UserID = 1
            });

            rulesDict.Add(2, new Rule {
                Operator = (int)Operator.And,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = true,
                LeftParamID = 3,
                RightParamID = 4,
                InvestorID = 1,
                RuleID = 2,
                UserID = 1
            });

            rulesDict.Add(3, new Rule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "Amount",
                MemberNameTarget = "DailyInvestmentAllowed",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 3,
                UserID = 1
            });

            rulesDict.Add(4, new Rule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "Amount",
                MemberNameTarget = "MonthlyInvestmentAllowed",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 4,
                UserID = 1
            });

            rulesDict.Add(5, new Rule {
                Operator = (int)Operator.GreaterThan,
                MemberNameSource = "Grade",
                MemberNameTarget = "GradeMin",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 5,
                UserID = 1
            });


            var loanParameters = new OfferParameters {
                Amount = 2000,
                Grade = 4
            };

            var investorParameters = new InvestorParameters {
                DailyInvestmentAllowed = 1000,
                MonthlyInvestmentAllowed = 5000,
                WeeklyInvestmentAllowed = 500000,
                GradeMin = 2,
                GradeMax = 3
            };

            var InvestorId = 1;
            //var rootRule = rulesDict.FirstOrDefault(x => x.Value.IsRoot);


            // ((LoanParameters.Amount < InvestorParameters.DailyInvestmentAllowed) &&
            // (LoanParameters.Amount < InvestorParameters.MonthlyInvestmentAllowed))
            // || LoanParameters.Grade > InvestorParameters.GradeMin


            IExressionBuilder exressionBuilder = new ExressionBuilder();
            var firstRuleFunc = exressionBuilder.CompileRule<OfferParameters, InvestorParameters>(InvestorId, rulesDict);
            var result = firstRuleFunc(loanParameters, investorParameters);
            Assert.IsTrue(result);

        }

        //[TestMethod]
        //public void TestRulesMultiInvestors() {
        //    var container = this.InitContainer(typeof(InvestorService));

        //    var ruleEngineDalMock = new Mock<IRulesEngineDAL>();

        //    var rulesDict1 = new Dictionary<int, Rule>();

        //    rulesDict1.Add(1, new Rule {
        //        Operator = Operator.And,
        //        MemberNameSource = null,
        //        MemberNameTarget = null,
        //        IsRoot = true,
        //        LeftParamID = 2,
        //        RightParamID = 3,
        //        InvestorID = 1,
        //        RuleID = 1,
        //        UserID = 1
        //    });

        //    rulesDict1.Add(2, new Rule {
        //        Operator = Operator.LessThan,
        //        MemberNameSource = "Amount",
        //        MemberNameTarget = "DailyInvestmentAllowed",
        //        IsRoot = false,
        //        LeftParamID = -1,
        //        RightParamID = -1,
        //        InvestorID = 1,
        //        RuleID = 2,
        //        UserID = 1
        //    });

        //    rulesDict1.Add(3, new Rule {
        //        Operator = Operator.LessThan,
        //        MemberNameSource = "Amount",
        //        MemberNameTarget = "MonthlyInvestmentAllowed",
        //        IsRoot = false,
        //        LeftParamID = -1,
        //        RightParamID = -1,
        //        InvestorID = 1,
        //        RuleID = 3,
        //        UserID = 1
        //    });


        //    ruleEngineDalMock.Setup(x => x.GetRules(1, ParameterType.Investor))
        //        .Returns(rulesDict1);

        //    var rulesDict2 = new Dictionary<int, Rule>();

        //    rulesDict2.Add(4, new Rule {
        //        Operator = Operator.And,
        //        MemberNameSource = null,
        //        MemberNameTarget = null,
        //        IsRoot = true,
        //        LeftParamID = 5,
        //        RightParamID = 6,
        //        InvestorID = 2,
        //        RuleID = 4,
        //        UserID = 1
        //    });

        //    rulesDict2.Add(5, new Rule {
        //        Operator = Operator.LessThan,
        //        MemberNameSource = "Amount",
        //        MemberNameTarget = "DailyInvestmentAllowed",
        //        IsRoot = false,
        //        LeftParamID = -1,
        //        RightParamID = -1,
        //        InvestorID = 2,
        //        RuleID = 5,
        //        UserID = 1
        //    });

        //    rulesDict2.Add(6, new Rule {
        //        Operator = Operator.LessThan,
        //        MemberNameSource = "Amount",
        //        MemberNameTarget = "MonthlyInvestmentAllowed",
        //        IsRoot = false,
        //        LeftParamID = -1,
        //        RightParamID = -1,
        //        InvestorID = 2,
        //        RuleID = 6,
        //        UserID = 1
        //    });

        //    ruleEngineDalMock.Setup(x => x.GetRules(2, ParameterType.Investor))
        //                     .Returns(rulesDict2);

        //    var investorParametersMock = new Mock<IInvestorParametersDAL>();
        //    var investorParametersList = new List<InvestorParameters>();

        //    investorParametersList.Add(new InvestorParameters() {
        //        InvestorID = 1,
        //        DailyInvestmentAllowed = 1000,
        //        MonthlyInvestmentAllowed = 5000,
        //        WeeklyInvestmentAllowed = 500000,
        //        GradeMin = 2,
        //        GradeMax = 3
        //    });

        //    investorParametersList.Add(new InvestorParameters() {
        //        InvestorID = 2,
        //        DailyInvestmentAllowed = 500,
        //        MonthlyInvestmentAllowed = 5000,
        //        WeeklyInvestmentAllowed = 500000,
        //        GradeMin = 2,
        //        GradeMax = 3
        //    });

        //    investorParametersMock.Setup(x => x.GetInvestorParametersList())
        //                     .Returns(investorParametersList);

        //    var loanParameters = new OfferParameters() {
        //        Amount = 800,
        //        Grade = 4
        //    };

        //    container.Configure(r => r.ForSingletonOf<IRulesEngineDAL>()
        //        .Use(() => ruleEngineDalMock.Object));

        //    container.Configure(r => r.ForSingletonOf<IInvestorParametersDAL>()
        //        .Use(() => investorParametersMock.Object));

        //    var investorService = container.GetInstance<IInvestorService>();
        //    //var ids = investorService.GetMatchedInvestors(loanParameters,);

        //    //Assert.IsTrue(ids.Count == 1);
        //}


        [TestMethod]
        public void TestRulesEngine() {



            var container = this.InitContainer(typeof(InvestorService));

            var ruleEngineDalMock = new Mock<IRulesEngineDAL>();

            var rulesDict1 = new Dictionary<int, Rule>();

            rulesDict1.Add(1, new Rule {
                Operator = (int)Operator.And,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = true,
                LeftParamID = 2,
                RightParamID = 3,
                InvestorID = 1,
                RuleID = 1,
                UserID = 1,
                RuleType = (int)RuleType.System
            });

            rulesDict1.Add(2, new Rule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSumFinal",
                MemberNameTarget = "Balance",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 2,
                UserID = 1
            });

            rulesDict1.Add(3, new Rule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSumFinal",
                MemberNameTarget = "DailyInvestmentAllowed",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 3,
                UserID = 1
            });


            ruleEngineDalMock.Setup(x => x.GetRules(1, RuleType.System))
                .Returns(rulesDict1);


            var investorParametersMock = new Mock<IInvestorParametersDAL>();

            var cashRequest = new InvestorCashRequest() {
                ManagerApprovedSum = 20
            };

            container.Configure(r => r.ForSingletonOf<IRulesEngineDAL>()
                .Use(() => ruleEngineDalMock.Object));

            container.Configure(r => r.ForSingletonOf<IInvestorParametersDAL>()
                .Use(() => investorParametersMock.Object));

            var investorService = container.GetInstance<IInvestorService>();

            var investorParametersList = new List<InvestorParameters>() {
                new InvestorParameters() {
                    InvestorID = 1,
                    DailyInvestmentAllowed = 700,
                    Balance = 500
                }
            };

            var ids = investorService.GetMatchedInvestors(cashRequest, investorParametersList, RuleType.System);

            //Assert.IsTrue(ids.Count == 1);
        }
    }




}
