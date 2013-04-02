using System.Collections.Generic;

namespace PaymentServices.Calculators
{
    public class PayFastResult
    {
        public decimal Saved { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal SavedPounds { get; set; }
        public List<string> TransactionRefNumbers { get; set; }
        public bool RolloverWasPaid { get; set; }
        public string TransactionRefNumbersFormatted
        {
            get { return string.Join(", ", TransactionRefNumbers.ToArray()); }
        }
    }
}