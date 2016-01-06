namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement {
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;
    using Ezbob.Database;

    public class RulesEngineDAL : IRulesEngineDAL {
        public Dictionary<int, Rule> GetRules(int InvestorId, RuleType ruleType) {

            //TODO: remove mock
            return GetRulesMock();

            int? investorID = null;
            if (ruleType != RuleType.System)
                investorID = InvestorId;
            var rules = Library.Instance.DB.Fill<Rule>("GetInvestorRules", CommandSpecies.StoredProcedure,
                new QueryParameter("InvestorId", investorID),
                new QueryParameter("RuleType", (int)ruleType)
            );
            return rules.ToDictionary(x => x.RuleID, x=> x);
        }


        private Dictionary<int, Rule> GetRulesMock()  {
            
            var rulesDict1 = new Dictionary<int, Rule>();

            rulesDict1.Add(1, new Rule {
                Operator = (int)Operator.And,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = true,
                LeftParamID = 2,
                RightParamID = 3,
                InvestorID = 1,
                RuleID = 1,
                UserID = 1,
                RuleType = (int)RuleType.System
            });

            rulesDict1.Add(2, new Rule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
                MemberNameTarget = "Balance",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 2,
                UserID = 1
            });

            rulesDict1.Add(3, new Rule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
                MemberNameTarget = "DailyInvestmentAllowed",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 3,
                UserID = 1
            });
            return rulesDict1;
        }


    }
}
