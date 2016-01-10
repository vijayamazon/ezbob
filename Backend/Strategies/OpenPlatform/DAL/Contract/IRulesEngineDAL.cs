namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract {
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public interface IRulesEngineDAL {
        Dictionary<int, InvestorRule> GetRules(int investorID, RuleType ruleType);
    }
}