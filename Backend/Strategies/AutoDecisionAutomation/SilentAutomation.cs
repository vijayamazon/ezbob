namespace Ezbob.Backend.Strategies.AutoDecisionAutomation {
	using System;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MainStrategy;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository.Turnover;
	using NHibernate;
	using StructureMap;

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
		} // enum Callers

		public SilentAutomation(int customerID) {
			this.customerID = customerID;
			this.caller = Callers.Unknown;
			this.tag = CreateTag();
		} // constructor

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

			ForceNhibernateResync();

			Log.Debug("Executing silent reject for customer '{0}'...", this.customerID);

			var rejectAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.Agent(
				this.customerID,
				DB,
				Log
			).Init();

			rejectAgent.MakeAndVerifyDecision(Tag, true);

			Log.Debug("Executing silent medal for customer '{0}'...", this.customerID);

			var instance = new CalculateMedal(this.customerID, DateTime.UtcNow, false, true) {
				Tag = Tag,
				QuietMode = true,
			};
			instance.Execute();

			MedalResult medal = instance.Result;

			int offeredCreditLine = CapOffer(medal);

			Log.Debug("Executing silent approve for customer '{0}'...", this.customerID);

			var approveAgent = new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval.Approval(
				this.customerID,
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
						"Not running auto decision for customer {0}: no potential ({1} and {2}).",
						this.customerID,
						isRejected ? "rejected" : "not rejected",
						isApproved ? "approved" : "not approved"
					);
				} // if
			} // if

			Log.Debug("Complete for customer '{0}'.", this.customerID);
		} // Execute

		private void ExecuteMain() {
			Log.Debug(
				"Silent decision for customer {0} is 'approve', checking whether there is available cash request...",
				this.customerID
			);

			SafeReader sr = DB.GetFirst(
				"LoadLastCustomerCashRequest",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID)
			);

			if (sr.IsEmpty) {
				Log.Debug(
					"Not running auto decision for customer {0}: there is no available cash request.",
					this.customerID
				);

				return;
			} // if

			long cashRequestID = sr["CashRequestID"];
			string uwDecision = (sr["UnderwriterDecision"] ?? string.Empty).Trim();

			if ((uwDecision != string.Empty) && (uwDecision != CreditResultStatus.WaitingForDecision.ToString())) {
				Log.Debug(
					"Not running auto decision for customer {0}: " +
					"last cash request (id: {3}) has decision '{1}' while desired is '{2}'.",
					this.customerID,
					uwDecision,
					CreditResultStatus.WaitingForDecision,
					cashRequestID
				);

				return;
			} // if

			Log.Debug(
				"Running auto decision for customer {0}: last cash request (id: {1}) has decision '{2}'...",
				this.customerID,
				cashRequestID,
				uwDecision
			);

			new MainStrategy(
				1, // this is an underwriter ID that is used for auto decisions
				this.customerID,
				NewCreditLineOption.SkipEverythingAndApplyAutoRules,
				0,
				null,
				cashRequestID,
				null
			).Execute();

			Log.Debug(
				"Running auto decision for customer {0}, last cash request (id: {1}) complete.",
				this.customerID,
				cashRequestID
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

		private void ForceNhibernateResync() {
			ISession session = ObjectFactory.GetInstance<ISession>();

			Customer customer = ObjectFactory.GetInstance<CustomerRepository>().ReallyTryGet(this.customerID);

			if (customer != null)
				session.Evict(customer);

			MarketplaceTurnoverRepository mpTurnoverRep = ObjectFactory.GetInstance<MarketplaceTurnoverRepository>();

			foreach (MarketplaceTurnover mpt in mpTurnoverRep.GetByCustomerId(this.customerID))
				if (mpt != null)
					session.Evict(mpt);
		} // ForceNhibernateResync

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

		private int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }
		private int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }

		private Callers caller;
		private readonly int customerID;
		private string tag;
		private bool mainStrategyExecutedBefore;
	} // class SilentAutomation
} // namespace
