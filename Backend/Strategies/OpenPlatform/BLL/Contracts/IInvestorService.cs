namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts
{
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public interface IInvestorService {
        List<int> GetMatchedInvestors(InvestorCashRequest cashRequest, List<InvestorParameters> investorList, RuleType ruleType);
    }
}
