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
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class TestInvestorRules : TestBase {


        [Test]
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
            investorParametersDALMock.Setup(x => x.GetInvestorsIds()).Returns(new List<int>() { 1 });
            investorParametersDALMock.Setup(x => x.GetInvestorParametersDB(1, RuleType.System)).Returns(new List<I_InvestorParams>() { new I_InvestorParams() { InvestorID = 1, Type = 1, ParameterID = 1, Value = 1000, InvestorParamsID = 1 } });
            genericRulesMock.Setup(x => x.RuleBadgetLevel(1, 1, 1)).Returns(true);
            Dictionary<int, decimal> dict2 = new Dictionary<int, decimal>();
            dict2.Add(1, 3000);
            investorParametersDALMock.Setup(x => x.InvestorsBalance).Returns(dict2);

            Dictionary<int, I_Parameter> ip = new Dictionary<int, I_Parameter>();
            ip.Add(1, new I_Parameter() {
                ParameterID = 1,
                Name = "DailyInvestmentAllowed",
                ValueType = "Decimal",
                DefaultValue = 0,
                MaxLimit = null,
                MinLimit = null
            });

            ip.Add(2, new I_Parameter() {
                ParameterID = 2,
                Name = "WeeklyInvestmentAllowed",
                ValueType = "Decimal",
                DefaultValue = 0,
                MaxLimit = null,
                MinLimit = null
            });

            investorParametersDALMock.Setup(x => x.InvestorsParameters).Returns(ip);


            container.Configure(r => r.ForSingletonOf<IInvestorCashRequestDAL>().Use(() => investorCashRequestDALMock.Object));
            container.Configure(r => r.ForSingletonOf<IRulesEngineDAL>().Use(() => ruleEngineDalMock.Object));
            container.Configure(r => r.ForSingletonOf<IInvestorParametersDAL>().Use(() => investorParametersDALMock.Object));
            container.Configure(r => r.ForSingletonOf<IGenericRulesBLL>().Use(() => genericRulesMock.Object));


            var investorService = container.GetInstance<IInvestorService>();

            var investor = investorService.GetMatchedInvestor(1);
            Assert.IsTrue(investor != null);

            genericRulesMock.Setup(x => x.RuleBadgetLevel(1, 1, 1))
            .Returns(false);

            investor = investorService.GetMatchedInvestor(1);
            Assert.IsTrue(investor == null);

        }


        [Test]
        public void TestGetMatchedInvestorsWithSystemRules() {

            var container = this.InitContainer(typeof(InvestorService));



            var ruleEngineDalMock = new Mock<IRulesEngineDAL>();
            var investorParametersDALMock = new Mock<IInvestorParametersDAL>();
            var investorCashRequestDALMock = new Mock<IInvestorCashRequestDAL>();
            var genericRulesMock = new Mock<IGenericRulesBLL>();

            SetSetups(ruleEngineDalMock, investorCashRequestDALMock, investorParametersDALMock, genericRulesMock);

            container.Configure(r => r.ForSingletonOf<IInvestorCashRequestDAL>()
                .Use(() => investorCashRequestDALMock.Object));
            container.Configure(r => r.ForSingletonOf<IRulesEngineDAL>()
                .Use(() => ruleEngineDalMock.Object));
            container.Configure(r => r.ForSingletonOf<IInvestorParametersDAL>()
                .Use(() => investorParametersDALMock.Object));
            container.Configure(r => r.ForSingletonOf<IGenericRulesBLL>()
                .Use(() => genericRulesMock.Object));


            var investorService = container.GetInstance<IInvestorService>();

            var investor = investorService.GetMatchedInvestor(1);
          Assert.IsTrue(investor != null);

            genericRulesMock.Setup(x => x.RuleBadgetLevel(1, 1, 1))
            .Returns(false);

            investor = investorService.GetMatchedInvestor(1);
           Assert.IsTrue(investor == null);

        }


        private void SetSetups(Mock<IRulesEngineDAL> ruleEngineDalMock,
            Mock<IInvestorCashRequestDAL> investorCashRequestDALMock,
            Mock<IInvestorParametersDAL> investorParametersDALMock,
            Mock<IGenericRulesBLL> genericRulesMock) {
            var rulesDict1 = GetRules();

            ruleEngineDalMock.Setup(x => x.GetRules(1, RuleType.System))
            .Returns(rulesDict1);


            genericRulesMock.Setup(x => x.RuleBadgetLevel(1, 1, 1))
                .Returns(true);

            investorCashRequestDALMock.Setup(x => x.GetInvestorLoanCashRequest(1))
                .Returns(GetInvestorLoanCashRequest());

            Dictionary<int, decimal> dict2 = new Dictionary<int, decimal>();
            dict2.Add(1, 3000);

            investorParametersDALMock.Setup(x => x.InvestorsBalance)
                .Returns(dict2);

            investorParametersDALMock.Setup(x => x.GetFundedAmountPeriod(1, InvesmentPeriod.Day))
                .Returns(0);

            investorParametersDALMock.Setup(x => x.GetFundedAmountPeriod(1, InvesmentPeriod.Week))
                .Returns(500);

            investorParametersDALMock.Setup(x => x.GetInvestorsIds())
                .Returns(new List<int>() {
                    1
                });


            Dictionary<int, I_Parameter> ip = new Dictionary<int, I_Parameter>();
            ip.Add(1, new I_Parameter() {
                ParameterID = 1,
                Name = "DailyInvestmentAllowed",
                ValueType = "Decimal",
                DefaultValue = 0,
                MaxLimit = null,
                MinLimit = null
            });

            ip.Add(2, new I_Parameter() {
                ParameterID = 2,
                Name = "WeeklyInvestmentAllowed",
                ValueType = "Decimal",
                DefaultValue = 0,
                MaxLimit = null,
                MinLimit = null
            });

            investorParametersDALMock.Setup(x => x.InvestorsParameters).Returns(ip);



            investorParametersDALMock.Setup(x => x.GetGradePercent(1, (int)Grade.A, 1))
                .Returns((decimal)0.2);
            investorParametersDALMock.Setup(x => x.GetGradeMonthlyInvestedAmount(1, Grade.A))
                .Returns(100);

            investorParametersDALMock.Setup(x => x.GetInvestorMonthlyFundingCapital(1))
                .Returns(5000);

            investorParametersDALMock.Setup(x => x.GetInvestorTotalMonthlyDeposits(1))
                .Returns(5000);

            investorParametersDALMock.Setup(x => x.GetInvestorParametersDB(1, RuleType.System))
                .Returns(new List<I_InvestorParams>() {
                    new I_InvestorParams() {
                        InvestorID = 1,
                        Type = 1,
                        ParameterID = 1,
                        Value = 1000,
                        InvestorParamsID = 1
                    },
                    new I_InvestorParams() {
                        InvestorID = 1,
                        Type = 1,
                        ParameterID = 2,
                        Value = 7000,
                        InvestorParamsID = 2
                    }
                });
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
                Balance = 500,
                WeeklyAvailableAmount = 2000
            };
        }

        private InvestorLoanCashRequest GetInvestorLoanCashRequest() {
            return new InvestorLoanCashRequest() {
                ManagerApprovedSum = 999,
                CashRequestID = 1,
                GradeID = (int)Grade.A
            };
        }

    }
}
