namespace EzBob3dParties.Hmrc
{
    using System;
    using EzBobCommon.Currencies;

    public class RtiTaxMonthInfo
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public Money AmountPaid { get; set; }
        public Money AmountDue { get; set; }
    }
}
