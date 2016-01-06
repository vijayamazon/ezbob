namespace Ezbob.Backend.Strategies.OpenPlatform.Facade.Implement
{
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.Facade.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.Registry;
    using StructureMap.Attributes;

    public class InvestorService : IInvestorService
    {
        [SetterProperty]
        public IInvestorParametersBLL InvestorParametersBLL { get; set; }
        [SetterProperty]
        public IInvestorCashRequestBLL IInvestorCashRequestBLL { get; set; }

        [SetterProperty]
        public IProvider<IMatchBLL<InvestorLoanCashRequest, InvestorParameters>> MatchProvider { get; set; }

        public List<InvestorParameters> GetMatchedInvestors(InvestorLoanCashRequest cashRequest, List<InvestorParameters> InvestorParametersList, RuleType parameterType) {

            var matchList = new List<IMatchBLL<InvestorLoanCashRequest, InvestorParameters>>();
            foreach (var investorParameters in InvestorParametersList) {
                var matchInvestor = MatchProvider.GetNew();
                matchInvestor.Source = cashRequest;
                matchInvestor.Target = investorParameters;
                matchInvestor.BuildFunc(investorParameters.InvestorID, cashRequest.CashRequestID, parameterType);
                matchList.Add(matchInvestor);
            }
            return matchList.Where(x => x.IsMatched()).Select(x => x.Target).ToList();
        }

        public List<InvestorParameters> GetInvestorParameters() {
            return InvestorParametersBLL.GetInvestorParametersList();
        }
        public InvestorLoanCashRequest GetInvestorLoanCashRequest(long cashRequestID) {
            return IInvestorCashRequestBLL.GetInvestorLoanCashRequest(cashRequestID);
        }
    }
}
