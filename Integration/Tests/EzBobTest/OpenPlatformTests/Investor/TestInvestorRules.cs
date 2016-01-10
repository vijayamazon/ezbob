namespace EzBobTest.OpenPlatformTests.Investor {
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
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
                RuleType = 1
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
                FuncName = "RuleBadgetLevel",
                RuleType = 1
            });

            rulesDict1.Add(3, new InvestorRule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
                MemberNameTarget = "DailyAvailableAmount",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 3,
                UserID = 1,
                RuleType = 1
            });

            var investorParameters = new InvestorParameters() {
                InvestorID = 1,
                DailyAvailableAmount = 700,
                Balance = 500
            };


            var investorParametersDALMock = new Mock<IInvestorParametersDAL>();
            var investorCashRequestDALMock = new Mock<IInvestorCashRequestDAL>();
            var genericRulesMock = new Mock<IGenericRulesBLL>();

            ruleEngineDalMock.Setup(x => x.GetRules(1, RuleType.System)).Returns(rulesDict1);
            investorCashRequestDALMock.Setup(x => x.GetInvestorLoanCashRequest(1)).Returns(new InvestorLoanCashRequest() { ManagerApprovedSum = 20, CashRequestID = 1 });
            investorParametersDALMock.Setup(x=> x.GetInvestorsIds()).Returns(new List<int>(){1});
            investorParametersDALMock.Setup(x => x.GetInvestorParameters(1,RuleType.System)).Returns(investorParameters);
            genericRulesMock.Setup(x => x.RuleBadgetLevel(1, 1, 1)).Returns(true);


            container.Configure(r => r.ForSingletonOf<IInvestorCashRequestDAL>().Use(() => investorCashRequestDALMock.Object));
            container.Configure(r => r.ForSingletonOf<IRulesEngineDAL>().Use(() => ruleEngineDalMock.Object));
            container.Configure(r => r.ForSingletonOf<IInvestorParametersDAL>().Use(() => investorParametersDALMock.Object));
            container.Configure(r => r.ForSingletonOf<IGenericRulesBLL>().Use(() => genericRulesMock.Object));


            var investorService = container.GetInstance<IInvestorService>();

            var ids = investorService.GetMatchedInvestors(1);
            Assert.IsTrue(ids.Count == 1);

            genericRulesMock.Setup(x => x.RuleBadgetLevel(1, 1, 1))
            .Returns(false);

            ids = investorService.GetMatchedInvestors(1);
            Assert.IsTrue(ids.Count == 0);

        }


        [TestMethod]
        public void TestGetMatchedInvestorsWithSystemRules() {

            var container = this.InitContainer(typeof(InvestorService));

            var rulesDict1 = GetRules();
            var investorParameters = GetInvestorParameters();
            
            var ruleEngineDalMock = new Mock<IRulesEngineDAL>();
            var investorParametersDALMock = new Mock<IInvestorParametersDAL>();
            var investorCashRequestDALMock = new Mock<IInvestorCashRequestDAL>();
            var genericRulesMock = new Mock<IGenericRulesBLL>();

            ruleEngineDalMock.Setup(x => x.GetRules(1, RuleType.System)).Returns(rulesDict1);
            investorCashRequestDALMock.Setup(x => x.GetInvestorLoanCashRequest(1)).Returns(GetInvestorLoanCashRequest());
            investorParametersDALMock.Setup(x => x.GetInvestorParameters(1, RuleType.System)).Returns(investorParameters);
            genericRulesMock.Setup(x => x.RuleBadgetLevel(1, 1, 1)).Returns(true);

            container.Configure(r => r.ForSingletonOf<IInvestorCashRequestDAL>().Use(() => investorCashRequestDALMock.Object));
            container.Configure(r => r.ForSingletonOf<IRulesEngineDAL>().Use(() => ruleEngineDalMock.Object));
            container.Configure(r => r.ForSingletonOf<IInvestorParametersDAL>().Use(() => investorParametersDALMock.Object));
            container.Configure(r => r.ForSingletonOf<IGenericRulesBLL>().Use(() => genericRulesMock.Object));

            var investorService = container.GetInstance<IInvestorService>();

            var ids = investorService.GetMatchedInvestors(1);
            Assert.IsTrue(ids.Count == 1);

            genericRulesMock.Setup(x => x.RuleBadgetLevel(1, 1, 1)).Returns(false);

            ids = investorService.GetMatchedInvestors(1);
            Assert.IsTrue(ids.Count == 0);

        }



        private Dictionary<int, InvestorRule> GetRules() {
            var rulesDict = new Dictionary<int, InvestorRule>();

            rulesDict.Add(1, new InvestorRule {
                Operator = (int)Operator.And,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = true,
                LeftParamID = 2,
                RightParamID = 3,
                InvestorID = 1,
                RuleID = 1,
                UserID = 1,
                RuleType = 1
            });


            rulesDict.Add(2, new InvestorRule {
                Operator = (int)Operator.And,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = false,
                LeftParamID = 4,
                RightParamID = 5,
                InvestorID = 1,
                RuleID = 2,
                UserID = 1,
                RuleType = 1
            });

            rulesDict.Add(3, new InvestorRule {
                Operator = (int)Operator.And,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = false,
                LeftParamID = 6,
                RightParamID = 7,
                InvestorID = 1,
                RuleID = 3,
                UserID = 1,
                RuleType = 1
            });

            rulesDict.Add(4, new InvestorRule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
                MemberNameTarget = "DailyAvailableAmount",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 4,
                UserID = 1,
                RuleType = 1
            });

            rulesDict.Add(5, new InvestorRule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
                MemberNameTarget = "WeeklyAvailableAmount",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 5,
                UserID = 1,
                RuleType = 1
            });

            rulesDict.Add(6, new InvestorRule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
                MemberNameTarget = "Balance",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 6,
                UserID = 1,
                RuleType = 1
            });

            rulesDict.Add(7, new InvestorRule {
                Operator = (int)Operator.IsTrue,
                MemberNameSource = null,
                MemberNameTarget = null,
                FuncName = "RuleBadgetLevel",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 7,
                UserID = 1,
                RuleType = 1
            });


            return rulesDict;
        }

        private InvestorParameters GetInvestorParameters() {
            return new InvestorParameters() {
                InvestorID = 1,
                DailyAvailableAmount = 700,
                Balance = 500
            };
        }

        private InvestorLoanCashRequest GetInvestorLoanCashRequest() {
            return new InvestorLoanCashRequest() {
                ManagerApprovedSum = 20,
                CashRequestID = 1
            };
        }

    }
}
