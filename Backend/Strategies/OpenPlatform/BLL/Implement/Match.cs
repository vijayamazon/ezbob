namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement
{
    using System;
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;
    using StructureMap.Attributes;

    public class Match<T1, T2> : IMatch<T1,T2>
    {
        [SetterProperty]
        public IExressionBuilder ExressionBuilder { get; set; }
        [SetterProperty]
        public IRulesEngineDAL RulesDAL { get; set; }
        public Func<T1, T2, bool> Func { get; set; }
        public T1 Source { get; set; }
        public T2 Target { get; set; }
        public void BuildFunc(int investorID, long cashRequestID, RuleType ruleType)
        {
            Dictionary<int, Rule> Rules = RulesDAL.GetRules(investorID, ruleType);
            Func = ExressionBuilder.CompileRule<T1, T2>(investorID,cashRequestID, Rules);
        }
        public bool IsMatched()
        {
            return Func.Invoke(Source, Target);
        }
    }
}
