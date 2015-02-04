namespace Ezbob.Backend.Strategies.Lottery {
	using System;
	using System.Collections.Generic;
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

		public bool? IsForNew { get; set; }

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		[FieldName("LotteryEnlistingType")]
		public string LotteryEnlistingTypeStr {
			get {
				if (Constructed == null)
					return LotteryEnlistingType.Unknown.ToString();

				if (Constructed.GetType() == typeof(CountAmount))
					return LotteryEnlistingType.Constructed.ToString();

				return Constructed.ToString();
			} // get
			set {
				LotteryEnlistingType lt;

				if (!Enum.TryParse(value, true, out lt))
					lt = LotteryEnlistingType.Unknown;

				switch (lt) {
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
					lt,
					Constructed
				);
			} // set
		} // LotteryEnlistingTypeStr

		public IFittable Constructed { get; private set; }

		/// <summary>
		/// Checks whether user fits promotion by his loan count and loan amount.
		/// </summary>
		/// <param name="allLoans">List of all the loans taken by customer/broker leads.</param>
		/// <returns></returns>
		public bool Fits(List<LoanData> allLoans) {
			if (Constructed == null) {
				Log.Debug("User ain't no fits lottery '{0}': no condition detected.", LotteryID);
				return false;
			} // if

			var preTerm = new LoanStat();
			var inTerm = new LoanStat();

			foreach (LoanData ld in allLoans) {
				if (ld.IssuedTime < StartDate)
					preTerm.Add(ld);
				else if (ld.IssuedTime <= EndDate)
					inTerm.Add(ld);
			} // for each

			if (IsForNew.HasValue) {
				if (IsForNew.Value) {
					if (preTerm.HasLoans) {
						Log.Debug(
							"User ain't no fits lottery '{0}': the lottery is for new users while user already has loans.",
							LotteryID
						);
						return false;
					} // if
				} else {
					if (!preTerm.HasLoans) {
						Log.Debug(
							"User ain't no fits lottery '{0}': the lottery is for old users while user has no loans.",
							LotteryID
						);
						return false;
					} // if
				} // if
			} // if

			Constructed.Init(inTerm.Count, LoanCount, inTerm.Amount, LoanAmount);

			bool fits = Constructed.Calculate();

			Log.Debug(
				"User {2}fits lottery '{0}' on condition '{1}'.",
				LotteryID,
				Constructed,
				fits ? string.Empty : "ain't no "
			);

			return fits;
		} // Fits

		private class LoanStat {
			public LoanStat() {
				Count = 0;
				Amount = 0;
			} // constructor

			public bool HasLoans {
				get { return Count > 0; }
			} // HasLoans

			public void Add(LoanData ld) {
				Count++;
				Amount += ld.Amount;
			} // Add

			public int Count { get; private set; }
			public decimal Amount { get; private set; }
		} // LoanStat

		private void TryToConstruct(string formulaPattern) {
			if (string.IsNullOrWhiteSpace(formulaPattern)) {
				Constructed = null;
				return;
			} // if

			var re = new Regex("^(Max|Min)Count(And|Or)(Max|Min)Amount$");

			Match match = re.Match(formulaPattern.Trim());

			if (!match.Success) {
				Constructed = null;
				return;
			} // if

			IFittable leftTerm = (match.Groups[1].Value == "Max") ? (IFittable)new MaxCount() : (IFittable)new MinCount();
			IFittable rightTerm = (match.Groups[3].Value == "Max") ? (IFittable)new MaxAmount() : (IFittable)new MinAmount();

			Type opType = (match.Groups[2].Value == "And") ? typeof(And) : typeof(Or);

			Constructed = new CountAmount(opType, leftTerm, rightTerm);
		} // TryToConstruct

		private static ASafeLog Log {
			get { return new SafeLog(); }
		} // Log

		private enum LotteryEnlistingType {
			Unknown,
			MinCount,
			MaxCount,
			MinAmount,
			MaxAmount,
			Constructed,
		} // enum LotteryEnlistingType
	} // class LotteryDataForEnlisting
} // namespace
