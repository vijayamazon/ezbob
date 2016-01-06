namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement {
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using StructureMap.Attributes;

    public class GenericRulesBLL : IGenericRulesBLL {
        [SetterProperty]
        public IInvestorCashRequestBLL InvestorCashRequestBLL { get; set; }
        [SetterProperty]
        public IInvestorParametersBLL InvestorParametersBLL { get; set; }
        
        public bool RuleBadgetLevel(int InvestorId, long CashRequestId) {
            Dictionary<Grade, double> grageABudgets = InvestorParametersBLL.GetInvestorBudgetSokets(InvestorId);
            InvestorLoanCashRequest investorLoancCashRequest = InvestorCashRequestBLL.GetInvestorLoanCashRequest(CashRequestId);
            return grageABudgets[investorLoancCashRequest.Grade] <= investorLoancCashRequest.ManagerApprovedSum;
        }
    }
}

