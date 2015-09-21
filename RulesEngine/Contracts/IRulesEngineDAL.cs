namespace RulesEngine.Contracts {
    using System.Collections.Generic;
    using RulesEngine.Models;

    public interface IRulesEngineDAL
    {
        Dictionary<int, Rule> GetRules(int ID, RuleType ruleType);
    }
}