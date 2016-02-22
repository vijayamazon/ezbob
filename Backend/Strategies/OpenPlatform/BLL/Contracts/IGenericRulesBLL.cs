namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts {
    public interface IGenericRulesBLL {
        bool RuleBadgetLevel(int InvestorId, long CashRequestId, int ruleType);
    }
}

