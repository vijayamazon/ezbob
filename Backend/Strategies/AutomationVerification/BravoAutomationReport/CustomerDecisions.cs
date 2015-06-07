namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class CustomerDecisions {
		public CustomerDecisions(int customerID, bool isAlibaba, string tag) {
			Manual = new ManualInterface(this);
			Auto = new AutoInterface(this);

			ManualDecisions = new List<ManualDecision>();
			AutoDecisions = new List<AutoDecision>();

			CustomerID = customerID;
			IsAlibaba = isAlibaba;
			this.tag = tag;

			this.nonAffirmativeTraces = null;
		} // constructor

		public int CustomerID { get; private set; }

		public bool IsAlibaba { get; private set; }

		public List<ManualDecision> ManualDecisions { get; private set; }
		public ManualInterface Manual { get; private set; }

		public List<AutoDecision> AutoDecisions { get; set; }
		public AutoInterface Auto { get; private set; }

		public AutoDecision CurrentAutoDecision { get; set; }

		public class ManualInterface {
			public ManualInterface(CustomerDecisions cd) {
				this.cd = cd;
			} // constructor

			public ManualDecision First { get { return this.cd.ManualDecisions[0]; } }

			public ManualDecision Last { get { return this.cd.ManualDecisions[this.cd.ManualDecisions.Count - 1]; } }

			private readonly CustomerDecisions cd;
		} // ManualInterface

		public class AutoInterface {
			public AutoInterface(CustomerDecisions cd) {
				this.cd = cd;
			} // constructor

			public AutoDecision First { get { return this.cd.AutoDecisions[0]; } }

			public AutoDecision Current { get { return this.cd.CurrentAutoDecision; } }

			public AutoDecision Last { get { return this.cd.AutoDecisions[this.cd.AutoDecisions.Count - 1]; } }

			private readonly CustomerDecisions cd;
		} // AutoInterface

		public void RunAutomation(DateTime now, SortedSet<string> allNonAffirmativeTraces) {
			AutoDecisions.Add(RunAutomationOnce(CustomerID, Manual.First.DecisionTime, IsAlibaba));

			if (ManualDecisions.Count > 1)
				AutoDecisions.Add(RunAutomationOnce(CustomerID, Manual.Last.DecisionTime, IsAlibaba));

			CurrentAutoDecision = RunAutomationOnce(CustomerID, now, IsAlibaba, allNonAffirmativeTraces);
		} // RunAutomation

		public string NonAffirmativeTraceResult(string traceName) {
			if (this.nonAffirmativeTraces == null)
				return null;

			if (this.nonAffirmativeTraces.Contains(traceName))
				return "fail";

			return "ok";
		} // NonAffirmativeTraceResult

		private AutoDecision RunAutomationOnce(
			int customerID,
			DateTime decisionTime,
			bool isAlibaba,
			SortedSet<string> allNonAffirmativeTraces = null
		) {
			AutoDecision result = null;

			bool doNext = true;

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			// Just for code consistency.
			if (doNext) { // Re-reject area
				if (IsRerejected(customerID, decisionTime)) {
					doNext = false;
					result = new AutoDecision(DecisionActions.ReReject);
				} // if
			} // if; Re-reject area

			if (doNext) {
				if (!isAlibaba) {
					if (IsRejected(customerID, decisionTime)) {
						doNext = false;
						result = new AutoDecision(DecisionActions.Reject);
					} // if
				} // if not alibaba
			} // if; Reject area

			AutomationCalculator.Common.MedalOutputModel medal = null;

			if (doNext) { // Medal area
				var verification = new AutomationCalculator.MedalCalculation.MedalChooser(DB, Log);
				medal = verification.GetMedal(customerID, decisionTime);
			} // if; Medal area

			int offeredCreditLine = medal == null
				? 0
				: Ezbob.Backend.Strategies.MedalCalculations.MedalResult.RoundOfferedAmount(medal.OfferedLoanAmount);

			if (doNext)
				offeredCreditLine = CapOffer(customerID, offeredCreditLine);

			if (doNext) { // Re-approve area
				if (IsReapproved(customerID, decisionTime)) {
					doNext = false;
					result = new AutoDecision(DecisionActions.ReApprove);
				} // if
			} // if; Re-approve area

			if (doNext) { // Approve area
				if (IsApproved(customerID, offeredCreditLine, medal, decisionTime, allNonAffirmativeTraces)) {
					doNext = false;
					result = new AutoDecision(DecisionActions.Approve);
				} // if
			} // if; Approve area

			if (doNext)
				result = new AutoDecision(null);

			return result;
		} // RunAutomationOnce

		private bool IsRerejected(int customerID, DateTime decisionTime) {
			var agent = new AutomationCalculator.AutoDecision.AutoReRejection.Agent(customerID, decisionTime, DB, Log);
			agent.Init();
			agent.MakeDecision();

			agent.Trail.Save(DB, null, tag: this.tag);

			return agent.Trail.HasDecided;
		} // IsRerejected

		private bool IsRejected(int customerID, DateTime decisionTime) {
			var agent =
				new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.ManAgainstAMachine.SameDataAgent(
					customerID,
					decisionTime,
					DB,
					Log
				);

			return agent.Decide(null, this.tag);
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

		private bool IsReapproved(int customerID, DateTime decisionTime) {
			var agent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval.ManAgainstAMachine.SameDataAgent(
				customerID,
				decisionTime,
				DB,
				Log
			);

			agent.Init();

			agent.Decide(true, null, this.tag);

			return agent.Trail.HasDecided;
		} // IsReapproved

		private bool IsApproved(
			int customerID,
			int offeredCreditLine,
			AutomationCalculator.Common.MedalOutputModel medal,
			DateTime decisionTime,
			SortedSet<string> allNonAffirmativeTraces
		) {
			if (medal == null)
				return false;

			var agent = new AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent(
				customerID,
				offeredCreditLine,
				medal.Medal,
				medal.MedalType,
				medal.TurnoverType,
				decisionTime,
				DB,
				Log
			);

			agent.Init();

			agent.MakeDecision();

			agent.Trail.Save(DB, null, tag: this.tag);

			if (!agent.Trail.HasDecided && (allNonAffirmativeTraces != null)) {
				this.nonAffirmativeTraces = new SortedSet<string>();

				foreach (string traceName in agent.Trail.NonAffirmativeTraces()) {
					this.nonAffirmativeTraces.Add(traceName);
					allNonAffirmativeTraces.Add(traceName);
				} // for each trace
			} // if

			return agent.Trail.HasDecided;
		} // IsApproved

		private SortedSet<string> nonAffirmativeTraces;

		private readonly string tag;

		private static AConnection DB { get { return Library.Instance.DB; } }
		private static ASafeLog Log { get { return Library.Instance.Log; } }

		private static int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }
		private static int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }
	} // class CustomerDecisions
} // namespace
