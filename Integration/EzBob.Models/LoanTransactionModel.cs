using System;

namespace EzBob.Web.Areas.Customer.Models
{
    public class LoanTransactionModel
    {
        public int Id { get; set; }
        public DateTime PostDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string TrackingNumber { get; set; }
        public string PacnetStatus { get; set; }
        public string PaypointId { get; set; }
        public string Ip { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Fees { get; set; }
        public decimal Rollover { get; set; }
        public string StatusDescription { get; set; }
        public decimal Balance { get; set; }
        public decimal LoanRepayment { get; set; }
    }
}