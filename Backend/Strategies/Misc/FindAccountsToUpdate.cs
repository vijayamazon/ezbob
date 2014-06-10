namespace EzBob.Backend.Strategies.Misc {
	using ConfigManager;
	using EKM;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;
	using YodleeLib.connector;

	public class FindAccountsToUpdate : AStrategy {
		#region public

		#region constructor

		public FindAccountsToUpdate(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new LoadUpdatableAccounts(nCustomerID, DB, Log);
			Result = new AccountsToUpdateModel();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Find accounts to update"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			bool bIncludeYodlee = CurrentValues.Instance.RefreshYodleeEnabled;

			var oEkmType = new EkmDatabaseMarketPlace().InternalId;
			var oYodleeType = new YodleeDatabaseMarketPlace().InternalId;
			//var oHmrcType = Integration.ChannelGrabberConfig.Configuration.GetInstance(Log).

		} // Execute

		#endregion method Execute

		#region property Result

		public AccountsToUpdateModel Result { get; private set; } // Result

		#endregion property Result

		#endregion public

		#region private

		private readonly LoadUpdatableAccounts m_oSp;

		#region class LoadUpdatableAccounts

		private class LoadUpdatableAccounts : AStoredProcedure {
			public LoadUpdatableAccounts(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerID = nCustomerID;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }
		} // class LoadUpdatableAccounts

		#endregion class LoadUpdatableAccounts

		#endregion private
	} // class FindAccountsToUpdate
} // namespace
