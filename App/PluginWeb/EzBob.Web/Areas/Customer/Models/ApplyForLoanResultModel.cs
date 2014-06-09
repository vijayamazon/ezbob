namespace EzBob.Web.Areas.Customer.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using EKM;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;

	public class ApplyForLoanResultModel {
		#region public

		#region constructor

		public ApplyForLoanResultModel(EZBob.DatabaseLib.Model.Database.Customer customer, bool bDirectApplication) {
			ekms = new Dictionary<string, string>();

			if (customer == null) {
				error = "Please log out and log in again.";
				ms_oLog.Debug("Apply for a loan requested but no customer in the current context.");
				return;
			} // if

			error = string.Empty;

			if (bDirectApplication)
				has_yodlee = false;
			else {
				has_yodlee = CurrentValues.Instance.RefreshYodleeEnabled && customer.GetYodleeAccounts().Any();
				FindBadEkms(customer);
			} // if

			ms_oLog.Debug("Apply for a loan from customer {0}: {1}.", customer.Stringify(), this);
		} // constructor

		#endregion constructor

		#region serialisable
		// ReSharper disable InconsistentNaming

		public string error { get; private set; }

		public Dictionary<string, string> ekms { get; private set; }

		public bool has_yodlee { get; private set; }

		public bool has_ekm { get { return ekms.Count > 0; } }

		// ReSharper restore InconsistentNaming
		#endregion serialisable

		#region method IsReadyForApply

		public bool IsReadyForApply() {
			return string.IsNullOrWhiteSpace(error) && !has_ekm;
		} // IsReadyForApply

		#endregion method IsReadyForApply

		#region method ToString

		public override string ToString() {
			return string.Format(
				"ready = {0}, has EKM = {1}, has Yodlee = {2}, error msg = '{3}'",
				IsReadyForApply(),
				has_ekm,
				has_yodlee,
				error
			);
		} // ToString

		#endregion method ToString

		#endregion public

		#region private

		#region method FindBadEkms

		private void FindBadEkms(EZBob.DatabaseLib.Model.Database.Customer customer) {
			var ekmType = new EkmDatabaseMarketPlace();

			IEnumerable<MP_CustomerMarketPlace> oEkmAccounts =
				customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == ekmType.InternalId);

			EkmConnector validator = null;

			foreach (MP_CustomerMarketPlace ekm in oEkmAccounts) {
				validator = validator ?? new EkmConnector();

				string sPassword;

				try {
					sPassword = Encrypted.Decrypt(ekm.SecurityData);
				}
				catch (Exception e) {
					ms_oLog.Warn(e, "Failed to parse EKM password for marketplace with id {0}.", ekm.Id);
					ekms[ekm.DisplayName] = "Invalid password.";
					continue;
				} // try

				string sError;

				if (!validator.Validate(ekm.DisplayName, sPassword, out sError))
					ekms[ekm.DisplayName] = sError;
			} // for each ekm
		} // FindBadEkms

		#endregion method FindBadEkms

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(ApplyForLoanResultModel));

		#endregion private
	} // class ApplyForLoanResultModel
} // namespace