namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using System.Globalization;
	using DbConstants;
	using Ezbob.Backend.Extensions;
	using Ezbob.Logger;

	public class OfferFee {
		public OfferFee(
			FeeTypes feeType,
			decimal loanAmount,
			decimal? percentOfIssued,
			decimal? absoluteAmount,
			decimal oneTimePartPct,
			decimal distributedPartPct
		) {
			Amount = DetectAmount(loanAmount, percentOfIssued, absoluteAmount);
			FeeType = feeType;
			OneTimePartPercent = oneTimePartPct;
			DistributedPartPercent = distributedPartPct;

			ValidateParts();
		} // constructor

		public decimal Amount { get; private set; }

		public FeeTypes FeeType{ get; private set; }

		public decimal OneTimePartPercent { get; private set; }

		public decimal DistributedPartPercent { get; private set; }

		public OfferFee DeepClone() {
			return new OfferFee(FeeType, Amount, OneTimePartPercent, DistributedPartPercent);
		} // DeepClone

		public override string ToString() {
			return string.Format(
				"{0} of {1} (1 time {2}, spread {3})",
				FeeType,
				Amount.ToString("C2", Culture),
				OneTimePartPercent.ToString("P2", Culture),
				DistributedPartPercent.ToString("P2", Culture)
			);
		} // ToString

		private OfferFee(
			FeeTypes feeType,
			decimal amount,
			decimal oneTimePartPct,
			decimal distributedPartPct
		) {
			Amount = amount;
			FeeType = feeType;
			OneTimePartPercent = oneTimePartPct;
			DistributedPartPercent = distributedPartPct;
		} // constructor

		private void ValidateParts() {
			bool isGood =
				(0 <= OneTimePartPercent) && (OneTimePartPercent <= 1) &&
				(0 <= DistributedPartPercent) && (DistributedPartPercent <= 1) &&
				(OneTimePartPercent + DistributedPartPercent == 1m);

			if (isGood)
				return;

			Log.Alert(
				"Offer fee dropped: cannot spread with one time part of {0} and distributed part of {1}.",
				OneTimePartPercent.ToString("P2", Culture),
				DistributedPartPercent.ToString("P2", Culture)
			);

			Amount = 0;
		} // ValidateParts

		private static decimal DetectAmount(decimal loanAmount, decimal? percentOfIssued, decimal? absoluteFeeAmount) {
			decimal absoluteAmount = absoluteFeeAmount > 0 ? absoluteFeeAmount.Value : 0;

			decimal relativeAmount = ((0 <= percentOfIssued) && (percentOfIssued <= 1m) && (loanAmount > 0))
				? loanAmount * percentOfIssued.Value
				: 0;

			return Math.Max(absoluteAmount, relativeAmount);
		} // DetectAmount

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture

		private static ASafeLog Log {
			get { return Library.Instance.Log; }
		} // Log
	} // class OfferFee
} // namespace
