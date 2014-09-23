namespace EzBob.Web.Areas.Customer.Models {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class ApplyForLoanResultModel {
		#region public

		#region constructor

		public ApplyForLoanResultModel(EZBob.DatabaseLib.Model.Database.Customer customer, bool bDirectApplication) {
			if (customer == null) {
				error = "Please log out and log in again.";
				ms_oLog.Debug("Apply for a loan requested but no customer in the current context.");
				return;
			} // if

			error = string.Empty;

			if (!bDirectApplication) {
				try {
					AccountsToUpdateActionResult atuar = new ServiceClient().Instance.FindAccountsToUpdate(customer.Id);
					m_oAccountInfo = atuar.AccountInfo;
				}
				catch (Exception e) {
					ms_oLog.Warn(e, "Something went erroneously while looking for accounts to update.");
				} // try
			} // if

			ms_oLog.Debug("Apply for a loan from customer {0}: {1}.", customer.Stringify(), this);
		} // constructor

		#endregion constructor

		#region serialisable
		// ReSharper disable InconsistentNaming

		public string error { get; private set; }

		public SortedDictionary<string, string> ekms {
			get {
				return m_oAccountInfo == null
					? new SortedDictionary<string, string>()
					: m_oAccountInfo.Ekms;
			}
		} // ekms

		public bool has_yodlee {
			get { return m_oAccountInfo != null && m_oAccountInfo.HasYodlee; } // get
		} // has_yodlee

		public bool has_ekm { get { return ekms.Count > 0; } }

		public bool vat_return_is_up_to_date {
			get { return m_oAccountInfo == null || m_oAccountInfo.IsVatReturnUpToDate; } // get
		} // vat_return_is_up_to_date

		public SortedSet<string> linked_hmrc {
			get {
				return m_oAccountInfo == null
					? new SortedSet<string>()
					: m_oAccountInfo.LinkedHmrc;
			} // get
		} // linked_hmrc

		public bool has_hmrc { get { return linked_hmrc.Count > 0; } }

		public bool HasUploadedHmrc { get { return m_oAccountInfo != null && m_oAccountInfo.HasUploadedHmrc; } }

		public bool good_to_go {
			get { return IsReadyForApply(); }
		} // good_to_go

		// ReSharper restore InconsistentNaming
		#endregion serialisable

		#region method IsReadyForApply

		public bool IsReadyForApply() {
			return string.IsNullOrWhiteSpace(error) && !has_ekm && !has_hmrc && vat_return_is_up_to_date;
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

		private readonly AccountsToUpdate m_oAccountInfo;
		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(ApplyForLoanResultModel));

		#endregion private
	} // class ApplyForLoanResultModel
} // namespace