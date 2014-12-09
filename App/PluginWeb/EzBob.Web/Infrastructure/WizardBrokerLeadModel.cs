namespace EzBob.Web.Infrastructure {
	using System.Web;

	public sealed class WizardBrokerLeadModel {

		public WizardBrokerLeadModel(HttpSessionStateBase oSession) {
			Session = oSession;

			LeadID = (int)(Session[Constant.Broker.LeadID] ?? 0);
			LeadEmail = (Session[Constant.Broker.LeadEmail] ?? string.Empty).ToString();
			BrokerFillsForCustomer = (Session[Constant.Broker.FillsForCustomer] ?? Constant.No).ToString() == Constant.Yes;
			FirstName = (Session[Constant.Broker.LeadFirstName] ?? string.Empty).ToString();
			LastName = (Session[Constant.Broker.LeadLastName] ?? string.Empty).ToString();
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

		public void Unset() {
			Clear();
			Upload();
		} // Unset

		public bool IsSet {
			get { return IsLeadSet(LeadID, LeadEmail); } // get
		} // IsSet

		public HttpSessionStateBase Session { get; private set; } // Session

		public int LeadID { get; private set; } // LeadID

		public string LeadEmail { get; private set; } // LeadEmail

		public string FirstName { get; private set; } // FirstName

		public string LastName { get; private set; } // LastName

		public bool BrokerFillsForCustomer {
			get { return IsSet && m_bBrokerFillsForCustomer; }
			private set { m_bBrokerFillsForCustomer = value; }
		} // BrokerFillsForCustomer

		private bool m_bBrokerFillsForCustomer;

		public override string ToString() {
			return string.Format(
				"[ WizardBrokerLeadModel: is set = {0}, id = {1}, details: {3} {4} - {2}, broker fills: {5} ]",
				IsSet ? "yes" : "no",
				LeadID,
				LeadEmail,
				FirstName,
				LastName,
				BrokerFillsForCustomer ? "yes" : "no"
			);
		} // ToString

		private void Clear() {
			LeadID = 0;
			LeadEmail = string.Empty;
			BrokerFillsForCustomer = false;
			FirstName = string.Empty;
			LastName = string.Empty;
		} // Clear

		private void Upload() {
			Session[Constant.Broker.LeadID] = LeadID;
			Session[Constant.Broker.LeadEmail] = LeadEmail;
			Session[Constant.Broker.LeadFirstName] = FirstName;
			Session[Constant.Broker.LeadLastName] = LastName;
			Session[Constant.Broker.FillsForCustomer] = BrokerFillsForCustomer ? Constant.Yes : Constant.No;
		} // Upload

		private static bool IsLeadSet(int nLeadID, string sLeadEmail) {
			return (nLeadID > 0) && !string.IsNullOrWhiteSpace(sLeadEmail);
		} // IsLeadSet

	} // class WizardBrokerLeadModel
} // namespace EzBob.Models
