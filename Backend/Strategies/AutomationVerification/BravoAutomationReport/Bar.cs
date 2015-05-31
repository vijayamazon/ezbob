namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;

	public class Bar : AStrategy {
		public Bar(DateTime? startTime, DateTime? endTime) {
			this.spLoad = new SpLoadCashRequestsForBravoAutomationReport(DB, Log);
			this.customerDecisions = new SortedDictionary<int, CustomerDecisions>();

			StartTime = startTime;
			EndTime = endTime;

			this.tag = string.Format(
				"#BravoAutoRpt_{0}_{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", Library.Instance.Culture),
				Guid.NewGuid().ToString("N")
			);

			this.now = DateTime.UtcNow;
		} // constructor

		public override string Name {
			get { return "Bravo automation report"; }
		} // Name

		public DateTime? StartTime {
			get { return this.spLoad.StartTime; }
			private set { this.spLoad.StartTime = value; }
		} // StartTime

		public DateTime? EndTime {
			get { return this.spLoad.EndTime; }
			private set { this.spLoad.EndTime = value; }
		} // EndTime

		public override void Execute() {
			LoadCashRequests();
			RunAutomation();
		} // Execute

		private void ProcessManualDecision(SafeReader sr) {
			int customerID = sr["CustomerID"];

			ManualDecision md = sr.Fill<ManualDecision>();

			if (!this.customerDecisions.ContainsKey(customerID))
				this.customerDecisions[customerID] = new CustomerDecisions(customerID, sr["IsAlibaba"]);

			this.customerDecisions[customerID].ManualDecisions.Add(md);

			this.pc++;
		} // ProcessManualDecision

		private void LoadCashRequests() {
			this.pc = new ProgressCounter("{0} manual decisions processed.", Log, 25);

			spLoad.ForEachRowSafe(ProcessManualDecision);

			this.pc.Log();

			Log.Info("{0} loaded.", Grammar.Number(this.customerDecisions.Count, "customer"));
		} // LoadCashRequests

		private void RunAutomation() {
			this.pc = new ProgressCounter("{0} customers processed.", Log, 10);

			foreach (CustomerDecisions cd in this.customerDecisions.Values) {
				bool doNext = true;

				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				// Just for code consistency.
				if (doNext) { // Re-reject area
					if (IsRerejected(cd.CustomerID)) {
						doNext = false;
						cd.AutoDecision = new AutoDecision(DecisionActions.ReReject);
					} // if
				} // if; Re-reject area

				if (doNext) {
					if (!cd.IsAlibaba) {
						if (IsRejected(cd.CustomerID)) {
							doNext = false;
							cd.AutoDecision = new AutoDecision(DecisionActions.Reject);
						} // if
					} // if not alibaba
				} // if; Reject area

				Ezbob.Backend.Strategies.MedalCalculations.MedalResult medal = null;

				if (doNext) { // Medal area
					var instance = new Ezbob.Backend.Strategies.MedalCalculations.CalculateMedal(
						cd.CustomerID,
						this.now,
						false,
						true
					);
					instance.Tag = this.tag;
					instance.Execute();
					medal = instance.Result;
				} // if; Medal area

				int offeredCreditLine = medal == null ? 0 : medal.RoundOfferedAmount();

				if (doNext)
					offeredCreditLine = CapOffer(cd.CustomerID, offeredCreditLine);

				if (doNext) { // Re-approve area
					if (IsReapproved(cd.CustomerID)) {
						doNext = false;
						cd.AutoDecision = new AutoDecision(DecisionActions.ReApprove);
					} // if
				} // if; Re-approve area

				if (doNext) { // Approve area
					if (IsApproved(cd.CustomerID, offeredCreditLine, medal)) {
						doNext = false;
						cd.AutoDecision = new AutoDecision(DecisionActions.Approve);
					} // if
				} // if; Approve area

				if (doNext)
					cd.AutoDecision = new AutoDecision(null);

				this.pc++;
			} // for each

			this.pc.Log();
		} // RunAutomation

		private bool IsRerejected(int customerID) {
			var agent = new AutomationCalculator.AutoDecision.AutoReRejection.Agent(customerID, this.now, DB, Log);
			agent.Init();
			agent.MakeDecision();

			agent.Trail.Save(DB, null, tag: this.tag);

			return agent.Trail.HasDecided;
		} // IsRerejected

		private bool IsRejected(int customerID) {
			AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent agent =
				new AutomationCalculator.AutoDecision.AutoRejection.RejectionAgent(DB, Log, customerID);

			agent.MakeDecision(agent.GetRejectionInputData(this.now));

			agent.Trail.Save(DB, null, tag: this.tag);

			return agent.Trail.HasDecided;
		} // IsRejected

		private int CapOffer(int customerID, int offeredCreditLine) {
			Log.Debug("Capping offer for customer {0} with uncapped offer {1}.", customerID, offeredCreditLine);

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerID)
			);

			if (isHomeOwnerAccordingToLandRegistry) {
				Log.Info("Capped for home owner according to land registry");
				offeredCreditLine = Math.Min(offeredCreditLine, MaxCapHomeOwner);
			} else {
				Log.Info("Capped for not home owner");
				offeredCreditLine = Math.Min(offeredCreditLine, MaxCapNotHomeOwner);
			} // if

			Log.Debug("Capped offer for customer {0} is {1}.", customerID, offeredCreditLine);

			return offeredCreditLine;
		} // CapOffer

		private bool IsReapproved(int customerID) {
			var agent = new AutomationCalculator.AutoDecision.AutoReApproval.Agent(
				DB,
				Log,
				customerID,
				this.now
			);

			agent.MakeDecision(agent.GetInputData());

			agent.Trail.Save(DB, null, tag: this.tag);

			return agent.Trail.HasDecided;
		} // IsReapproved

		private bool IsApproved(
			int customerID,
			int offeredCreditLine,
			Ezbob.Backend.Strategies.MedalCalculations.MedalResult medal
		) {
			if (medal == null)
				return false;

			var agent = new AutomationCalculator.AutoDecision.AutoApproval.Agent(
				customerID,
				offeredCreditLine,
				(AutomationCalculator.Common.Medal)medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)medal.TurnoverType,
				DB,
				Log
			);

			agent.Init();

			agent.MakeDecision();

			agent.Trail.Save(DB, null, tag: this.tag);

			return agent.Trail.HasDecided;
		} // IsApproved

		private static int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }

		private static int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }

		private ProgressCounter pc;

		private readonly SpLoadCashRequestsForBravoAutomationReport spLoad;

		private readonly SortedDictionary<int, CustomerDecisions> customerDecisions;

		private readonly string tag;
		private readonly DateTime now;
	} // class Bar
} // namespace
