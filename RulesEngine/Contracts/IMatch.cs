namespace RulesEngine.Contracts
{
    using System;
    using RulesEngine.Models;

    public interface IMatch<T1, T2>
    {        
        T1 Source { get; set; }
        T2 Target { get; set; }
        Func<T1, T2, bool> Func { get; set; }
        void BuildFunc(int userId, RuleType ruleType);
        bool IsMatched();

    }
}
