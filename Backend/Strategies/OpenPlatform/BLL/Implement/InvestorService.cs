namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement
{
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using StructureMap.Attributes;

    public class InvestorService : IInvestorService
    {
        [SetterProperty]
        public IInvestorParametersBLL InvestorParametersBLL { get; set; }

        [SetterProperty]
        public IProvider<IMatch<InvestorCashRequest, InvestorParameters>> MatchProvider { get; set; }

        public List<int> GetMatchedInvestors(InvestorCashRequest cashRequest, List<InvestorParameters> InvestorParametersList, RuleType parameterType) {

            var matchList = new List<IMatch<InvestorCashRequest, InvestorParameters>>();
            foreach (var investorParameters in InvestorParametersList) {
                var matchInvestor = MatchProvider.GetNew();
                matchInvestor.Source = cashRequest;
                matchInvestor.Target = investorParameters;
                matchInvestor.BuildFunc(investorParameters.InvestorID, cashRequest.CashRequestID, parameterType);
                matchList.Add(matchInvestor);
            }
            return matchList.Where(x => x.IsMatched()).Select(x => x.Target.InvestorID).ToList();
        }
	}
}
