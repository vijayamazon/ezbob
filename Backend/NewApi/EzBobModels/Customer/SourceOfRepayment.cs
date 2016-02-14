namespace EzBobModels.Customer {
    /// <summary>
    /// The values are ids taken from 'CustomerSourceOfRepayment' table
    /// </summary>
    public enum SourceOfRepayment {
        OngoingSourceOfIncome = 1,
        NewSourcesOfIncome = 2,
        NewDebt = 3,        
        SaleOfFixedAssets = 4,
        Other = 5
    }
}
