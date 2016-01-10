namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement {
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Database;

    public class RulesEngineDAL : IRulesEngineDAL {
        public Dictionary<int, InvestorRule> GetRules(int InvestorId, RuleType ruleType) {

            //TODO: remove mock
            var rules = GetRulesList(InvestorId, ruleType);

            switch (ruleType) {
                    case RuleType.System:
                        return rules.Where(x => x.RuleType == (int)RuleType.System).ToDictionary(x => x.RuleID, x => x);
                    case RuleType.UnderWriter:
                        return rules.Where(x => x.RuleType == (int)RuleType.UnderWriter && x.InvestorID == InvestorId).ToDictionary(x => x.RuleID, x => x);
                    case RuleType.Investor:
                        return rules.Where(x => x.RuleType == (int)RuleType.Investor && x.InvestorID == InvestorId).ToDictionary(x => x.RuleID, x => x);
            }
            return null;
        }


        private List<InvestorRule> GetRulesList(int InvestorId, RuleType ruleType) {

            int? investorID = null;
            if (ruleType != RuleType.System)
                investorID = InvestorId;
            var rules = Library.Instance.DB.Fill<InvestorRule>("I_GetInvestorRules", CommandSpecies.StoredProcedure,
                new QueryParameter("InvestorId", investorID),
                new QueryParameter("RuleType", (int)ruleType)
            );
            return rules;
        }


    }
}
