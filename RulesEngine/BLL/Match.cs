namespace RulesEngine.BLL
{
    using System;
    using System.Collections.Generic;
    using RulesEngine.Contracts;
    using RulesEngine.Models;
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
        public  void BuildFunc(int userId, RuleType ruleType)
        {
            Dictionary<int, Rule> Rules = RulesDAL.GetRules(userId, ruleType);
            Func = ExressionBuilder.CompileRule<T1, T2>(userId, Rules);
        }
        public bool IsMatched()
        {
            return Func.Invoke(Source, Target);
        }
    }
}
