namespace Ezbob.Backend.Strategies.OpenPlatform.BLL
{
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
	using Ezbob.Backend.Strategies.OpenPlatform.Provider.Contracts;
	using RulesEngine.Contracts;
	using RulesEngine.Models;
	using StructureMap.Attributes;

	public class InvestorService : IInvestorService
    {
        [SetterProperty]
        public IInvestorParametersBLL InvestorParametersBLL { get; set; }

        [SetterProperty]
        public IProvider<IMatch<OfferParameters, InvestorParameters>> MatchProvider { get; set; }

        public List<int> GetMatchedInvestors(OfferParameters offerParameters)
        {
            var investorParametersList = InvestorParametersBLL.GetInvestorParametersList();
            var matchList = new List<IMatch<OfferParameters, InvestorParameters>>();
            foreach (var investorParameters in investorParametersList)
            {
                var matchInvestor = MatchProvider.GetNew();
                matchInvestor.Source = offerParameters;
                matchInvestor.Target = investorParameters;
                matchInvestor.BuildFunc(investorParameters.InvestorID, RuleType.InvestorRule);
                matchList.Add(matchInvestor);
            }
            return matchList.Where(x => x.IsMatched()).Select(x => x.Target.InvestorID).ToList();
        }

    }
}
