namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using EzBob.Backend.ModelsWithDB;

	public class BuildMultiBrandLoanSummary : AStrategy {
		public BuildMultiBrandLoanSummary(int customerID) {
			this.customerID = customerID;
			this.brandedLoans = new SortedDictionary<string, List<MultiBrandLoanModel>>();
			Result = new MultiBrandLoanSummary();
		} // constructor

		public override string Name {
			get { return "BuildMultiBrandLoanSummary"; }
		} // Name

		public MultiBrandLoanSummary Result { get; private set; }

		public override void Execute() {
			DB.ForEachRowSafe(
				sr => {
					string rowType = sr["RowType"];

					switch (rowType) {
					case "origin":
						string origin = sr["Origin"];

						if (!this.brandedLoans.ContainsKey(origin))
							this.brandedLoans[origin] = new List<MultiBrandLoanModel>();
						break;

					case "loan":
						ProcessLoan(sr.Fill<MultiBrandLoanModel>());
						break;
					} // switch
				},
				"BuildMultiBrandAlert",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			Result.OriginCount = this.brandedLoans.Keys.Count;

			decimal total = 0;

			foreach (KeyValuePair<string, List<MultiBrandLoanModel>> pair in this.brandedLoans) {
				string origin = pair.Key;

				foreach (MultiBrandLoanModel loan in pair.Value) {
					total += loan.OpenBalance;

					Result.Loans.Add(string.Format(
						"{0} ({1} at {2})",
						loan.OpenBalance.ToString("C2", Library.Instance.Culture),
						loan.TimeLeft,
						origin
					));
				} // for each loan
			} // for each brand

			if (total > 0)
				Result.Loans.Add(string.Format("Total {0}", total.ToString("C2", Library.Instance.Culture)));
			else
				Result.Loans.Add("No open loans.");
		} // Execute

		private void ProcessLoan(MultiBrandLoanModel loan) {
			if (!this.brandedLoans.ContainsKey(loan.Origin))
				this.brandedLoans[loan.Origin] = new List<MultiBrandLoanModel>();

			this.brandedLoans[loan.Origin].Add(loan);
		} // ProcessLoan

		private readonly SortedDictionary<string, List<MultiBrandLoanModel>> brandedLoans; 

		private readonly int customerID;
	} // class BuildMultiBrandLoanSummary
} // namespace

