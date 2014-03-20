namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using Ezbob.Database;

	public class BrokerLeadData : CustomerData {
		#region constructor

		public BrokerLeadData(string sBrokerContactEmail) {
			m_sBrokerContactEmail = sBrokerContactEmail;
		} // constructor

		#endregion constructor

		#region method Load

		public override void Load(int nLeadID, AConnection oDB) {
			LeadID = 0;
			IsOffline = true;
			NumOfLoans = 0;

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					LeadID = sr["LeadID"];
					FirstName = sr["FirstName"];
					LastName = sr["LastName"];
					FullName = FirstName + " " + LastName;
					Email = sr["Email"];
					FirmName = sr["FirmName"];
					return ActionResult.SkipAll;
				},
				"BrokerLeadLoadDataForEmail",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LeadID", nLeadID),
				new QueryParameter("@ContactEmail", m_sBrokerContactEmail)
			);

			if (LeadID < 1)
				throw new Exception("Failed to find a lead by id " + nLeadID + " and contact email " + m_sBrokerContactEmail);
		} // Load

		#endregion method Load

		#region method ToString

		public override string ToString() {
			return string.Format(
				"broker lead {0}: {1} {2} ({3})",
				LeadID,
				FirstName,
				LastName,
				Email
			);
		} // ToString

		#endregion method ToString

		#region properties

		public virtual int LeadID {
			get { return Id; } // get
			protected set { Id = value; } // set
		} // LeadID

		public virtual string LastName {
			get { return Surname; } // get
			protected set { Surname = value; } // set
		} // LeadID

		public virtual string Email {
			get { return Mail; } // get
			protected set { Mail = value; } // set
		} // LeadID

		public virtual string FirmName { get; protected set; } // FirmName

		#endregion properties

		#region private

		private readonly string m_sBrokerContactEmail;

		#endregion private
	} // class CustomerData
} // namespace
