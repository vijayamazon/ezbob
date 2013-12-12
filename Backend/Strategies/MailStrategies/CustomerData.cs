namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Data;
	using DbConnection;

	public class CustomerData
	{
		public string FirstName { get; set; }
		public string Surname { get; set; }
		public string FullName { get; set; }
		public string Mail { get; set; }
		public bool IsOffline { get; set; }
		public int NumOfLoans { get; set; }

		public CustomerData(int customerId)
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetBasicCustomerData", DbConnection.CreateParam("CustomerId", customerId));
			DataRow results = dt.Rows[0];

			FirstName = results["FirstName"].ToString();
			Surname = results["Surname"].ToString();
			FullName = results["FullName"].ToString();
			Mail = results["Mail"].ToString();
			IsOffline = bool.Parse(results["IsOffline"].ToString());
			NumOfLoans = int.Parse(results["NumOfLoans"].ToString());
		}
	}
}
