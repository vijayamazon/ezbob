using System;

namespace EzBobModels.Hmrc
{
    public class RtiTaxMonthEntry
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
        public string CurrencyCode { get; set; }//mandatory
    }
}
