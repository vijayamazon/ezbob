namespace Ezbob.Backend.Strategies.OpenPlatform.BLL
{
	using System.Collections.Generic;
	using System.Dynamic;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
	using Ezbob.Backend.Strategies.OpenPlatform.Provider.Contracts;
	using EZBob.DatabaseLib.Model.Database;
	using RulesEngine.Contracts;
	using RulesEngine.Models;
	using StructureMap.Attributes;
    using System;
	using Ezbob.Backend.Models.Investor;

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
                matchInvestor.BuildFunc(investorParameters.InvestorID, parameterType);
                matchList.Add(matchInvestor);
            }
            return matchList.Where(x => x.IsMatched()).Select(x => x.Target.InvestorID).ToList();
        }
	}
}
