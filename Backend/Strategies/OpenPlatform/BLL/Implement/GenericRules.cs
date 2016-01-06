namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement {
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;

    public class GenericRules : IGenericRules {
        public bool RuleBadgetLevel(int InvestorId, long CashRequestId) {

            var cashRequest = new InvestorCashRequest() {
                ManagerApprovedSum = 200,
                Grade = Grade.A
            };
            var investor = new I_Investor();
            var investorParameters = new InvestorParameters() {
                GrageABudgets = new Dictionary<Grade, double>() 
            };
            investorParameters.GrageABudgets.Add(Grade.A, 400);

            return investorParameters.GrageABudgets[cashRequest.Grade] <= cashRequest.ManagerApprovedSum;
        }
    }
}

