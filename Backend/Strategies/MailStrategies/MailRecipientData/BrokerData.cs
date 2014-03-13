namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using Ezbob.Database;

	public class BrokerData : CustomerData {
		#region method Load

		public override void Load(int nBrokerID, AConnection oDB) {
			BrokerID = 0;
			IsOffline = true;
			NumOfLoans = 0;

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					BrokerID = sr["BrokerID"];
					FirstName = sr["ContactName"];
					LastName = sr["ContactName"];
					FullName = sr["ContactName"];
					Email = sr["ContactEmail"];
					return ActionResult.SkipAll;
				},
				"BrokerLoadContactData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@BrokerID", nBrokerID)
			);

			if (BrokerID < 0)
				throw new Exception("Failed to find a broker by id " + nBrokerID);
		} // Load

		#endregion method Load

		#region properties

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

		#endregion properties
	} // class CustomerData
} // namespace
