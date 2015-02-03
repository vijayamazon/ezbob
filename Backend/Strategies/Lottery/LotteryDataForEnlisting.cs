namespace Ezbob.Backend.Strategies.Lottery {
	using System;
	using System.Text.RegularExpressions;
	using Ezbob.Backend.Strategies.Lottery.FitConditions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Formula.Boolean;

	public class LotteryDataForEnlisting {
		public long LotteryID { get; set; }
		public int? LoanCount { get; set; }
		public decimal? LoanAmount { get; set; }

		public int LotteryEnlistingTypeID { get; set; }

		[FieldName("LotteryEnlistingType")]
		public string LotteryEnlistingTypeStr {
			get { return LotteryEnlistingType.ToString(); }
			set {
				LotteryEnlistingType lt;
				LotteryEnlistingType = Enum.TryParse(value, true, out lt) ? lt : LotteryEnlistingType.Unknown;

				switch (LotteryEnlistingType) {
				case LotteryEnlistingType.MaxAmount:
					Constructed = new MaxAmount();
					break;

				case LotteryEnlistingType.MinAmount:
					Constructed = new MinAmount();
					break;

				case LotteryEnlistingType.MaxCount:
					Constructed = new MaxCount();
					break;

				case LotteryEnlistingType.MinCount:
					Constructed = new MinCount();
					break;

				case LotteryEnlistingType.Unknown:
					TryToConstruct(value);
					break;
				} // switch

				Log.Debug(
					"From lottery enlisting pattern {0}: lottery enlisting type set to {1}, formula constructed is {2}.",
					value,
					LotteryEnlistingType,
					Constructed
				);
			} // set
		} // LotteryEnlistingTypeStr

		public LotteryEnlistingType LotteryEnlistingType { get; private set; }

		public IFittable Constructed { get; private set; }

		/// <summary>
		/// Checks whether user fits promotion by his loan count and loan amount.
		/// </summary>
		/// <param name="loanCount">Number of loans taken since promotion start date.</param>
		/// <param name="loanAmount">Amount of loans taken since promotion start date.</param>
		/// <returns></returns>
		public bool Fits(int loanCount, decimal loanAmount) {
			if (Constructed == null)
				return false;

			Constructed.Init(loanCount, LoanCount, loanAmount, LoanAmount);

			return Constructed.Calculate();
		} // Fits

		private void TryToConstruct(string formulaPattern) {
			if (string.IsNullOrWhiteSpace(formulaPattern)) {
				Constructed = null;
				LotteryEnlistingType = LotteryEnlistingType.Unknown;
				return;
			} // if

			var re = new Regex("^(Max|Min)Count(And|Or)(Max|Min)Amount$");

			Match match = re.Match(formulaPattern.Trim());

			if (!match.Success) {
				Constructed = null;
				LotteryEnlistingType = LotteryEnlistingType.Unknown;
				return;
			} // if

			IFittable leftTerm = (match.Groups[1].Value == "Max") ? (IFittable)new MaxCount() : (IFittable)new MinCount();
			IFittable rightTerm = (match.Groups[3].Value == "Max") ? (IFittable)new MaxAmount() : (IFittable)new MinAmount();

			Type opType = (match.Groups[2].Value == "And") ? typeof(And) : typeof(Or);

			Constructed = new CountAmount(opType, leftTerm, rightTerm);
			LotteryEnlistingType = LotteryEnlistingType.Constructed;
		} // TryToConstruct

		private static ASafeLog Log {
			get { return Library.Instance.Log; }
		} // Log
	} // class LotteryDataForEnlisting
} // namespace
