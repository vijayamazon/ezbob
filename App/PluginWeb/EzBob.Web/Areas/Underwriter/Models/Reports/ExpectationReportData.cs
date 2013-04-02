using System;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class ExpectationReportData
    {
        private PaymentItem _before = new PaymentItem();
        private PaymentItem _expected = new PaymentItem();
        private PaymentItem _paid = new PaymentItem();
        private PaymentItem _after = new PaymentItem();
        
        public DateTime Date { get; set; }
        public string LoanRef { get; set; }
        public string CustomerName { get; set; }
        public DateTime OriginationDate { get; set; }

        public PaymentItem Before
        {
            get { return _before; }
            set { _before = value; }
        }

        public PaymentItem Expected
        {
            get { return _expected; }
            set { _expected = value; }
        }

        public PaymentItem Paid
        {
            get { return _paid; }
            set { _paid = value; }
        }

        public PaymentItem After
        {
            get { return _after; }
            set { _after = value; }
        }

        public decimal Variance { get; set; }
        public string Status
        {
            get
            {
                if (Paid.Total == 0) return "Not paid"; 
                if (Variance == 0) return "Fully paid on time";
                if (Variance > 0) return "Paid partialy on time";
                if (Paid.Total > 0 && Expected.Total == 0) return "Early repayment";
                //Early repayment
                //Late repayment - partial
                return "Unknown";
            }
        }
    }

    public class PaymentItem
    {
        public decimal Principal { get; set; }
        public decimal Balance { get; set; }
        public decimal Interest { get; set; }
        public decimal Fees { get; set; }
        public decimal Total { get; set; }
    }
}