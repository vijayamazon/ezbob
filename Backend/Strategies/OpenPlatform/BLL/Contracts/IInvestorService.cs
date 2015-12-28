namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts
{
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using EZBob.DatabaseLib.Model.Database;

    public interface IInvestorService {
        List<int> GetMatchedInvestors(InvestorCashRequest cashRequest, List<InvestorParameters> investorList, RuleType ruleType);
    }
}
