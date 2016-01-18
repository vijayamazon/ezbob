namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement {
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Database;

    public class RulesEngineDAL : IRulesEngineDAL {
        public Dictionary<int, InvestorRule> GetRules(int InvestorId, RuleType ruleType) {
            var queryParameters = ruleType == RuleType.System ? new[] { new QueryParameter("RuleType", (int)ruleType) } : new[] { new QueryParameter("RuleType", (int)ruleType), new QueryParameter("InvestorId", InvestorId) };
            var rules = Library.Instance.DB.Fill<InvestorRule>("I_GetInvestorRules", CommandSpecies.StoredProcedure, queryParameters);
            return rules.ToDictionary(x => x.RuleID, x => x);          
        }
    }
}
