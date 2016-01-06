namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract {
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;

    public interface IRulesEngineDAL {
        Dictionary<int, Rule> GetRules(int ID, RuleType ruleType);
    }
}