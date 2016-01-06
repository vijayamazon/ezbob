namespace Ezbob.Backend.Strategies.OpenPlatform.Facade.Contracts
{
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public interface IInvestorService {
        List<InvestorParameters> GetMatchedInvestors(InvestorLoanCashRequest cashRequest, List<InvestorParameters> investorList, RuleType ruleType);
        List<InvestorParameters> GetInvestorParameters();
        InvestorLoanCashRequest GetInvestorLoanCashRequest(long cashRequestID);
    }
}
