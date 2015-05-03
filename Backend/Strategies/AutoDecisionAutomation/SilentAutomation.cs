namespace Ezbob.Backend.Strategies.AutoDecisionAutomation {
	using System;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository.Turnover;
	using NHibernate;
	using StructureMap;

	/// <summary>
	/// Executes all the automation decisions (re-reject, reject, re-approve, approve) in silent mode,
	/// i.e. decision is written to database and that's it. This class is used to check auto decision
	/// against manual decision and is triggered when underwriter clicks "approve" button or "reject" button
	/// or transfers customer to "pending" state.
	/// </summary>
	public class SilentAutomation : AStrategy {
		public enum Callers {
			UpdateMarketplace,
			LandRegistry,
			Aml,
			Consumer,
			Company,
		} // enum Callers

		public SilentAutomation(int customerID) {
			this.customerID = customerID;
		} // constructor

		public override string Name { get { return "SilentAutomation"; } }

		public virtual string Tag {
			get {
				if (string.IsNullOrWhiteSpace(this.tag))
					this.tag = CreateTag();

				return this.tag;
			} // get
			set { this.tag = CreateTag(value); }
		} // Tag

		public virtual SilentAutomation SetTag(Callers caller) {
			Tag = caller.ToString();
			return this;
		} // SetTag

		public override void Execute() {
			ForceNhibernateResync();

			new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.Agent(
				this.customerID, DB, Log
			).Init().MakeAndVerifyDecision(Tag);

			var instance = new CalculateMedal(this.customerID, DateTime.UtcNow, false, true) {
				Tag = Tag
			};
			instance.Execute();

			MedalResult medal = instance.Result;

			int offeredCreditLine = CapOffer(medal);

			new Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval.Approval(
				this.customerID,
				offeredCreditLine,
				medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)medal.TurnoverType,
				DB,
				Log
			).Init().MakeAndVerifyDecision(Tag);
		} // Execute

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

		private string CreateTag(string label = null) {
			return string.Format(
				"#{0}_{1}_{2}",
				string.IsNullOrWhiteSpace(label) ? Name : label.Trim(),
				DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture),
				Guid.NewGuid().ToString("N")
			);
		} // CreateTag

		private int MaxCapHomeOwner { get { return CurrentValues.Instance.MaxCapHomeOwner; } }
		private int MaxCapNotHomeOwner { get { return CurrentValues.Instance.MaxCapNotHomeOwner; } }

		private readonly int customerID;
		private string tag;
	} // class SilentAutomation
} // namespace
