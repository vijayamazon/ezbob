namespace AutomationCalculator.OfferCalculation
{
	using Common;
	using Ezbob.Logger;

	public class OfferCalculator
	{
		protected decimal SetupFeeStep {get { return 0.05M; }}
		protected decimal InterestRateStep {get { return 0.005M; }}
		protected ASafeLog Log;

		public OfferCalculator(ASafeLog log) {
			Log = log;
		}

		public OfferOutputModel GetOffer(OfferInputModel input) {
			var dbHelper = new DbHelper(Log);
			var setupFeeRange = dbHelper.GetOfferSetupFeeRange(input.Amount);
			var interestRateRange = dbHelper.GetOfferIneterestRateRange(input.Medal);

			//todo implement

			return new OfferOutputModel();
		}
	}
}
