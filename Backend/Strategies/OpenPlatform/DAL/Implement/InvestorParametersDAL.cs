namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement
{
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;

    public class InvestorParametersDAL : IInvestorParametersDAL
    {
        public List<InvestorParameters> GetInvestorParametersList() {
            var investorParametersList = new List<InvestorParameters>();
            investorParametersList.Add(new InvestorParameters() {
                InvestorID = 1,
                DailyInvestmentAllowed = 700,
                Balance = 700
            });
            investorParametersList.Add(new InvestorParameters() {
                InvestorID = 2,
                DailyInvestmentAllowed = 100,
                Balance = 100
            });
            investorParametersList.Add(new InvestorParameters() {
                InvestorID = 3,
                DailyInvestmentAllowed = 400,
                Balance = 1000
            });
            investorParametersList.Add(new InvestorParameters() {
                InvestorID = 4,
                DailyInvestmentAllowed = 2000,
                Balance = 5000
            });
            return investorParametersList;
        }
    }
}
