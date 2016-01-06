namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts
{
    using System;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;

    public interface IMatchBLL<T1, T2>
    {        
        T1 Source { get; set; }
        T2 Target { get; set; }
        Func<T1, T2, bool> Func { get; set; }
        void BuildFunc(int investorID, long cashRequestID, RuleType ruleType);
        bool IsMatched();

    }
}
