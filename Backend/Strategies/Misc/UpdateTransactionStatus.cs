namespace EzBob.Backend.Strategies.Misc {
	using Ezbob.Database;
	using Ezbob.Logger;
	using PaymentServices.PacNet;
	using StructureMap;

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
			var lst = DB.ExecuteEnumerable("GetPacnetTransactions", CommandSpecies.StoredProcedure);

			var service = ObjectFactory.GetInstance<IPacnetService>();

			foreach (var sr in lst) {
				int customerId = sr["CustomerId"];
				string trackingNumber = sr["TrackingNumber"];
				
				PacnetReturnData result = service.CheckStatus(customerId, trackingNumber);

				string newStatus;
				string description = sr["allDescriptions"];

				if (string.IsNullOrEmpty(result.Status)) {
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
					description = "Done";
				}
				else if (result.Status.ToLower().Contains("cleared"))
				{
					newStatus = "Done";
					description = "Cleared";
				}
				else
				{
					newStatus = "Error";
					description = "Status: '" + result.Status + "' Error: " + result.Error;
				} // if

				Log.Debug(
					"UpdateTransactionStatus: CustomerId {4}, Tracking number {5}, " +
					"Pacnet Result: status: {0}, error: {1}, Update data: status {2}, description {3}",
					result.Status, result.Error, newStatus, description, customerId, trackingNumber
				);

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
