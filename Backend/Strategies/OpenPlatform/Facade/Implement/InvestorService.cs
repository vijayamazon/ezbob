namespace Ezbob.Backend.Strategies.OpenPlatform.Facade.Implement
{
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.Facade.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;
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

        public Dictionary<int, InvestorParameters> GetMatchedInvestors(long cashRequestID) {
            InvestorLoanCashRequest investorLoancCashRequest = IInvestorCashRequestBLL.GetInvestorLoanCashRequest(cashRequestID);
            Dictionary<int, InvestorParameters> investorsDict = InvestorParametersBLL.GetInvestorsParameters();

            investorsDict = FilterInvestors(investorLoancCashRequest, investorsDict, RuleType.System);
            investorsDict = FilterInvestors(investorLoancCashRequest, investorsDict, RuleType.UnderWriter);
            investorsDict = FilterInvestors(investorLoancCashRequest, investorsDict, RuleType.Investor);
            return investorsDict;
        }


        public Dictionary<int, InvestorParameters> FilterInvestors(InvestorLoanCashRequest investorLoancCashRequest, Dictionary<int, InvestorParameters> InvestorParametersDict, RuleType parameterType)
        {
            var matchList = new List<IMatchBLL<InvestorLoanCashRequest, InvestorParameters>>();
            foreach (var investorParameters in InvestorParametersDict) {
                var matchInvestor = MatchProvider.GetNew();
                matchInvestor.Source = investorLoancCashRequest;
                matchInvestor.Target = investorParameters.Value;
                matchInvestor.BuildFunc(investorParameters.Value.InvestorID, investorLoancCashRequest.CashRequestID, parameterType);
                matchList.Add(matchInvestor);
            }
            return matchList.Where(x => x.IsMatched())
                .Select(x => x.Target)
                .ToDictionary(x => x.InvestorID, x => x);
        }
    }
}
