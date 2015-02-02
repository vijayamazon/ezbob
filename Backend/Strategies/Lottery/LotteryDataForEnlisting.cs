namespace Ezbob.Backend.Strategies.Lottery {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LotteryDataForEnlisting {
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
			} // set
		} // LotteryEnlistingTypeStr

		public LotteryEnlistingType LotteryEnlistingType { get; private set; }

		public bool Fits(int loanCount, decimal loanAmount) {
			switch (LotteryEnlistingType) {
			case LotteryEnlistingType.Unknown:
				Log.Warn("Unknown lottery enlisting type detected for lottery id {0}.", LotteryID);
				return false;

			case LotteryEnlistingType.LoanCount:
				return FitsCount(loanCount);

			case LotteryEnlistingType.LoanOrAmount:
				return FitsCountOrAmount(loanCount, loanAmount);

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // Fits

		private bool FitsCount(int loanCount) {
			if (loanCount <= 0)
				return false;

			return (LoanCount == null) || (LoanCount <= 0) || (loanCount <= LoanCount); 
		} // FitsCount

		private bool FitsCountOrAmount(int loanCount, decimal loanAmount) {
			bool countResult = (LoanCount == null) || (LoanCount <= 0) || (loanCount >= LoanCount);
			bool amountResult = (LoanAmount == null) || (LoanAmount <= 0) || (loanAmount >= LoanAmount);

			return countResult || amountResult;
		} // FitsCountOrAmount

		private static ASafeLog Log {
			get { return Library.Instance.Log; }
		} // Log
	} // class LotteryDataForEnlisting
} // namespace
