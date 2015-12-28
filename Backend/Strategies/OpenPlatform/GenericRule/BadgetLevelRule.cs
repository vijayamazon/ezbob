namespace Ezbob.Backend.Strategies.OpenPlatform.GenericRule {
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using EZBob.DatabaseLib.Model.Database;

    class BudgetLevelRule : IGenericRules {
        public bool InvokeRule(int cashRequestId, int investorID) {
            var cashRequest = new InvestorCashRequest();
            var investor = new I_Investor();
            var investorParameters = new InvestorParameters();
            return investorParameters.GrageABudgets[cashRequest.Grade] <= cashRequest.ManagerApprovedSum;
        }
    }
}
