using System;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class LoanHistoryModel
    {
        public int Id { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal Repayments { get; set; }
        public DateTime DateApplyed { get; set; }
        public string DateClosed { get; set; }
        public decimal OnTime { get; set; }
        public decimal Late30 { get; set; }
        public decimal Late60 { get; set; }
        public decimal Late90 { get; set; }
        public decimal Late90Num { get; set; }
        public decimal PastDues { get; set; }
        public string Status { get; set; }
        public decimal Outstanding { get; set; }
    }
}