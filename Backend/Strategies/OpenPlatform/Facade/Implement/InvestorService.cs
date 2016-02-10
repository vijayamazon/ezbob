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
	using log4net;
	using StructureMap.Attributes;

    public class InvestorService : IInvestorService
    {
        [SetterProperty]
        public IInvestorParametersBLL InvestorParametersBLL { get; set; }
        [SetterProperty]
        public IInvestorCashRequestBLL IInvestorCashRequestBLL { get; set; }

        [SetterProperty]
        public IProvider<IMatchBLL<InvestorLoanCashRequest, InvestorParameters>> MatchProvider { get; set; }

        public KeyValuePair<int, decimal>? GetMatchedInvestor(long cashRequestID) {
            InvestorLoanCashRequest investorLoancCashRequest = IInvestorCashRequestBLL.GetInvestorLoanCashRequest(cashRequestID);
            List<int> investorsList = InvestorParametersBLL.GetInvestorsIds();

            investorsList = FilterInvestors(investorLoancCashRequest, investorsList, RuleType.System);
            investorsList = FilterInvestors(investorLoancCashRequest, investorsList, RuleType.UnderWriter);
            investorsList = FilterInvestors(investorLoancCashRequest, investorsList, RuleType.Investor);

            if (investorsList.Count == 0)
                return null;

            int finalInvestor = InvestorParametersBLL.GetInvestorWithLatestLoanDate(investorsList);
            return new KeyValuePair<int, decimal>(finalInvestor, investorLoancCashRequest.FundingType);
        }


        public List<int> FilterInvestors(InvestorLoanCashRequest investorLoanCashRequest, List<int> InvestorList, RuleType ruleType)
        {
            
            var matchList = new List<IMatchBLL<InvestorLoanCashRequest, InvestorParameters>>();
            foreach (var investorId in InvestorList) {
                
				var investorParameter = InvestorParametersBLL.GetInvestorParameters(investorId, ruleType);
				
				var matchInvestor = MatchProvider.GetNew();
				
                if (investorParameter == null) {
                    matchInvestor.Target = new InvestorParameters() {
                        InvestorID = investorId,                       
                    };
                    matchInvestor.Func = delegate { return true; };
                    matchList.Add(matchInvestor);
                    continue;                    
                }
                matchInvestor.Source = investorLoanCashRequest;
                matchInvestor.Target = investorParameter;

				Log.InfoFormat("\n\n\n\nFilterInvestors cr: \n{0}\ninvestor:\n{1}", investorLoanCashRequest, investorParameter);
                matchInvestor.BuildFunc(investorParameter.InvestorID, investorLoanCashRequest.CashRequestID, ruleType);
				Log.InfoFormat(string.Format("{0}\n {1} < {2}\n {3} < {4}\n {5} < {6}\n ",matchInvestor.Func.ToString(), 
					investorLoanCashRequest.ManagerApprovedSum, investorParameter.DailyAvailableAmount, 
					investorLoanCashRequest.ManagerApprovedSum, investorParameter.WeeklyAvailableAmount, 
					investorLoanCashRequest.ManagerApprovedSum, investorParameter.Balance));

                matchList.Add(matchInvestor);
            }
            return matchList.Where(x => x.IsMatched())
                .Select(x => x.Target.InvestorID)
                .ToList();
        }

		protected static ILog Log = LogManager.GetLogger(typeof(InvestorService));
    }
}
