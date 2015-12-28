namespace Ezbob.Backend.Strategies.OpenPlatform.GenericRule {
    interface IGenericRules {
        bool InvokeRule(int cashRequestId, int investorID);
    }
}
