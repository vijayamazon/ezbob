namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.ManAgainstAMachine {
	using System;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	/// Verifies whether the customer should be rejected using customer data that was available on specific date.
	/// </summary>
	public class SameDataAgent : Agent {
		public SameDataAgent(
			int nCustomerID,
			long? cashRequestID,
			long? nlCashRequestID,
			DateTime oNow,
			string tag,
			AConnection oDB,
			ASafeLog oLog
		) : base(nCustomerID, cashRequestID, nlCashRequestID, tag, oDB, oLog) {
			this.m_oNow = oNow;
		} // constructor

		public override Agent Init() {
			base.Init();

			Now = this.m_oNow;

			return this;
		} // Init

		public virtual bool Decide(string tag) {
			Init();

			RunPrimary();

			Trail.SetTag(tag).Save(DB, null);

			return Trail.HasDecided;
		} // Decide

		protected override Configuration InitCfg() {
			return new Configuration(DB, Log);
		} // InitCfg

		protected override void LoadData() {
			DB.ForEachRowSafe(
				ProcessRow,
				"LoadAutoRejectData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Args.CustomerID),
				new QueryParameter("Now", Now)
			);

			MetaData.Validate();

			CalculateTurnoverForReject(Args.CustomerID, Now);
		} // LoadData

		protected override ExperianConsumerData LoadConsumerData() {
			var lcd = new LoadExperianConsumerData(Args.CustomerID, null, MetaData.ConsumerServiceLogID);
			lcd.Execute();

			return lcd.Result;
		} // LoadConsumerData

		protected override ExperianLtd LoadCompanyData() {
			var ltd = new LoadExperianLtd(null, MetaData.CompanyServiceLogID);
			ltd.Execute();

			return ltd.Result;
		} // LoadCompanyData

		private readonly DateTime m_oNow;
	} // class SameDataAgent
} // namespace
