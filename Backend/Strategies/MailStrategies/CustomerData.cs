namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Data;
	using Ezbob.Database;

	public class CustomerData {
		#region constructor

		public CustomerData(int customerId, AConnection oDb) {
			DataTable dt = oDb.ExecuteReader(
				"GetBasicCustomerData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			Id = customerId;

			if (dt.Rows.Count != 1)
				throw new Exception("Failed to find a customer by id " + customerId);

			var sr = new SafeReader(dt.Rows[0]);

			FirstName = sr.String("FirstName");
			Surname = sr.String("Surname");
			FullName = sr.String("FullName");
			Mail = sr.String("Mail");
			IsOffline = sr.Bool("IsOffline");
			NumOfLoans = sr.Int("NumOfLoans");
		} // constructor

		#endregion constructor

		#region method ToString

		public override string ToString() {
			return string.Format(
				"{0}: {1} {2} ({5}, {3}) {4} loan #: {6}",
				Id,
				FirstName,
				Surname,
				FullName,
				Mail,
				IsOffline ? "offline" : "online",
				NumOfLoans
			);
		} // ToString

		#endregion method ToString

		#region properties

		public int Id { get; private set; }
		public string FirstName { get; private set; }
		public string Surname { get; private set; }
		public string FullName { get; private set; }
		public string Mail { get; private set; }
		public bool IsOffline { get; private set; }
		public int NumOfLoans { get; private set; }

		#endregion properties
	} // class CustomerData
} // namespace
