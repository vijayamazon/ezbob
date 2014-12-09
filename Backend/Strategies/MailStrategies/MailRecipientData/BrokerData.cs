namespace Ezbob.Backend.Strategies.MailStrategies {
	using Exceptions;
	using Ezbob.Database;

	public class BrokerData : CustomerData {

		public BrokerData(AStrategy oStrategy, int nBrokerID, AConnection oDB) : base(oStrategy, nBrokerID, oDB) {} // constructor

		public override void Load() {
			BrokerID = 0;
			IsOffline = true;
			NumOfLoans = 0;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					BrokerID = sr["BrokerID"];
					FirstName = sr["ContactName"];
					LastName = sr["ContactName"];
					FullName = sr["ContactName"];
					Email = sr["ContactEmail"];
					FirmName = sr["FirmName"];
					UserID = sr["UserID"];
					return ActionResult.SkipAll;
				},
				"BrokerLoadContactData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@BrokerID", RequestedID)
			);

			if (BrokerID != RequestedID)
				throw new StrategyWarning(Strategy, "Failed to find a broker by id " + RequestedID);
		} // Load

		public virtual int BrokerID {
			get { return Id; } // get
			protected set { Id = value; } // set
		} // BrokerID

		public virtual string LastName {
			get { return Surname; } // get
			protected set { Surname = value; } // set
		} // LeadID

		public virtual string Email {
			get { return Mail; } // get
			protected set { Mail = value; } // set
		} // LeadID

		public virtual string FirmName { get; set; } // FirmName

	} // class CustomerData
} // namespace
