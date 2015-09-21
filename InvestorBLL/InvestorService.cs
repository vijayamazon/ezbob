namespace InvestorBLL
{
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.Investor;
    using InvestorBLL.Contracts;
    using RulesEngine.BLL;
    using RulesEngine.Contracts;
    using RulesEngine.Models;
    using StructureMap.Attributes;

    public class InvestorService : IInvestorService
    {
        [SetterProperty]
        public IInvestorParametersBLL InvestorParametersBLL { get; set; }

        [SetterProperty]
        public IProvider<IMatch<LoanParameters, InvestorParameters>> MatchProvider { get; set; }

        public List<int> GetMatchedInvestors(LoanParameters loanParameters)
        {
            var investorParametersList = InvestorParametersBLL.GetInvestorParametersList();
            var matchList = new List<IMatch<LoanParameters, InvestorParameters>>();
            foreach (var investorParameters in investorParametersList)
            {
                var matchInvestor = MatchProvider.GetNew();
                matchInvestor.Source = loanParameters;
                matchInvestor.Target = investorParameters;
                matchInvestor.BuildFunc(investorParameters.investorID, RuleType.InvestorRule);
                matchList.Add(matchInvestor);
            }
            return matchList.Where(x => x.IsMatched()).Select(x => x.Target.investorID).ToList();
        }

    }
}
