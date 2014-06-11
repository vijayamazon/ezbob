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
			GetCustomerID();
			GetDisplayName();
			GetPassword();
			ValidateHash();

			var oReader = new LoadCustomerMarketplaceSecurityData(
				m_nCustomerID,
				m_sDisplayName,
				Integration.ChannelGrabberConfig.Configuration.GetInstance(Log).Hmrc.Guid(),
				DB,
				Log
			);

			oReader.Execute();

			if (oReader.Result.Count != 1) {
				Log.Warn("Too many/few HMRC accounts with display name of '{0}' for customer {1}.", m_sDisplayName, m_nCustomerID);
				return;
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

				return;
			} // try

			oSecInfo.password = m_sPassword;

			var oSaver = new UpdateMarketplaceSecurityData(DB, Log) {
				CustomerMarketplaceID = hmrc.CustomerMarketplaceID,
				SecurityData = new Encrypted(new Serialized(oSecInfo)),
			};

			oSaver.ExecuteNonQuery();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private int m_nCustomerID;
		private string m_sDisplayName;
		private string m_sPassword;

		private readonly string m_sRawCustomerID;
		private readonly string m_sRawDisplayName;
		private readonly string m_sRawPassword;
		private readonly string m_sHash;

		#region method GetCustomerID

		private void GetCustomerID() {
			try {
				m_nCustomerID = int.Parse(Encrypted.Decrypt(m_sRawCustomerID));
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
				m_sPassword = Encrypted.Decrypt(m_sRawPassword);
			}
			catch (Exception e) {
				throw new StrategyWarning(this, "Failed to get password from " + m_sRawPassword, e);
			}
		} // GetPassword

		#endregion method GetPassword

		#region method ValidateHash

		private void ValidateHash() {
			string sHash = SecurityUtils.Hash(m_nCustomerID + m_sPassword + m_sDisplayName);

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
