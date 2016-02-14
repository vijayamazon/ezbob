namespace EzBobModels.Customer {
    using System;

    /// <summary>
    /// Look at 'CustomerRequestedLoan' table
    /// </summary>
    public class CustomerRequestedLoan {
        public int? Id { get; set; }
        public int CustomerId { get; set; }
        public LoanRequestReason? ReasonId { get; set; }
        public SourceOfRepayment? SourceOfRepaymentId { get; set; }
        public DateTime Created { get; set; }
        public decimal? Amount { get; set; }
        public string OtherReason { get; set; }
        public string OtherSourceOfRepayment { get; set; }
    }
}
