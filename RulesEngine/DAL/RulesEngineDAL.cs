namespace RulesEngine.DAL
{
    using System.Collections.Generic;
    using RulesEngine.Contracts;
    using RulesEngine.Models;

    public class RulesEngineDAL : IRulesEngineDAL
    {
        public Dictionary<int, Rule> GetRules(int ID, RuleType ruleType) {
            return null;
        }
    }
}
