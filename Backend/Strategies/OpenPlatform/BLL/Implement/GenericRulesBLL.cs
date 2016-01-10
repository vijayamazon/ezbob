namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement {
    using System.Linq;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using StructureMap.Attributes;

    public class GenericRulesBLL : IGenericRulesBLL {
        [SetterProperty]
        public IInvestorCashRequestBLL InvestorCashRequestBLL { get; set; }
        [SetterProperty]
        public IInvestorParametersBLL InvestorParametersBLL { get; set; }

        public bool RuleBadgetLevel(int InvestorId, long CashRequestId, int ruleType) {
            var investorLoanCashRequest = InvestorCashRequestBLL.GetInvestorLoanCashRequest(CashRequestId);
            InvestorLoanCashRequest investorLoancCashRequest = InvestorCashRequestBLL.GetInvestorLoanCashRequest(CashRequestId);            
            double gradeAvailableAmount = InvestorParametersBLL.GetGradeAvailableAmount(InvestorId, investorLoanCashRequest, ruleType);
            return gradeAvailableAmount >= investorLoancCashRequest.ManagerApprovedSum;
        }

        public bool RuleDailyInvesmentAllowed(int InvestorId, long CashRequestId, int ruleType) {
            var investorLoanCashRequest = InvestorCashRequestBLL.GetInvestorLoanCashRequest(CashRequestId);
            var investorParameters = InvestorParametersBLL.GetInvestorParameters(InvestorId, ruleType);
            return investorParameters.DailyAvailableAmount <= investorLoanCashRequest.ManagerApprovedSum;
        }

        public bool RuleWeeklyInvesmentAllowed(int InvestorId, long CashRequestId, int ruleType) {
            var investorLoanCashRequest = InvestorCashRequestBLL.GetInvestorLoanCashRequest(CashRequestId);
            var investorParameters = InvestorParametersBLL.GetInvestorParameters(InvestorId, ruleType);
            return investorParameters.WeeklyAvailableAmount <= investorLoanCashRequest.ManagerApprovedSum;
        }

    }
}
