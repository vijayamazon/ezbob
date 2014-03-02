namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	#region class BrokerLoadCustomerDetails

	public class BrokerLoadCustomerDetails : AStrategy {
		#region public

		#region constructor

		public BrokerLoadCustomerDetails(int nCustomerID, string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_sContactEmail = sContactEmail;
			Result = new BrokerCustomerDetails();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker load customer details"; }
		} // Name

		#endregion property Name

		#region class BrokerLoadCustomerDetailsRawData

		public class BrokerLoadCustomerDetailsRawData : ITraversable {
			#region properties

			public int CustomerID { get; set; }
			public string FirstName { get; set; }
			public string Surname { get; set; }
			public DateTime DateOfBirth { get; set; }
			public string Gender { get; set; }
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

			#endregion properties

			#region method ToModel

			public void ToModel(BrokerCustomerPersonalData oModel) {
				oModel.id = CustomerID;
				oModel.name = string.Join(" ", FirstName, Surname);
				oModel.birthdate = DateOfBirth;
				oModel.gender = Gender;
				oModel.maritalstatus = MaritalStatus;
				oModel.mobilephone = MobilePhone;
				oModel.daytimephone = DaytimePhone;
				oModel.address = string.Join("\n",
					Organisation, Line1, Line2, Line3,
					Pobox, Town, Postcode, County, Country
				);
			} // ToModel

			#endregion method ToModel
		} // BrokerLoadCustomerDetailsRawData

		#endregion class BrokerLoadCustomerDetailsRawData

		#region class BrokerLoadCustomerCrmRawData

		public class BrokerLoadCustomerCrmRawData : ITraversable {
			#region properties

			public DateTime CrDate { get; set; }
			public string ActionName { get; set; }
			public string StatusName { get; set; }
			public string Comment { get; set; }

			#endregion properties

			#region method ToModel

			public BrokerCustomerCrmEntry ToModel() {
				return new BrokerCustomerCrmEntry {
					CrDate = CrDate,
					ActionName = ActionName,
					StatusName = StatusName,
					Comment = Comment,
				};
			} // ToModel

			#endregion method ToModel
		} // class BrokerLoadCustomerCrmRawData

		#endregion class BrokerLoadCustomerCrmRawData

		#region method Execute

		public override void Execute() {
			BrokerLoadCustomerDetailsRawData raw = DB.FillFirst<BrokerLoadCustomerDetailsRawData>(
				"BrokerLoadCustomerDetails",
				new QueryParameter("@CustomerID", m_nCustomerID),
				new QueryParameter("@ContactEmail", m_sContactEmail)
			);

			if (raw == null) {
				Log.Warn("{0}: personal details not found for customer {1} (broker {2}).", Name, m_nCustomerID, m_sContactEmail);
				return;
			} // if

			raw.ToModel(Result.PersonalData);

			List<BrokerLoadCustomerCrmRawData> lst = DB.Fill<BrokerLoadCustomerCrmRawData>(
				"BrokerLoadCustomerCRM",
				new QueryParameter("@CustomerID", m_nCustomerID),
				new QueryParameter("@ContactEmail", m_sContactEmail)
			);

			foreach (BrokerLoadCustomerCrmRawData crm in lst)
				Result.CrmData.Add(crm.ToModel());
		} // Execute

		#endregion method Execute

		#region property Result

		public BrokerCustomerDetails Result { get; private set; } // Result

		#endregion property Result

		#endregion public

		#region private

		private int m_nCustomerID;
		private readonly string m_sContactEmail;

		#endregion private
	} // class BrokerLoadCustomerDetails

	#endregion class BrokerLoadCustomerDetails
} // namespace EzBob.Backend.Strategies.Broker
