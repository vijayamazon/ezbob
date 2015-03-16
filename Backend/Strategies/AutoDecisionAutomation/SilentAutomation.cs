namespace Ezbob.Backend.Strategies.AutoDecisionAutomation {
	using System;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Database;

	/// <summary>
	/// Executes all the automation decisions (re-reject, reject, re-approve, approve) in silent mode,
	/// i.e. decision is written to database and that's it. This class is used to check auto decision
	/// against manual decision and is triggered when underwriter clicks "approve" button or "reject" button
	/// or transfers customer to "pending" state.
	/// </summary>
	public class SilentAutomation : AStrategy {
		public SilentAutomation(int customerID) {
			this.customerID = customerID;
		} // constructor

		public override string Name { get { return "Silent automation"; } }

		public override void Execute() {
			string tag = string.Format(
				"#SilentAutomation_{0}_{1}",
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
				Guid.NewGuid().ToString("N")
			);

			new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReRejection(
				this.customerID, DB, Log
			).MakeAndVerifyDecision(tag);

			new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.Agent(
				this.customerID, DB, Log
			).Init().MakeAndVerifyDecision(tag);

			var instance = new CalculateMedal(this.customerID, DateTime.UtcNow, false, true) {
				Tag = tag
			};
			instance.Execute();

			MedalResult medal = instance.Result;

			int offeredCreditLine = CapOffer(medal);

			new AutoDecisionAutomation.AutoDecisions.ReApproval.Agent(
				this.customerID, DB, Log
			).Init().MakeAndVerifyDecision(tag);

			new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval.Approval(
				this.customerID,
				offeredCreditLine,
				medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)medal.TurnoverType,
				DB,
				Log
			).Init().MakeAndVerifyDecision(tag);
		} // Execute

		private int CapOffer(MedalResult medal) {
			Log.Info("Finalizing and capping offer");

			int offeredCreditLine = medal.RoundOfferedAmount();

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerID)
			);

			if (isHomeOwnerAccordingToLandRegistry) {
				Log.Info("Capped for home owner according to land registry");
				offeredCreditLine = Math.Min(offeredCreditLine, MaxCapHomeOwner);
			} else {
				Log.Info("Capped for not home owner");
				offeredCreditLine = Math.Min(offeredCreditLine, MaxCapNotHomeOwner);
			} // if

			return offeredCreditLine;
		} // CapOffer

		private int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }
		private int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }

		private readonly int customerID;
	} // class SilentAutomation
} // namespace
