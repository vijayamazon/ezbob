namespace Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.DAL
{
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.OpenPlatform.RulesEngine.Models;

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
