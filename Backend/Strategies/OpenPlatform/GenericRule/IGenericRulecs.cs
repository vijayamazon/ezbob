namespace Ezbob.Backend.Strategies.OpenPlatform.GenericRule {
    interface IGenericRules {
        bool InvokeRule(long cashRequestId, int investorID);
    }
}
