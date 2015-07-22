namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.LoadAndCategorize {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Database;
	using Ezbob.Logger;

	using RerejectAgent = AutomationCalculator.AutoDecision.AutoReRejection.Agent;

	using RejectAgent =
		Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.ManAgainstAMachine.SameDataAgent;

	using ApproveAgent = AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent;

	using ReapproveAgent =
		Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval.ManAgainstAMachine.SameDataAgent;

	public class CustomerDecisions {
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
			AutoDecisions.Add(RunAutomationOnce(Manual.First.DecisionTime, 1, Manual.First.CashRequestID));

			if (ManualDecisions.Count > 1)
				AutoDecisions.Add(RunAutomationOnce(Manual.Last.DecisionTime, 2, Manual.Last.CashRequestID));

			CurrentAutoDecision = RunAutomationOnce(now, 0, null, allNonAffirmativeTraces);
		} // RunAutomation

		public string NonAffirmativeTraceResult(string traceName) {
			if (this.nonAffirmativeTraces == null)
				return null;

			if (this.nonAffirmativeTraces.Contains(traceName))
				return "fail";

			return "ok";
		} // NonAffirmativeTraceResult

		protected virtual ApproveAgent CreateAutoApproveAgent(
			int offeredCreditLine,
			AutomationCalculator.Common.MedalOutputModel medal,
			DateTime decisionTime,
			long? cashRequestID
		) {
			return new ApproveAgent(
				CustomerID,
				cashRequestID,
				offeredCreditLine,
				medal.Medal,
				medal.MedalType,
				medal.TurnoverType,
				decisionTime,
				DB,
				Log
			);
		} // CreateAutoApproveAgent

		private AutoDecision RunAutomationOnce(
			DateTime decisionTime,
			int runTimeCount,
			long? cashRequestID,
			SortedSet<string> allNonAffirmativeTraces = null
		) {
			AutoDecision result = null;

			bool doNext = true;

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			// Just for code consistency.
			if (doNext) { // Re-reject area
				if (IsRerejected(decisionTime, cashRequestID)) {
					doNext = false;
					result = new AutoDecision(DecisionActions.ReReject);
				} // if
			} // if; Re-reject area

			if (doNext) {
				if (!IsAlibaba) {
					if (IsRejected(decisionTime, cashRequestID)) {
						doNext = false;
						result = new AutoDecision(DecisionActions.Reject);
					} // if
				} // if not alibaba
			} // if; Reject area

			AutomationCalculator.Common.MedalOutputModel medal = null;

			if (doNext) { // Medal area
				var verification = new AutomationCalculator.MedalCalculation.MedalChooser(DB, Log);
				medal = verification.GetMedal(CustomerID, decisionTime);
				medal.SaveToDb(this.tag, DB, Log);
			} // if; Medal area

			int offeredCreditLine = medal == null
				? 0
				: Ezbob.Backend.Strategies.MedalCalculations.MedalResult.RoundOfferedAmount(medal.OfferedLoanAmount);

			if (doNext)
				offeredCreditLine = CapOffer(offeredCreditLine);

			if (doNext) { // Re-approve area
				if (IsReapproved(decisionTime, cashRequestID)) {
					doNext = false;
					result = new AutoDecision(DecisionActions.ReApprove);
				} // if
			} // if; Re-approve area

			Guid? trailID = null;

			if (doNext) { // Approve area
				if (IsApproved(offeredCreditLine, medal, decisionTime, cashRequestID, allNonAffirmativeTraces, out trailID)) {
					doNext = false;
					result = new AutoDecision(DecisionActions.Approve, trailID, runTimeCount);

					Log.Debug(
						"{2} time auto approved: trail id is {0} ({1}).",
						trailID.HasValue ? "not null" : "null",
						trailID.HasValue ? trailID.Value.ToString() : "---",
						runTimeCount
					);
				} // if

				Log.Debug(
					"{2} time auto approval: trail id is {0} ({1}).",
					trailID.HasValue ? "not null" : "null",
					trailID.HasValue ? trailID.Value.ToString() : "---",
					runTimeCount
				);
			} // if; Approve area

			if (doNext)
				result = new AutoDecision(null, trailID, runTimeCount);

			Log.Debug(
				"{2} time automation: trail id is {0} ({1}).",
				trailID.HasValue ? "not null" : "null",
				trailID.HasValue ? trailID.Value.ToString() : "---",
				runTimeCount
			);

			return result;
		} // RunAutomationOnce

		private bool IsRerejected(DateTime decisionTime, long? cashRequestID) {
			var agent = new RerejectAgent(CustomerID, cashRequestID, decisionTime, DB, Log);
			agent.Init();
			agent.MakeDecision();

			agent.Trail.SetTag(this.tag).Save(DB, null);

			return agent.Trail.HasDecided;
		} // IsRerejected

		private bool IsRejected(DateTime decisionTime, long? cashRequestID) {
			var agent = new RejectAgent(CustomerID, cashRequestID, decisionTime, DB, Log);

			return agent.Decide(this.tag);
		} // IsRejected

		private int CapOffer(int offeredCreditLine) {
			Log.Debug("Capping offer for customer {0} with uncapped offer {1}.", CustomerID, offeredCreditLine);

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerID)
			);

			if (isHomeOwnerAccordingToLandRegistry) {
				Log.Info("Capped for home owner according to land registry");
				offeredCreditLine = Math.Min(offeredCreditLine, MaxCapHomeOwner);
			} else {
				Log.Info("Capped for not home owner");
				offeredCreditLine = Math.Min(offeredCreditLine, MaxCapNotHomeOwner);
			} // if

			Log.Debug("Capped offer for customer {0} is {1}.", CustomerID, offeredCreditLine);

			return offeredCreditLine;
		} // CapOffer

		private bool IsReapproved(DateTime decisionTime, long? cashRequestID) {
			var agent = new ReapproveAgent(CustomerID, cashRequestID, decisionTime, DB, Log);

			agent.Init();

			agent.Decide(this.tag);

			return agent.Trail.HasDecided;
		} // IsReapproved

		private bool IsApproved(
			int offeredCreditLine,
			AutomationCalculator.Common.MedalOutputModel medal,
			DateTime decisionTime,
			long? cashRequestID,
			SortedSet<string> allNonAffirmativeTraces,
			out Guid? trailUniqueID
		) {
			trailUniqueID = null;

			if (medal == null)
				return false;

			var agent = CreateAutoApproveAgent(offeredCreditLine, medal, decisionTime, cashRequestID);

			agent.Init();

			agent.MakeDecision();

			agent.Trail.SetTag(this.tag).Save(DB, null);

			if (!agent.Trail.HasDecided && (allNonAffirmativeTraces != null)) {
				this.nonAffirmativeTraces = new SortedSet<string>();

				foreach (string traceName in agent.Trail.NonAffirmativeTraces()) {
					this.nonAffirmativeTraces.Add(traceName);
					allNonAffirmativeTraces.Add(traceName);
				} // for each trace
			} // if

			trailUniqueID = agent.Trail.UniqueID;

			return agent.Trail.HasDecided;
		} // IsApproved

		private SortedSet<string> nonAffirmativeTraces;

		private readonly string tag;

		protected static AConnection DB { get { return Library.Instance.DB; } }
		protected static ASafeLog Log { get { return Library.Instance.Log; } }

		private static int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }
		private static int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }
	} // class CustomerDecisions
} // namespace
