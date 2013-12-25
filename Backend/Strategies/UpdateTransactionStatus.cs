﻿namespace EzBob.Backend.Strategies {
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using PaymentServices.PacNet;

	public class UpdateTransactionStatus : AStrategy {
		#region constructor

		public UpdateTransactionStatus(AConnection oDb, ASafeLog oLog) : base(oDb, oLog) { } // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Update Transaction Status"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			DataTable dt = Db.ExecuteReader("GetPacnetTransactions", CommandSpecies.StoredProcedure);

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

				Log.Debug("UpdateTransactionStatus: CustomerId {4}, Tracking number {5}, Pacnet Result: status: {0}, error: {1}, Update data: status {2}, description {3}", result.Status, result.Error, newStatus, description);
				Db.ExecuteNonQuery("UpdateTransactionStatus",
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
