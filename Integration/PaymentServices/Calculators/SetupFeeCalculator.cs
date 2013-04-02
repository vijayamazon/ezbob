using System;

namespace PaymentServices.Calculators
{
    public class SetupFeeCalculator
    {
        private int _maxFee = 30;
        private decimal _feePercent = 0.008M;

        public int MaxFee
        {
            get { return _maxFee; }
            set { _maxFee = value; }
        }

        public decimal FeePercent
        {
            get { return _feePercent; }
            set { _feePercent = value; }
        }

        public decimal Calculate(decimal amount)
        {
            return Math.Max(Math.Floor(amount * FeePercent), MaxFee);
        }
    }
}