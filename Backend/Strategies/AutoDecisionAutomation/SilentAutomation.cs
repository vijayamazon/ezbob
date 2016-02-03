namespace Ezbob.Backend.Strategies.AutoDecisionAutomation {
	using System;
	using System.Globalization;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.Common;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MainStrategy;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	/// <summary>
	/// <para>Executes non-repeating automation decisions (reject, approve) in silent mode,
	/// i.e. decision is written to database and that's it. This class is used to check auto decision
	/// against manual decision and is triggered when underwriter clicks "approve" button or "reject" button
	/// or transfers customer to "pending" state. Also it is triggered when customer data was updated
	/// (add/update marketplace, land registry data updated, AML updated, Experian consumer data updated,
	/// Experian company data updated).</para>
	/// <para>After adding new marketplace (and only in this case) if approve decision is "approved"
	/// Main strategy is executed in "skip all apply auto rules" mode to auto approve customer if possible.</para>
	/// </summary>
	public class SilentAutomation : AMainStrategyBase {
		public enum Callers {
			Unknown,
			AddMarketplace,
			UpdateMarketplace,
			LandRegistry,
			Aml,
			Consumer,
			Company,
			MainSkipEverything,
			MainUpdateAndGoManual,
			ManuallyApproved,
			ManuallyRejected,
			ManuallySuspended,
		} // enum Callers

		public SilentAutomation(int customerID) {
			this.medalToUse = null;
			this.doMainStrategy = true;
			this.customerID = customerID;
			this.caller = Callers.Unknown;
			this.tag = CreateTag();
		} // constructor

		public SilentAutomation PreventMainStrategy() {
			this.doMainStrategy = false;
			return this;
		} // PreventMainStrategy

		public SilentAutomation SetMedal(MedalResult medal) {
			this.medalToUse = medal;
			return this;
		} // SetMedal

		public override string Name { get { return "SilentAutomation"; } }

		public override int CustomerID { get { return this.customerID; } }

		public virtual string Tag {
			get {
				if (string.IsNullOrWhiteSpace(this.tag))
					this.tag = CreateTag();

				return this.tag;
			} // get
		} // Tag

		public virtual SilentAutomation SetTag(Callers theCaller) {
			this.caller = theCaller;
			this.tag = CreateTag();
			return this;
		} // SetTag

		public override void Execute() {
			Log.Debug("Executing for customer '{0}'...", this.customerID);

			LoadMainStrategyExecutedBefore();

			if (!this.mainStrategyExecutedBefore) {
				Log.Debug("Not executing: main strategy has never run before for customer '{0}'.", this.customerID);
				return;
			} // if

			SafeReader sr = DB.GetFirst(
				"LoadLastCustomerCashRequest",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID)
			);

			if (sr.IsEmpty) {
				Log.Debug(
					"Not running silent automation for customer {0}: there is no available cash request.",
					this.customerID
				);

				return;
			} // if

			this.cashRequestID = sr["CashRequestID"];

			this.nlCashRequestID = sr["NLCashRequestID"];

			ForceNhibernateResync.ForCustomer(this.customerID);

			Log.Debug(
				"Executing silent reject for customer '{0}' using cash request 'o {1}/n {2}'...",
				this.customerID,
				this.cashRequestID,
				this.nlCashRequestID
			);

			LoadCompanyAndMonthlyPayment(DateTime.UtcNow);

			var rejectAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.LogicalGlue.Agent(
				new AutoRejectionArguments(
					this.customerID,
					CompanyID,
					this.cashRequestID,
					this.nlCashRequestID,
					Tag,
					DateTime.UtcNow,
					DB,
					Log
				)
			) { CompareTrailsQuietly = true, };

			rejectAgent.MakeAndVerifyDecision();

			MedalResult medal = CalculateMedal();

			int offeredCreditLine = CapOffer(medal);

			Log.Debug(
				"Executing silent approve for customer '{0}' using cash request '{1}', nlCashRequest '{2}'...",
				this.customerID,
				this.cashRequestID,
				this.nlCashRequestID
			);

			var approveAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval.LogicalGlue.Agent(
				new AutoApprovalArguments(
					this.customerID,
					this.cashRequestID,
					this.nlCashRequestID,
					offeredCreditLine,
					(AutomationCalculator.Common.Medal)medal.MedalClassification,
					(AutomationCalculator.Common.MedalType)medal.MedalType,
					(AutomationCalculator.Common.TurnoverType?)medal.TurnoverType,
					rejectAgent.Output.FlowType,
					rejectAgent.Output.ErrorInLGData,
					Tag,
					DateTime.UtcNow,
					DB,
					Log
				)
			) { CompareTrailsQuietly = true, }.Init();

			approveAgent.MakeAndVerifyDecision();

			if (this.caller == Callers.AddMarketplace) {
				bool isRejected = !rejectAgent.WasMismatch && rejectAgent.Trail.HasDecided;
				bool isApproved = !approveAgent.WasMismatch && (
					(
						(rejectAgent.Output.FlowType == AutoDecisionFlowTypes.LogicalGlue) &&
						approveAgent.Trail.HasDecided
					) || (
						(rejectAgent.Output.FlowType != AutoDecisionFlowTypes.LogicalGlue) &&
						(approveAgent.Trail.RoundedAmount > 0)
					)
				);

				if (!isRejected && isApproved)
					ExecuteMain();
				else {
					Log.Debug(
						"Not running auto decision for customer {0} using cash request 'o {3}/n {4}': " +
						"no potential ({1} and {2}).",
						this.customerID,
						isRejected ? "rejected" : "not rejected",
						isApproved ? "approved" : "not approved",
						this.cashRequestID,
						this.nlCashRequestID
					);
				} // if
			} // if

			Log.Debug("Complete for customer '{0}'.", this.customerID);
		} // Execute

		private void ExecuteMain() {
			bool skipMain =
				!this.doMainStrategy ||
				(this.caller == Callers.MainSkipEverything) ||
				(this.caller == Callers.MainUpdateAndGoManual);

			if (skipMain) {
				Log.Debug(
					"Silent decision for customer {0} is 'approve', but main strategy won't be executed " +
					"(prevent main: '{1}', caller: '{2}').",
					this.customerID,
					this.doMainStrategy ? "yes" : "no",
					this.caller
				);

				return;
			} // if

			Log.Debug(
				"Silent decision for customer {0} using cash request 'o {1}/n {2}' is 'approve', " +
				"checking whether cash request is intact...",
				this.customerID,
				this.cashRequestID,
				this.nlCashRequestID
			);

			SafeReader sr = DB.GetFirst(
				"LoadLastCustomerCashRequest",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID)
			);

			if (sr.IsEmpty) { // should never happen because cash request was loaded earlier.
				Log.Alert(
					"Not running auto decision for customer {0} using cash request {1}: there is no available cash request.",
					this.customerID,
					this.cashRequestID
				);

				return;
			} // if

			long currentCashRequestID = sr["CashRequestID"];
			string uwDecision = (sr["UnderwriterDecision"] ?? string.Empty).Trim();

			// TODO: when removing old cash request: it should be nlCashRequestID.
			bool suitableCashRequest = this.cashRequestID == currentCashRequestID;

			bool suitableDecision =
				(uwDecision == string.Empty) ||
				(uwDecision == CreditResultStatus.WaitingForDecision.ToString());

			if (!suitableCashRequest || !suitableDecision) {
				Log.Debug(
					"Not running auto decision for customer {0}: " +
					"new cash request was added (id change {1} -> {2} or " +
					"last cash request (id: {1}) has decision '{3}' while desired is '{4}'.",
					this.customerID,
					this.cashRequestID,
					currentCashRequestID,
					uwDecision,
					CreditResultStatus.WaitingForDecision
				);

				return;
			} // if

			Log.Debug(
				"Running auto decision for customer {0}: last cash request (id: {1}) has decision '{2}'...",
				this.customerID,
				this.cashRequestID,
				uwDecision
			);

			new MainStrategy(new MainStrategyArguments{
				UnderwriterID = 1, // this is an underwriter ID that is used for auto decisions
				CustomerID = this.customerID,
				NewCreditLine = NewCreditLineOption.SkipEverythingAndApplyAutoRules,
				AvoidAutoDecision = 0,
				FinishWizardArgs = null,
				CashRequestID = this.cashRequestID,
				CashRequestOriginator = null
			}).Execute();

			Log.Debug(
				"Running auto decision for customer {0}, last cash request (id: {1}) complete.",
				this.customerID,
				this.cashRequestID
			);
		} // ExecuteMain

		private void LoadMainStrategyExecutedBefore() {
			SafeReader sr = DB.GetFirst(
				"GetMainStrategyStallerData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerID)
			);

			this.mainStrategyExecutedBefore = sr.IsEmpty ? false : sr["MainStrategyExecutedBefore"];
		} // LoadMainStrategyExecutedBefore 

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

		private string CreateTag() {
			return string.Format(
				"#{0}_{1}_{2}",
				this.caller == Callers.Unknown ? Name : this.caller.ToString(),
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
				Guid.NewGuid().ToString("N")
			);
		} // CreateTag

		private MedalResult CalculateMedal() {
			if (this.medalToUse != null) {
				Log.Debug("Using preset medal for customer '{0}'...", this.customerID);
				return this.medalToUse;
			} // if

			Log.Debug("Executing silent medal for customer '{0}'...", this.customerID);

			var instance = new CalculateMedal(this.customerID, this.cashRequestID, this.nlCashRequestID, DateTime.UtcNow, false, true ) {
				Tag = Tag,
				QuietMode = true,
			};
			instance.Execute();

			return instance.Result;
		} // CalculateMedal

		private int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }
		private int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }

		private Callers caller;
		private readonly int customerID;
		private string tag;
		private long cashRequestID;
		private bool mainStrategyExecutedBefore;
		private bool doMainStrategy;
		private MedalResult medalToUse;
		private long nlCashRequestID;
	} // class SilentAutomation
} // namespace
