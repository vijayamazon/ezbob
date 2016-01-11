namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Linq;
	using DbConstants;
	using Ezbob.Database;
    using PaymentServices.PacNet;
    using StructureMap;

    public class UpdateTransactionStatus : AStrategy {
        public UpdateTransactionStatus() {
            this.service = ObjectFactory.GetInstance<IPacnetService>();
        } //construction

        public override string Name {
            get { return "Update Transaction Status"; }
        } // Name

        public override void Execute() {
            UpdateLoanTransactionStatus();
            UpdateBrokerCommissionTransactionStatus();
        }// Execute


        private void UpdateLoanTransactionStatus() {
            var lst = DB.ExecuteEnumerable("GetPacnetTransactions", CommandSpecies.StoredProcedure);

            foreach (var sr in lst) {

                int customerId = sr["CustomerId"];
                string trackingNumber = sr["TrackingNumber"];

                Log.Debug("Checking PacNet transaction status for customer {0} tracking number {1}", customerId, trackingNumber);

                string newStatus;
                string description = sr["allDescriptions"];
                GetStatus(customerId, trackingNumber, out newStatus, ref description);

                DB.ExecuteNonQuery("UpdateTransactionStatus",
                    CommandSpecies.StoredProcedure,
                    new QueryParameter("TrackingId", trackingNumber),
                    new QueryParameter("TransactionStatus", newStatus),
                    new QueryParameter("Description", description)
                );


				// UPDATE [dbo].[LoanTransaction] SET [Status] = @TransactionStatus, PacnetStatus=@TransactionStatus, [Description] = @Description WHERE TrackingNumber = @TrackingId

				// update NL_PacnetTransactions:
				// PacnetTransactionStatusID = @TransactionStatus 
				// TrackingNumber = @TrackingId
				// StatusUpdatedTime = now
				// Notes = @Description
	            try {
		            var nlStatus = Enum.GetNames(typeof(NLPacnetTransactionStatuses)).FirstOrDefault(s => s.Equals(newStatus));
		            if (nlStatus != null) {
			            int nlStatusID = (int)Enum.Parse(typeof(NLPacnetTransactionStatuses), nlStatus);
			            DB.ExecuteNonQuery("NL_PacnetTransactionsUpdate",
				            CommandSpecies.StoredProcedure,
				            new QueryParameter("TrackingNumber", trackingNumber),
				            new QueryParameter("PacnetTransactionStatusID", nlStatusID),
				            new QueryParameter("Notes", description),
				            new QueryParameter("UpdateTime", DateTime.UtcNow),
				            new QueryParameter("FundTransferActive", (newStatus.Equals("Done") ? 1 : 0))
				        );
		            }
	            } catch (OverflowException overflowException) {
					Log.Alert("Failed to get status {0} from NLPacnetTransactionStatuses. {1}", newStatus, overflowException);
		            // ReSharper disable once CatchAllClause
	            } catch (Exception ex) {
					Log.Alert("Failed to update NL_PacnetTransactionsUpdate, NL_FundTransfers for customer: {0}, trackingNumber: {1}, {2}", customerId, trackingNumber, ex);
	            }

            } // foreach
        }//UpdateLoanTransactionStatus

        private void UpdateBrokerCommissionTransactionStatus() {
            DB.ForEachRowSafe(HandleOneBrokerCommissionTransactionStatus, "GetBrokerCommissionsForStatusUpdate", CommandSpecies.StoredProcedure);
        }//UpdateBrokerCommissionTransactionStatus

        private ActionResult HandleOneBrokerCommissionTransactionStatus(SafeReader sr, bool b) {
            int loanBrokerCommissionID = sr["LoanBrokerCommissionID"];
            string trackingNumber = sr["TrackingNumber"];
            int brokerID = sr["BrokerID"];

            Log.Debug("Checking PacNet transaction status for broker {0} tracking number {1}", brokerID, trackingNumber);
            string newStatus;
            string description = sr["Description"];
            GetStatus(brokerID, trackingNumber, out newStatus, ref description);

            DB.ExecuteNonQuery("UpdateBrokerCommissionTransferStatus",
                CommandSpecies.StoredProcedure,
                new QueryParameter("LoanBrokerCommissionID", loanBrokerCommissionID),
                new QueryParameter("TrackingNumber", trackingNumber),
                new QueryParameter("TransactionStatus", newStatus),
                new QueryParameter("Description", description),
                new QueryParameter("Now")
            );
            return ActionResult.Continue;
        }//HandleOneBrokerCommissionTransactionStatus



        private void GetStatus(int userId, string trackingNumber, out string newStatus, ref string description) {
            PacnetReturnData result = this.service.CheckStatus(userId, trackingNumber);

            if (string.IsNullOrEmpty(result.Status)) {
                newStatus = "Error";
                description = "Status is empty, " + result.Error;
            } else if (result.Status.ToLower().Contains("inprogress")) {
                newStatus = "InProgress";
            } else if (result.Status.ToLower().Contains("submitted")) {
                newStatus = "Done";
				description = result.Status;
            } else if (result.Status.ToLower().Contains("cleared")) {
                newStatus = "Done";
				description = result.Status;
            } else {
                newStatus = "Error";
                description = "Status: '" + result.Status + "' Error: " + result.Error;
            } // if

            Log.Debug(
                "UpdateTransactionStatus: Tracking number {4}, " +
                "PacNet Result: status: {0}, error: {1}, Update data: status {2}, description {3}",
                result.Status, result.Error, newStatus, description, trackingNumber
                );
        } //GetStatus

        private readonly IPacnetService service;
    } // UpdateTransactionStatus
} // namespace
