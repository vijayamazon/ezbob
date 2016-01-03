namespace EzBob3dParties.HMRC
{
    using EzBobCommon.Currencies;

    class RtiTaxMonthSection
    {
        public int DayStart { get; set; }
        public int MonthStart { get; set; }

        public int DayEnd { get; set; }
        public int MonthEnd { get; set; }

        public Money AmountPaid { get; set; }
        public Money AmountDue { get; set; }
    }
}
