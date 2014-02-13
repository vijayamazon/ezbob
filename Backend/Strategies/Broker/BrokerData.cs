namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Data;
	using Ezbob.Database;
	using MailStrategies;

	public class BrokerData : CustomerData {
		#region method Load

		public override void Load(int nBrokerID, AConnection oDB) {
			DataTable dt = oDB.ExecuteReader(
				"BrokerLoadContactData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@BrokerID", nBrokerID)
			);

			Id = nBrokerID;

			if (dt.Rows.Count != 1)
				throw new Exception("Failed to find a broker by id " + nBrokerID);

			var sr = new SafeReader(dt.Rows[0]);

			FirstName = sr["ContactName"];
			Surname = sr["ContactName"];
			FullName = sr["ContactName"];
			Mail = sr["ContactEmail"];
			IsOffline = true;
			NumOfLoans = 0;
		} // Load

		#endregion method Load
	} // class CustomerData
} // namespace
