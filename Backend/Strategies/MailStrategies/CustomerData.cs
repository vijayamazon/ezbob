using System;
using System.Data;
using Ezbob.Database;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class CustomerData {
		#region constructor

		public CustomerData(int customerId, AConnection oDB) {
			DataTable dt = oDB.ExecuteReader(
				"GetBasicCustomerData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			ID = customerId;

			if (dt.Rows.Count != 1)
				throw new Exception("Failed to find a customer by id " + customerId);

			DataRow results = dt.Rows[0];

			FirstName = results["FirstName"].ToString();
			Surname = results["Surname"].ToString();
			FullName = results["FullName"].ToString();
			Mail = results["Mail"].ToString();
			IsOffline = Convert.ToBoolean(results["IsOffline"]);
			NumOfLoans = int.Parse(results["NumOfLoans"].ToString());
		} // constructor

		#endregion constructor

		#region method ToString

		public override string ToString() {
			return string.Format(
				"{0}: {1} {2} ({5}, {3}) {4} loan #: {6}",
				ID,
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

		public int ID { get; private set; }
		public string FirstName { get; private set; }
		public string Surname { get; private set; }
		public string FullName { get; private set; }
		public string Mail { get; private set; }
		public bool IsOffline { get; private set; }
		public int NumOfLoans { get; private set; }

		#endregion properties
	} // class CustomerData
} // namespace
