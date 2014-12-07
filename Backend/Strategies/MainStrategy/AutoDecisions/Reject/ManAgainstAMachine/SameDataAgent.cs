namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.ManAgainstAMachine {
	using System;
	using EzBob.Backend.Strategies.Experian;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;

	/// <summary>
	/// Verifies whether the customer should be rejected using customer data that was available on specific date.
	/// </summary>
	public class SameDataAgent : EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Agent {
		#region public

		#region constructor

		public SameDataAgent(int nCustomerID, DateTime oNow, AConnection oDB, ASafeLog oLog) : base(nCustomerID, oDB, oLog) {
			m_oNow = oNow;
		} // constructor

		#endregion constructor

		#region method Init

		public override Reject.Agent Init() {
			base.Init();

			Now = m_oNow;

			return this;
		} // Init

		#endregion method Init

		#region method Decide

		public virtual bool Decide() {
			RunPrimary();

			Trail.Save(DB, null);

			return Trail.HasDecided;
		} // Decide

		#endregion method Decide

		#endregion public

		#region protected

		#region method InitCfg

		protected override Configuration InitCfg() {
			return new Configuration(DB, Log);
		} // InitCfg

		#endregion method InitCfg

		#region method LoadData

		protected override void LoadData() {
			DB.ForEachRowSafe(
				ProcessRow,
				"LoadAutoRejectData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Args.CustomerID),
				new QueryParameter("Now", Now)
			);

			MetaData.Validate();
		} // LoadData

		#endregion method LoadMetaData

		#region method LoadConsumerData

		protected override ExperianConsumerData LoadConsumerData() {
			var lcd = new LoadExperianConsumerData(Args.CustomerID, null, MetaData.ConsumerServiceLogID, DB, Log);
			lcd.Execute();

			return lcd.Result;
		} // LoadConsumerData

		#endregion method LoadConsumerData

		#region method LoadCompanyData

		protected override ExperianLtd LoadCompanyData() {
			var ltd = new LoadExperianLtd(null, MetaData.CompanyServiceLogID, DB, Log);
			ltd.Execute();

			return ltd.Result;
		} // LoadCompanyData

		#endregion method LoadCompanyData

		#endregion protected

		private readonly DateTime m_oNow;
	} // class SameDataAgent
} // namespace
