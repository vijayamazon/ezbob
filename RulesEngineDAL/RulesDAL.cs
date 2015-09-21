namespace RulesEngineDAL
{
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.Investor;
    using Ezbob.Backend.ModelsWithDB.Rules;

    public interface IRulesEngineDAL
    {
        Dictionary<int, Rule> GetRules(int ID, RuleType ruleType);
    }

    public class RulesEngineDAL : IRulesEngineDAL
    {
        public Dictionary<int, Rule> GetRules(int ID, RuleType ruleType) {
            return null;
        }
    }
}
