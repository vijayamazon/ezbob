namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement
{
    using System;
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using StructureMap.Attributes;

    public class MatchBLL<T1, T2> : IMatchBLL<T1,T2>
    {
        [SetterProperty]
        public IExressionBuilderBLL ExressionBuilder { get; set; }
        
        [SetterProperty]
        public IRulesEngineDAL RulesDAL { get; set; }
        
        public Func<T1, T2, bool> Func { get; set; }
        public T1 Source { get; set; }
        public T2 Target { get; set; }
        public void BuildFunc(int investorID, long cashRequestID, RuleType ruleType)
        {
            Dictionary<int, InvestorRule> Rules = RulesDAL.GetRules(investorID, ruleType);
            if ((Rules == null) || Rules.Count ==0) {
                Func = delegate { return true; };
                return;
            }
            Func = ExressionBuilder.CompileRule<T1, T2>(investorID,cashRequestID, Rules);
        }
        public bool IsMatched()
        {
            return Func.Invoke(Source, Target);
        }
    }
}
