namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LoadOfferRanges {
		public LoadOfferRanges(
			int approvedAmount,
			bool hasLoans,
			Medal medal,
			AConnection db,
			ASafeLog log
		) {
			this.approvedAmout = approvedAmount;
			this.hasLoans = hasLoans;
			this.medal = medal;
			this.db = db;
			this.log = log.Safe();

			LoadLoanTypeID = true;
			LoanTypeID = 0;
			InterestRate = new MinMax();
			SetupFee = new MinMax();

			Success = false;
		} // constructor

		public LoadOfferRanges Execute() {
			Success = LoadLoanType() && LoadInterestRate() && LoadSetupFee();
			return this;
		} // Execute

		public bool LoadLoanTypeID { get; set; }

		public bool Success { get; private set; }

		public MinMax InterestRate { get; private set; }
		public MinMax SetupFee { get; private set; }
		public int LoanTypeID { get; private set; }

		public class MinMax {
			public MinMax() : this(0, 0) {} // constructor

			public MinMax(decimal min, decimal max) {
				Min = min;
				Max = max;
			} // constructor

			public decimal Min { get; private set; }
			public decimal Max { get; private set; }
		} // class MinMax

		private bool LoadLoanType() {
			if (!LoadLoanTypeID)
				return true;

			var sr = this.db.GetFirst(
				"GetLoanTypeAndDefault",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanTypeID")
			);

			LoanTypeID = sr.IsEmpty ? 0 : sr["DefaultLoanTypeID"];

			return (LoanTypeID > 0);
		} // LoadLoanType

		private bool LoadInterestRate() {
			if (this.medal == Medal.NoClassification)
				return false;

			SafeReader sr = this.db.GetFirst(
				"AV_OfferInterestRateRange",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Medal", this.medal.ToString())
				);

			if (sr.IsEmpty) {
				this.log.Alert(
					"Failed to load medal {0} interest rate range.",
					this.medal
					);
				return false;
			} // if

			InterestRate = new MinMax(sr["MinInterestRate"] / 100.0M, sr["MaxInterestRate"] / 100.0M);
			return true;
		} // LoadInterestRate

		private bool LoadSetupFee() {
			var sr = this.db.GetFirst(
				"LoadOfferRanges",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Amount", this.approvedAmout),
				new QueryParameter("@IsNewLoan", !this.hasLoans)
			);

			if (sr.IsEmpty) {
				this.log.Alert(
					"Failed to load set up fee range for a {0} amount of {1}.",
					this.hasLoans ? "repeating" : "new",
					this.approvedAmout
				);
				return false;
			} // if

			SetupFee = new MinMax(sr["MinSetupFee"], sr["MaxSetupFee"]);

			return true;
		} // LoadSetupFee

		private readonly int approvedAmout;
		private readonly bool hasLoans;
		private readonly Medal medal;
		private readonly AConnection db;
		private readonly ASafeLog log;
	} // class LoadOfferRanges
} // namespace
