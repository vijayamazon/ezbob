namespace Ezbob.Backend.Strategies.AutoDecisionAutomation {
	using System;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MainStrategy;
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
	public class SilentAutomation : AStrategy {
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
				"Executing silent reject for customer '{0}' using cash request '{1}'...",
				this.customerID,
				this.cashRequestID
			);

			var rejectAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.Agent(
				this.customerID,
				this.cashRequestID,
				DB,
				Log
			).Init();

			rejectAgent.MakeAndVerifyDecision(Tag, true);

			MedalResult medal = CalculateMedal();

			int offeredCreditLine = CapOffer(medal);

			Log.Debug(
				"Executing silent approve for customer '{0}' using cash request '{1}', nlCashRequest '{2}'...",
				this.customerID,
				this.cashRequestID,
				this.nlCashRequestID
			);

			var approveAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval.Approval(
				this.customerID,
				this.cashRequestID,
				this.nl
				offeredCreditLine,
				medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)medal.TurnoverType,
				DB,
				Log
			).Init();

			approveAgent.MakeAndVerifyDecision(Tag, true);

			if (this.caller == Callers.AddMarketplace) {
				bool isRejected = !rejectAgent.WasMismatch && rejectAgent.Trail.HasDecided;
				bool isApproved = !approveAgent.WasMismatch && (approveAgent.ApprovedAmount > 0);

				if (!isRejected && isApproved)
					ExecuteMain();
				else {
					Log.Debug(
						"Not running auto decision for customer {0} using cash request {3}: no potential ({1} and {2}).",
						this.customerID,
						isRejected ? "rejected" : "not rejected",
						isApproved ? "approved" : "not approved",
						this.cashRequestID
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
				"Silent decision for customer {0} using cash request {1} is 'approve', " +
				"checking whether cash request is intact...",
				this.customerID,
				this.cashRequestID
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

			new MainStrategy(
				1, // this is an underwriter ID that is used for auto decisions
				this.customerID,
				NewCreditLineOption.SkipEverythingAndApplyAutoRules,
				0,
				null,
				this.cashRequestID,
				null
			).Execute();

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
