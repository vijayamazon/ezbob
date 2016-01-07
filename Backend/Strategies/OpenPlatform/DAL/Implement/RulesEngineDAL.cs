namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement {
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;

    public class RulesEngineDAL : IRulesEngineDAL {
        public Dictionary<int, InvestorRule> GetRules(int InvestorId, RuleType ruleType) {

            //TODO: remove mock
            var rules =  GetRules();

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


        private List<InvestorRule> GetRules() {

            //int? investorID = null;
            //if (ruleType != RuleType.System)
            //    investorID = InvestorId;
            //var rules = Library.Instance.DB.Fill<Rule>("GetInvestorRules", CommandSpecies.StoredProcedure,
            //    new QueryParameter("InvestorId", investorID),
            //    new QueryParameter("RuleType", (int)ruleType)
            //);
            //return rules.ToDictionary(x => x.RuleID, x=> x);
            
            var rulList = new List<InvestorRule>();

            rulList.Add(new InvestorRule {
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

            rulList.Add(new InvestorRule {
                Operator = (int)Operator.LessThan,
                MemberNameSource = "ManagerApprovedSum",
                MemberNameTarget = "Balance",
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 2,
                UserID = 1,
                RuleType = (int)RuleType.System
            });

            rulList.Add(new InvestorRule {
                Operator = (int)Operator.IsTrue,
                MemberNameSource = null,
                MemberNameTarget = null,
                IsRoot = false,
                LeftParamID = -1,
                RightParamID = -1,
                InvestorID = 1,
                RuleID = 3,
                UserID = 1,
                FuncName = "RuleBadgetLevel",
                RuleType = (int)RuleType.System
            });

            return rulList;


           
        }


    }
}
