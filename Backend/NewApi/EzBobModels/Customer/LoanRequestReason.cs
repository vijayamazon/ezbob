namespace EzBobModels.Customer {
    /// <summary>
    /// The values are Ids taken from "CustomerReason" table
    /// </summary>
    public enum LoanRequestReason {
        BridgingTemporaryCashShortageDueToSlowCollections = 1,
        BridgingTemporaryCashShortageSueToVAT,
        BridgingTemporaryCashShortageDueToOtherUnexpectedExpenses,
        BridgingSeasonality,
        CapitalExpenditure,
        Debtrepayment,
        Other
    }
}
