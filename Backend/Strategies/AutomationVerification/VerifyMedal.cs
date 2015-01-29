namespace Ezbob.Backend.Strategies.AutomationVerification {
	using System;
	using System.Globalization;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Database;
	using Ezbob.Utils;

	public class VerifyMedal : AStrategy {
		public VerifyMedal(int topCount, int lastCheckedID, bool includeTest, DateTime? calculationTime) {
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
			this.progressCounter = new ProgressCounter("{0} out of " + CustomerCount + " customers processed.", Log, 10);

			DB.ForEachRowSafe(
				sr => DoCustomer(sr["Id"]),
				this.condition.Apply("c.Id", true),
				CommandSpecies.Text
			);

			this.progressCounter.Log();
		} // Execute

		private int CustomerCount {
			get {
				if (this.customerCount != null)
					return this.customerCount.Value;

				this.customerCount = DB.ExecuteScalar<int>(this.condition.Apply("COUNT(*)", false), CommandSpecies.Text);
				
				return this.customerCount.Value;
			} // get
		} // CustomerCount

		private void DoCustomer(int customerID) {
			try {
				new CalculateMedal(customerID, this.calculationTime, false, true).Execute();
			} catch (Exception e) {
				Log.Alert(
					e,
					"Medal verification failed for customer {0} and calcuation time {1}.",
					customerID,
					this.calculationTime.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);
			} // try

			progressCounter++;
		} // DoCustomer

		private class Condition {
			public Condition(VerifyMedal vm) {
				this.verifyMedal = vm;
			} // constructor

			public string Apply(string selectedFields, bool withOrderBy) {
				return string.Format(
					QueryTemplate,
					selectedFields,
					withOrderBy ? string.Empty : Top,
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
	} // class VerifyMedal
} // namespace

