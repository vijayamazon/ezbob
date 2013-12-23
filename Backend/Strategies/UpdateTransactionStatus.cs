using System.Data;
using Ezbob.Database;
using Ezbob.Logger;
using PaymentServices.PacNet;

namespace EzBob.Backend.Strategies {
	public class UpdateTransactionStatus : AStrategy {
		#region constructor

		public UpdateTransactionStatus(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Update Transaction Status"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			DataTable dt = DB.ExecuteReader("GetPacnetTransactions", CommandSpecies.StoredProcedure);

			foreach (DataRow row in dt.Rows) {
				int customerId = int.Parse(row["CustomerId"].ToString());
				string trackingNumber = row["TrackingNumber"].ToString();

				var service = new PacnetService();
				PacnetReturnData result = service.CheckStatus(customerId, trackingNumber);

				string newStatus;
				string description = null;

				if (string.IsNullOrEmpty(result.Status)) {
					newStatus = "Error";
					description = result.Error;
				}
				else if (result.Status.ToLower().Contains("inprogress"))
					newStatus = "InProgress";
				else if (result.Status.ToLower().Contains("submitted"))
					newStatus = "Done";
				else {
					newStatus = "Error";
					description = result.Status;
				} // if

				DB.ExecuteNonQuery("UpdateTransactionStatus",
					CommandSpecies.StoredProcedure,
					new QueryParameter("TrackingId", trackingNumber),
					new QueryParameter("TransactionStatus", newStatus),
					new QueryParameter("Description", description)
				);
			} // foreach
		} // Execute

		#endregion method Execute
	} // UpdateTransactionStatus
} // namespace
