namespace EzBob.Backend.Strategies
{
	using System.Data;
	using PaymentServices.PacNet;
	using log4net;
	using DbConnection;

	public class UpdateTransactionStatus
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(UpdateTransactionStatus));

		public void Execute()
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetPacnetTransactions");
			foreach (DataRow row in dt.Rows)
			{
				int customerId = int.Parse(row["CustomerId"].ToString());
				string trackingNumber = row["TrackingNumber"].ToString();

				var service = new PacnetService();
				PacnetReturnData result = service.CheckStatus(customerId, trackingNumber);

				string newStatus;
				string description = null;

				if (string.IsNullOrEmpty(result.Status))
				{
					newStatus = "Error";
					description = result.Error;
				}
				else if (result.Status.ToLower().Contains("inprogress"))
				{
					newStatus = "InProgress";
				}
				else if (result.Status.ToLower().Contains("submitted"))
				{
					newStatus = "Done";
				}
				else
				{
					newStatus = "Error";
					description = result.Status;
				}

				DbConnection.ExecuteSpNonQuery("GetPacnetTransactions",
					DbConnection.CreateParam("TrackingId", trackingNumber),
					DbConnection.CreateParam("TransactionStatus", newStatus),
					DbConnection.CreateParam("Description", description));
			}
		}
	}
}
