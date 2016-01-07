namespace EzBobTest.OpenPlatformTests.Investor {
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Backend.Strategies.OpenPlatform.Facade.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.Facade.Implement;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;
    using EzBobTest.OpenPlatformTests.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TestInvestorRules : TestBase {
        
        [TestMethod]
        public void TestGetMatchedInvestorsWithResults() {



            var container = this.InitContainer(typeof(InvestorService));

            var ruleEngineDalMock = new Mock<IRulesEngineDAL>();

            var rulesDict1 = new Dictionary<int, InvestorRule>();

            rulesDict1.Add(1, new InvestorRule {
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

            rulesDict1.Add(2, new InvestorRule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
                MemberNameTarget = "Balance",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 2,
                UserID = 1
            });

            rulesDict1.Add(3, new InvestorRule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
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

            var cashRequest = new InvestorLoanCashRequest() {
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
            Assert.IsTrue(ids.Count == 1);
        }

        [TestMethod]
        public void TestGetMatchedInvestorsNoResults() {



            var container = this.InitContainer(typeof(InvestorService));

            var ruleEngineDalMock = new Mock<IRulesEngineDAL>();

            var rulesDict1 = new Dictionary<int, InvestorRule>();

            rulesDict1.Add(1, new InvestorRule {
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

            rulesDict1.Add(2, new InvestorRule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
                MemberNameTarget = "Balance",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 2,
                UserID = 1
            });

            rulesDict1.Add(3, new InvestorRule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
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

            var cashRequest = new InvestorLoanCashRequest() {
                ManagerApprovedSum = 2000
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
            Assert.IsTrue(ids.Count == 0);
        }

        [TestMethod]
        public void TestGetMatchedInvestorsWithComplexRule() {

            var container = this.InitContainer(typeof(InvestorService));

            var ruleEngineDalMock = new Mock<IRulesEngineDAL>();

            var rulesDict1 = new Dictionary<int, InvestorRule>();

            rulesDict1.Add(1, new InvestorRule {
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

            rulesDict1.Add(2, new InvestorRule {
                Operator = (int)Operator.IsTrue,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 2,
                UserID = 1,
                FuncName = "RuleBadgetLevel"
            });

            rulesDict1.Add(3, new InvestorRule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
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

            var cashRequest = new InvestorLoanCashRequest() {
                ManagerApprovedSum = 20,
                CashRequestID = 1
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

            var genericRulesMock = new Mock<IGenericRulesBLL>();
            genericRulesMock.Setup(x => x.RuleBadgetLevel(1, 1))
                .Returns(true);

            container.Configure(r => r.ForSingletonOf<IGenericRulesBLL>()
            .Use(() => genericRulesMock.Object));


            var ids = investorService.GetMatchedInvestors(cashRequest, investorParametersList, RuleType.System);
            Assert.IsTrue(ids.Count == 1);

            int a = 1;
            long b = 1;

            genericRulesMock.Setup(x => x.RuleBadgetLevel(a,b))
            .Returns(false);

            ids = investorService.GetMatchedInvestors(cashRequest, investorParametersList, RuleType.System);
            Assert.IsTrue(ids.Count == 0);


        }
    }
}
