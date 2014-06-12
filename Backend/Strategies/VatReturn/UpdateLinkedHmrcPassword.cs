namespace EzBob.Backend.Strategies.VatReturn {
	using System;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using Integration.ChannelGrabberFrontend;
	using JetBrains.Annotations;
	using Misc;

	public class UpdateLinkedHmrcPassword : AStrategy {
		#region public

		#region constructor

		public UpdateLinkedHmrcPassword(
			string sCustomerID,
			string sDisplayName,
			string sPassword,
			string sHash,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_sRawCustomerID = sCustomerID;
			m_sRawDisplayName = sDisplayName;
			m_sRawPassword = sPassword;
			m_sHash = sHash;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "UpdateLinkedHmrcPassword"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			UpdatePassword(ValidateInput());
		} // Execute

		#endregion method Execute

		#endregion public

		#region protected

		#region structure AccountData

		protected class AccountData {
			public int CustomerMarketplaceID;
			public AccountModel SecInfo;
		} // AccountData

		#endregion structure AccountData

		#region method ValidateInput

		protected AccountData ValidateInput() {
			GetCustomerID();
			GetDisplayName();
			GetPassword();
			ValidateHash();

			var oReader = new LoadCustomerMarketplaceSecurityData(
				CustomerID,
				m_sDisplayName,
				Integration.ChannelGrabberConfig.Configuration.GetInstance(Log).Hmrc.Guid(),
				DB,
				Log
			);

			oReader.Execute();

			if (oReader.Result.Count != 1) {
				Log.Warn("Too many/few HMRC accounts with display name of '{0}' for customer {1}.", m_sDisplayName, CustomerID);
				return null;
			} // if

			LoadCustomerMarketplaceSecurityData.ResultRow hmrc = oReader.Result[0];

			AccountModel oSecInfo;
			
			try {
				oSecInfo = Serialized.Deserialize<AccountModel>(Encrypted.Decrypt(hmrc.SecurityData));
			}
			catch (Exception e) {
				Log.Alert(
					e,
					"Failed to de-serialise security data for HMRC marketplace {0} ({1}).",
					hmrc.DisplayName,
					hmrc.CustomerMarketplaceID
				);

				return null;
			} // try

			return new AccountData {
				CustomerMarketplaceID = hmrc.CustomerMarketplaceID,
				SecInfo = oSecInfo,
			};
		} // ValidateInput

		#endregion method ValidateInput

		#region method UpdatePassword

		protected void UpdatePassword(AccountData data) {
			if (data == null)
				return;

			data.SecInfo.password = Password;

			var oSaver = new UpdateMarketplaceSecurityData(DB, Log) {
				CustomerMarketplaceID = data.CustomerMarketplaceID,
				SecurityData = new Encrypted(new Serialized(data.SecInfo)),
			};

			oSaver.ExecuteNonQuery();
		} // UpdatePassword

		#endregion method UpdatePassword

		protected int CustomerID { get; set; }
		protected string Password { get; set; }

		#endregion protected

		#region private

		private string m_sDisplayName;

		private readonly string m_sRawCustomerID;
		private readonly string m_sRawDisplayName;
		private readonly string m_sRawPassword;
		private readonly string m_sHash;

		#region method GetCustomerID

		private void GetCustomerID() {
			try {
				CustomerID = int.Parse(Encrypted.Decrypt(m_sRawCustomerID));
			}
			catch (Exception e) {
				throw new StrategyWarning(this, "Failed to get customer ID from " + m_sRawCustomerID, e);
			}
		} // GetCustomerID

		#endregion method GetCustomerID

		#region method GetDisplayName

		private void GetDisplayName() {
			try {
				m_sDisplayName = Encrypted.Decrypt(m_sRawDisplayName);
			}
			catch (Exception e) {
				throw new StrategyWarning(this, "Failed to get display name from " + m_sRawDisplayName, e);
			}
		} // GetDisplayName

		#endregion method GetDisplayName

		#region method GetPassword

		private void GetPassword() {
			try {
				Password = Encrypted.Decrypt(m_sRawPassword);
			}
			catch (Exception e) {
				throw new StrategyWarning(this, "Failed to get password from " + m_sRawPassword, e);
			}
		} // GetPassword

		#endregion method GetPassword

		#region method ValidateHash

		private void ValidateHash() {
			string sHash = SecurityUtils.Hash(CustomerID + Password + m_sDisplayName);

			if (sHash != m_sHash)
				throw new StrategyAlert(this, "Failed to validate hash: " + sHash + " != " + m_sHash);
		} // ValidateHash

		#endregion method ValidateHash

		#region class UpdateMarketplaceSecurityData

		private class UpdateMarketplaceSecurityData : AStoredProcedure {
			public UpdateMarketplaceSecurityData(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return CustomerMarketplaceID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerMarketplaceID { get; set; }

			[UsedImplicitly]
			public byte[] SecurityData { get; set; }
		} // class UpdateMarketplaceSecurityData

		#endregion class UpdateMarketplaceSecurityData

		#endregion private
	} // class UpdateLinkedHmrcPassword
} // namespace
