namespace Ezbob.Backend.Strategies.AutomationVerification {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Database;
	using Ezbob.Utils;

	public class VerifyMedal : AStrategy {
		public VerifyMedal(int topCount, int lastCheckedID, bool includeTest, DateTime? calculationTime) {
			this.tag = "#VerifyMedal_" +
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture) + "_" +
				Guid.NewGuid().ToString("N");

			this.topCount = topCount;
			this.lastCheckedID = lastCheckedID;
			this.includeTest = includeTest;
			this.calculationTime = calculationTime ?? DateTime.UtcNow;

			this.customerCount = null;

			this.condition = new Condition(this);
		} // constructor

		public override string Name {
			get { return "VerifyMedal"; }
		} // Name

		public override void Execute() {
			var customerIDs = new List<int>[3];

			for (int i = 0; i < customerIDs.Length; i++)
				customerIDs[i] = new List<int>();

			int j = 0;

			DB.ForEachRowSafe(
				sr => {
					customerIDs[j].Add(sr["Id"]);

					j++;

					if (j == customerIDs.Length)
						j = 0;
				},
				this.condition.Apply("c.Id", true),
				CommandSpecies.Text
			);

			this.progressCounter = new ProgressCounter("{0} out of " + CustomerCount + " customers processed.", Log, 10);

			if (customerIDs.Length == 1)
				DoCustomerList(customerIDs[0]);
			else
				customerIDs.AsParallel().ForAll(DoCustomerList);

			this.progressCounter.Log();
		} // Execute

		private int CustomerCount {
			get {
				if (this.customerCount != null)
					return this.customerCount.Value;

				int loadedCount = DB.ExecuteScalar<int>(this.condition.Apply("COUNT(*)", false), CommandSpecies.Text);

				this.customerCount = (this.topCount > 0) ? Math.Min(this.topCount, loadedCount) : loadedCount;

				return this.customerCount.Value;
			} // get
		} // CustomerCount

		private void DoCustomerList(List<int> lst) {
			foreach (int customerID in lst) {
				try {
					new CalculateMedal(customerID, null, null, this.calculationTime, false, true) { Tag = this.tag, }.Execute();
				} catch (Exception e) {
					Log.Alert(
						e,
						"Medal verification failed for customer {0} and calculation time {1}.",
						customerID,
						this.calculationTime.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
					);
				} // try

				this.progressCounter++;
			} // for each
		} // DoCustomerList

		private class Condition {
			public Condition(VerifyMedal vm) {
				this.verifyMedal = vm;
			} // constructor

			public string Apply(string selectedFields, bool withOrderBy) {
				return string.Format(
					QueryTemplate,
					selectedFields,
					withOrderBy ? Top : string.Empty,
					LastChecked,
					WithTest,
					withOrderBy ? OrderBy : string.Empty
				);
			} // Apply

			private string Top {
				get {
					return (this.verifyMedal.topCount > 0) ? "TOP " + this.verifyMedal.topCount : string.Empty;
				} // get
			} // Top

			private string LastChecked {
				get {
					return (this.verifyMedal.lastCheckedID > 0)
						? "AND c.Id < " + this.verifyMedal.lastCheckedID
						: string.Empty;
				} // get
			} // LastChecked

			private string WithTest {
				get {
					return this.verifyMedal.includeTest ? "AND c.IsTest = 0" : string.Empty;
				} // get
			} // WithTest

			private readonly VerifyMedal verifyMedal;

			private const string QueryTemplate = "SELECT {1} {0} FROM Customer c WHERE c.WizardStep = 4 {2} {3} {4}";
			private const string OrderBy = "ORDER BY c.Id DESC";
		} // class Condition

		private readonly Condition condition;

		private readonly int topCount;
		private readonly int lastCheckedID;
		private readonly bool includeTest;
		private readonly DateTime calculationTime;

		private int? customerCount;
		private ProgressCounter progressCounter;
		private readonly string tag;
	} // class VerifyMedal
} // namespace

