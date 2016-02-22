namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	public class BrokerLoadCustomerDetails : AStrategy {
		public BrokerLoadCustomerDetails(string sCustomerRefNum, string sContactEmail, CustomerOriginEnum origin) {
			m_sCustomerRefNum = sCustomerRefNum;
			m_sContactEmail = sContactEmail;
			this.origin = origin;
			Result = new BrokerCustomerDetails();
			PotentialEsigners = new List<Esigner>();
		} // constructor

		public override string Name {
			get { return "Broker load customer details"; }
		} // Name

		public class BrokerLoadCustomerDetailsRawData {
			public int CustomerID { get; set; }
			public string FirstName { get; set; }
			public string Surname { get; set; }
			public DateTime DateOfBirth { get; set; }
			public string Gender { get; set; }
			public string Email { get; set; }
			public string MaritalStatus { get; set; }
			public string MobilePhone { get; set; }
			public string DaytimePhone { get; set; }
			public string Organisation { get; set; }
			public string Line1 { get; set; }
			public string Line2 { get; set; }
			public string Line3 { get; set; }
			public string Pobox { get; set; }
			public string Town { get; set; }
			public string County { get; set; }
			public string Postcode { get; set; }
			public string Country { get; set; }
			public int LeadID { get; set; }
			public bool FinishedWizard { get; set; }

			public void ToModel(BrokerCustomerPersonalData oModel) {
				oModel.id = CustomerID;
				oModel.name = string.Join(" ", FirstName, Surname);
				oModel.birthdate = DateOfBirth;
				oModel.gender = Gender;
				oModel.email = Email;
				oModel.maritalstatus = MaritalStatus;
				oModel.mobilephone = MobilePhone;
				oModel.daytimephone = DaytimePhone;
				oModel.address = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}",
					string.IsNullOrEmpty(Organisation) ? "" : Organisation + "\n",
					string.IsNullOrEmpty(Line1) ? "" : Line1 + "\n",
					string.IsNullOrEmpty(Line2) ? "" : Line2 + "\n",
					string.IsNullOrEmpty(Line3) ? "" : Line3 + "\n",
					string.IsNullOrEmpty(Pobox) ? "" : Pobox + "\n",
					string.IsNullOrEmpty(Town) ? "" : Town + "\n",
					string.IsNullOrEmpty(Postcode) ? "" : Postcode + "\n",
					string.IsNullOrEmpty(County) ? "" : County + "\n",
					string.IsNullOrEmpty(Country) ? "" : Country + "\n"
				);
				oModel.leadID = LeadID;
				oModel.finishedWizard = FinishedWizard;
			} // ToModel

		} // BrokerLoadCustomerDetailsRawData

		public override void Execute() {
			BrokerLoadCustomerDetailsRawData raw = DB.FillFirst<BrokerLoadCustomerDetailsRawData>(
				"BrokerLoadCustomerDetails",
				new QueryParameter("@RefNum", m_sCustomerRefNum),
				new QueryParameter("@ContactEmail", m_sContactEmail),
				new QueryParameter("@Origin", (int)this.origin)
			);

			if (raw == null) {
				Log.Warn("{0}: personal details not found for customer {1} (broker {2}).", Name, m_sCustomerRefNum, m_sContactEmail);
				return;
			} // if

			raw.ToModel(Result.PersonalData);

			Result.CrmData = DB.Fill<BrokerCustomerCrmEntry>(
				"BrokerLoadCustomerCRM",
				new QueryParameter("@RefNum", m_sCustomerRefNum),
				new QueryParameter("@ContactEmail", m_sContactEmail),
				new QueryParameter("@Origin", (int)this.origin)
			);

			PotentialEsigners = DB.Fill<Esigner>(
				"LoadPotentialEsigners",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", raw.CustomerID)
			);
		} // Execute

		public BrokerCustomerDetails Result { get; private set; } // Result

		public List<Esigner> PotentialEsigners { get; private set; }

		private readonly string m_sCustomerRefNum;
		private readonly string m_sContactEmail;
		private readonly CustomerOriginEnum origin;
	} // class BrokerLoadCustomerDetails
} // namespace Ezbob.Backend.Strategies.Broker
