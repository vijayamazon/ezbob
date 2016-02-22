namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using EKM;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using Ezbob.ValueIntervals;
	using global::Integration.ChannelGrabberConfig;
	using global::Integration.ChannelGrabberFrontend;
	using JetBrains.Annotations;
	using MailStrategies;
	using YodleeLib.connector;

	public class FindAccountsToUpdate : AStrategy {

		public FindAccountsToUpdate(int nCustomerID) {
			m_oStra = new LoadCustomerMarketplaceSecurityData(nCustomerID);
			m_oCustomerData = new CustomerData(this, nCustomerID, DB);
			Result = new AccountsToUpdate();
		} // constructor

		public override string Name {
			get { return "Find accounts to update"; }
		} // Name

		public override void Execute() {
			m_oCustomerData.Load();

			Guid oEkmType = new EkmDatabaseMarketPlace().InternalId;
			Guid oYodleeType = new YodleeDatabaseMarketPlace().InternalId;
			VendorInfo oHmrc = global::Integration.ChannelGrabberConfig.Configuration.GetInstance(Log).Hmrc;

			m_oStra.Execute();

			foreach (LoadCustomerMarketplaceSecurityData.ResultRow oRes in m_oStra.Result) {
				if (oRes.InternalID == oEkmType)
					CheckEkm(oRes);
				else if (oRes.InternalID == oYodleeType && oRes.DisplayName != "ParsedBank")
					Result.HasYodlee = CurrentValues.Instance.RefreshYodleeEnabled;
				else if (oRes.InternalID == oHmrc.Guid())
					CheckHmrc(oRes);
			} // for each result

			Result.IsVatReturnUpToDate = new CheckAllVatReturnPeriods(m_oCustomerData.Id, DB, Log).IsUpToDate();
		} // Execute

		public AccountsToUpdate Result { get; private set; } // Result

		private readonly LoadCustomerMarketplaceSecurityData m_oStra;
		private readonly CustomerData m_oCustomerData;

		private EkmConnector m_oEkmConnector;

		private void CheckEkm(LoadCustomerMarketplaceSecurityData.ResultRow ekm) {
			string sPassword;

			try {
				sPassword = Encrypted.Decrypt(ekm.SecurityData);
			}
			catch (Exception e) {
				Log.Warn(e, "Failed to parse EKM password for marketplace with id {0}.", ekm.CustomerMarketplaceID);
				Result.Ekms[ekm.DisplayName] = "Invalid password.";
				return;
			} // try

			m_oEkmConnector = m_oEkmConnector ?? new EkmConnector();
			string sError;

			if (!m_oEkmConnector.Validate(ekm.DisplayName, sPassword, out sError))
				Result.Ekms[ekm.DisplayName] = sError;
		} // CheckEkm

		private void CheckHmrc(LoadCustomerMarketplaceSecurityData.ResultRow hmrc) {
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

			if ((oSecInfo.login == m_oCustomerData.Mail) && (oSecInfo.password == VendorInfo.TopSecret))
			{
				if (hmrc.UpdatingStart.HasValue && hmrc.UpdatingStart.Value.AddDays(1) < DateTime.UtcNow)
				{
					Result.HasUploadedHmrc = true;
				}
				return;
			}

			try {
				var ctr = new Connector(oSecInfo.Fill(), Log, m_oCustomerData.Id, m_oCustomerData.Mail);

				if (ctr.Init()) {
					ctr.Run(true);
					ctr.Done();
				} // if
			}
			catch (InvalidCredentialsException) {
				Log.Debug("Invalid credentials detected for linked HMRC account {0} ({1}).", hmrc.CustomerMarketplaceID, oSecInfo.login);
				Result.LinkedHmrc.Add(oSecInfo.login);
			}
			catch (Exception e) {
				Log.Warn(e, "Failed to validate credentials for linked HMRC account {0} ({1}).", hmrc.CustomerMarketplaceID, oSecInfo.login);
			} // try
		} // CheckHmrc

		private class CheckAllVatReturnPeriods : AStoredProcedure {

			public CheckAllVatReturnPeriods(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerID = nCustomerID;
				m_bResult = null;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			public bool IsUpToDate() {
				if (m_bResult.HasValue)
					return m_bResult.Value;

				var oData = new SortedDictionary<long, List<ResultRow>>();

				ForEachResult<ResultRow>(oRow => {
					oRow.IsDeleted = false;

					if (!oData.ContainsKey(oRow.RegistrationNo)) {
						oData[oRow.RegistrationNo] = new List<ResultRow> { oRow };
						return ActionResult.Continue;
					} // if

					List<ResultRow> lst = oData[oRow.RegistrationNo];

					foreach (ResultRow cur in lst) {
						if (cur.IsDeleted)
							continue;

						if (!cur.Interval.Intersects(oRow.Interval))
							continue;

						if (cur.SourceID >= oRow.SourceID)
							cur.IsDeleted = true;
						else {
							oRow.IsDeleted = true;
							break;
						} // if
					} // for each

					if (!oRow.IsDeleted)
						lst.Add(oRow);

					return ActionResult.Continue;
				});

				var oNow = DateTime.UtcNow;

				var oMonthMiddle = new DateTime(oNow.Year, oNow.Month, 1, 0, 0, 0, DateTimeKind.Utc)
					.AddMonths(-1).AddDays(14);

				for (int i = 0; i < 15; i++) {
					if (!IsDateContained(oMonthMiddle, oData)) {
						m_bResult = false;
						break;
					} // if

					oMonthMiddle = oMonthMiddle.AddMonths(-1);
				} // for

				if (!m_bResult.HasValue)
					m_bResult = true;

				return m_bResult.Value;
			} // IsUpToDate

			protected override string GetName() {
				return "LoadAllVatReturnPeriods";
			} // GetName

			private bool? m_bResult;

			private static bool IsDateContained(DateTime oMonthMiddle, SortedDictionary<long, List<ResultRow>> oData) {
				var oEdge = new DateIntervalEdge(oMonthMiddle, AIntervalEdge<DateTime>.EdgeType.Finite);

				return oData.Any(pair =>
					pair.Value.Where(cur => !cur.IsDeleted).Any(cur => cur.Interval.Contains(oEdge))
				);
			} // IsDateContained

			private class ResultRow : AResultRow {
				[UsedImplicitly]
				public int Id { get; set; }

				[UsedImplicitly]
				public DateTime DateFrom { get; set; }

				[UsedImplicitly]
				public DateTime DateTo { get; set; }

				[UsedImplicitly]
				public long RegistrationNo { get; set; }

				[UsedImplicitly]
				public int SourceID { get; set; }

				[NonTraversable]
				public bool IsDeleted { get; set; }

				public DateInterval Interval {
					get {
						m_oInterval = m_oInterval ?? new DateInterval(DateFrom, DateTo);

						return m_oInterval;
					} // get
				} // Interval

				private DateInterval m_oInterval;
			} // class ResultRow

		} // CheckAllVatReturnPeriods

	} // class FindAccountsToUpdate
} // namespace
