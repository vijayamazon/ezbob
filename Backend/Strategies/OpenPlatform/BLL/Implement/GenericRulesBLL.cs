﻿namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement {
    using Ezbob.Backend.ModelsWithDB.Investor;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using StructureMap.Attributes;
    public class GenericRulesBLL : IGenericRulesBLL {
        [SetterProperty]
        public IInvestorCashRequestBLL InvestorCashRequestBLL { get; set; }
        [SetterProperty]
        public IInvestorParametersBLL InvestorParametersBLL { get; set; }

        public bool RuleBadgetLevel(int InvestorId, long CashRequestId, int ruleType) {
            var investorLoanCashRequest = InvestorCashRequestBLL.GetInvestorLoanCashRequest(CashRequestId);
            double gradeAvailableAmount = InvestorParametersBLL.GetGradeAvailableAmount(InvestorId, investorLoanCashRequest, ruleType);
            return gradeAvailableAmount >= investorLoanCashRequest.ManagerApprovedSum;
        }
    }
}
