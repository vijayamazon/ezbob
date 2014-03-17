namespace EzBob.Web.Infrastructure {
	using System.Web;

	public class WizardBrokerLeadModel {
		#region public

		#region constructor

		public WizardBrokerLeadModel(HttpSessionStateBase oSession) {
			Session = oSession;

			LeadID = (int)(Session[Constant.BrokerLeadID] ?? 0);
			LeadEmail = (Session[Constant.BrokerLeadEmail] ?? string.Empty).ToString();
			BrokerFillsForCustomer = (Session[Constant.BrokerFillsForCustomer] ?? Constant.No).ToString() == Constant.Yes;
			FirstName = (Session[Constant.BrokerLeadFirstName] ?? string.Empty).ToString();
			LastName = (Session[Constant.BrokerLeadLastName] ?? string.Empty).ToString();
		} // constructor

		public WizardBrokerLeadModel(
			HttpSessionStateBase oSession,
			int nLeadID,
			string sLeadEmail,
			string sFirstName,
			string sLastName,
			bool bBrokerFillsForCustomer
		) : this(oSession) {
			Clear();

			if (IsLeadSet(nLeadID, sLeadEmail)) {
				LeadID = nLeadID;
				LeadEmail = sLeadEmail;
				BrokerFillsForCustomer = bBrokerFillsForCustomer;
				FirstName = sFirstName;
				LastName = sLastName;
			} // if

			Upload();
		} // constructor

		#endregion constructor

		#region method Unset

		public void Unset() {
			Clear();
			Upload();
		} // Unset

		#endregion method Unset

		#region property IsSet

		public bool IsSet {
			get { return IsLeadSet(LeadID, LeadEmail); } // get
		} // IsSet

		#endregion property IsSet

		#region property Session

		public HttpSessionStateBase Session { get; private set; } // Session

		#endregion property Session

		#region property LeadID

		public int LeadID { get; private set; } // LeadID

		#endregion property LeadID

		#region property LeadEmail

		public string LeadEmail { get; private set; } // LeadEmail

		#endregion property LeadEmail

		#region property FirstName

		public string FirstName { get; private set; } // FirstName

		#endregion property FirstName

		#region property LastName

		public string LastName { get; private set; } // LastName

		#endregion property LastName

		#region property BrokerFillsForCustomer

		public bool BrokerFillsForCustomer { get; private set; } // BrokerFillsForCustomer

		#endregion property BrokerFillsForCustomer

		#endregion public

		#region private

		#region method Clear

		private void Clear() {
			LeadID = 0;
			LeadEmail = string.Empty;
			BrokerFillsForCustomer = false;
			FirstName = string.Empty;
			LastName = string.Empty;
		} // Clear

		#endregion method Clear

		#region method Upload

		private void Upload() {
			Session[Constant.BrokerLeadID] = LeadID;
			Session[Constant.BrokerLeadEmail] = LeadEmail;
			Session[Constant.BrokerLeadFirstName] = FirstName;
			Session[Constant.BrokerLeadLastName] = LastName;
			Session[Constant.BrokerFillsForCustomer] = BrokerFillsForCustomer ? Constant.Yes : Constant.No;
		} // Upload

		#endregion method Upload

		#region method IsSet

		private static bool IsLeadSet(int nLeadID, string sLeadEmail) {
			return (nLeadID > 0) && !string.IsNullOrWhiteSpace(sLeadEmail);
		} // IsSet

		#endregion method IsSet

		#endregion private
	} // class WizardBrokerLeadModel
} // namespace EzBob.Models
