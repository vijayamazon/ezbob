namespace Ezbob.Backend.Strategies.MailStrategies {
	using Exceptions;
	using Ezbob.Database;

	public class BrokerLeadData : CustomerData {
		public BrokerLeadData(
			string sBrokerContactEmail,
			int origin,
			AStrategy oStrategy,
			int nLeadID,
			AConnection oDB
		) : base(oStrategy, nLeadID, oDB) {
			m_sBrokerContactEmail = sBrokerContactEmail;
			this.origin = origin;
		} // constructor

		public override void Load() {
			LeadID = 0;
			IsOffline = true;
			NumOfLoans = 0;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					LeadID = sr["LeadID"];
					FirstName = sr["FirstName"];
					LastName = sr["LastName"];
					FullName = FirstName + " " + LastName;
					Email = sr["Email"];
					FirmName = sr["FirmName"];
					Origin = sr["Origin"];
					OriginSite = sr["OriginSite"];
					return ActionResult.SkipAll;
				},
				"BrokerLeadLoadDataForEmail",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LeadID", RequestedID),
				new QueryParameter("@ContactEmail", m_sBrokerContactEmail),
				new QueryParameter("@OriginID", this.origin)
			);

			if (LeadID != RequestedID)
				throw new StrategyWarning(Strategy, "Failed to find a lead by id " + RequestedID + " and contact email " + m_sBrokerContactEmail);
		} // Load

		public override string ToString() {
			return string.Format(
				"broker lead {0}: {1} {2} ({3})",
				LeadID,
				FirstName,
				LastName,
				Email
			);
		} // ToString

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

		private readonly string m_sBrokerContactEmail;
		private readonly int origin;
	} // class CustomerData
} // namespace
