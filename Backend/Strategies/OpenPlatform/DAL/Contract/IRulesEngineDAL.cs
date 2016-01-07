namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract {
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;

    public interface IRulesEngineDAL {
        Dictionary<int, InvestorRule> GetRules(int investorID, RuleType ruleType);
    }
}