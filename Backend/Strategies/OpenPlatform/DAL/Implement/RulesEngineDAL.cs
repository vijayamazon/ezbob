namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement {
	using System;
	using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Database;
	using log4net;

	public class RulesEngineDAL : IRulesEngineDAL {
        public Dictionary<int, InvestorRule> GetRules(int InvestorId, RuleType ruleType) {
	        try {
		        var queryParameters = ruleType == RuleType.System ? new[] {
			        new QueryParameter("RuleType", (int)ruleType)
		        } : new[] {
			        new QueryParameter("RuleType", (int)ruleType), new QueryParameter("InvestorId", InvestorId)
		        };
		        var rules = Library.Instance.DB.Fill<InvestorRule>("I_GetInvestorRules", CommandSpecies.StoredProcedure, queryParameters);
		        return rules.ToDictionary(x => x.RuleID, x => x);
			} catch (Exception ex) {
				Log.ErrorFormat("Failed to retrieve I_GetInvestorRules {0} {1}", InvestorId, ruleType);
				return new Dictionary<int, InvestorRule>();
			}
        }

		protected static ILog Log = LogManager.GetLogger(typeof(RulesEngineDAL));

    }
}
