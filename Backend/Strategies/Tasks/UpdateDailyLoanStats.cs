namespace Ezbob.Backend.Strategies.Tasks {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using Ezbob.Backend.Extensions;
	using Ezbob.Database;
	using Ezbob.Utils.Lingvo;
	using global::Reports.EarnedInterest;

	/// <summary>
	/// Calculates earned interest per loan on specific date and stores this value in DB.
	/// It is used by BI for calculating earned interest.
	/// </summary>
	public class UpdateDailyLoanStats : AStrategy {
		/// <summary>
		/// Calculates earned interest per loan for yesterday and stores this value in DB.
		/// </summary>
		/// <param name="daysToKeep">How many days of history to keep; 0 or less means keep 'em all.</param>
		public UpdateDailyLoanStats(int daysToKeep) : this(daysToKeep, DateTime.UtcNow.AddDays(-1)) {
		} // constructor

		/// <summary>
		/// Calculates earned interest per loan on specific date and stores this value in DB.
		/// </summary>
		/// <param name="daysToKeep">How many days of history to keep; 0 or less means keep 'em all.</param>
		/// <param name="calculationDate">Calculates earned interest for this date. Null means yesterday.</param>
		public UpdateDailyLoanStats(int daysToKeep, DateTime calculationDate) {
			this.daysToKeep = Math.Max(daysToKeep, 0);
			this.today = calculationDate.Date;
			this.todayStr = this.today.DateStr();
		} // constructor

		public override string Name {
			get { return "Daily update earned interest stats"; }
		} // Name

		public override void Execute() {
			Log.Debug(
				"Update started for date '{0}' keeping {1} history...",
				this.todayStr,
				this.daysToKeep == 0 ? "entire" : Grammar.Number(this.daysToKeep, "day") + " of"
			);

			SortedDictionary<int, decimal> earnedInterestByPeriodsList = new EarnedInterest(
				DB,
				EarnedInterest.WorkingMode.ForPeriod,
				false,
				this.today,
				this.today.AddDays(1),
				Log
			).Run();

			SortedDictionary<int, decimal> earnedInterestBySomeDateList = new EarnedInterest(
				DB,
				EarnedInterest.WorkingMode.ForPeriod,
				true,
				this.today,
				this.today.AddDays(1),
				Log
			).Run();

			decimal totalEarnedInterestByPeriods = 0;
			decimal totalEarnedInterestBySomeDate = 0;

			List<int> union = earnedInterestByPeriodsList.Keys.Union(earnedInterestBySomeDateList.Keys).ToList();

			var updatePkg = new List<UpdatePkgRow>();

			foreach (int loanID in union) {
				decimal byPeriods = earnedInterestByPeriodsList.ContainsKey(loanID)
					? earnedInterestByPeriodsList[loanID]
					: 0;

				decimal bySomeDate = earnedInterestBySomeDateList.ContainsKey(loanID)
					? earnedInterestBySomeDateList[loanID]
					: 0;

				totalEarnedInterestByPeriods += byPeriods;
				totalEarnedInterestBySomeDate += bySomeDate;

				Log.Debug(
					"On {0} for loan ID {1} earned interest is: by periods {2}, by some date {3}.",
					this.todayStr,
					loanID,
					byPeriods.ToString("C2", Library.Instance.Culture),
					bySomeDate.ToString("C2", Library.Instance.Culture)
				);

				updatePkg.Add(new UpdatePkgRow {
					LoanID = loanID,
					TheDate = this.today,
					EarnedInterestByPeriods = byPeriods,
					EarnedInterestBySomeDate = bySomeDate,
				});
			} // for each

			Log.Debug(
				"On {0} for {1} total earned interest is: by periods {2}, by some date {3}.",
				this.todayStr,
				Grammar.Number(union.Count, "loan"),
				totalEarnedInterestByPeriods.ToString("C2", Library.Instance.Culture),
				totalEarnedInterestBySomeDate.ToString("C2", Library.Instance.Culture)
			);

			DB.ExecuteNonQuery(
				"UpdateDailyLoanStats",
				CommandSpecies.StoredProcedure,
				new QueryParameter("DaysToKeep", this.daysToKeep),
				new QueryParameter("Now", this.today),
				DB.CreateTableParameter<UpdatePkgRow>("UpdatePkg", updatePkg)
			);

			Log.Debug(
				"Update complete for date '{0}' keeping {1} history...",
				this.todayStr,
				this.daysToKeep == 0 ? "entire" : Grammar.Number(this.daysToKeep, "day") + " of"
			);
		} // Execute

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		// "Gets" are used implicitly via reflection while sending data to DB.
		private class UpdatePkgRow {
			public int LoanID { get; set; }
			public DateTime TheDate { get; set; }
			public decimal EarnedInterestByPeriods { get; set; }
			public decimal EarnedInterestBySomeDate { get; set; }
		} // class UpdatePkgRow

		private readonly int daysToKeep;
		private readonly DateTime today;
		private readonly string todayStr;
	} // class UpdateDailyLoanStats
} // namespace

