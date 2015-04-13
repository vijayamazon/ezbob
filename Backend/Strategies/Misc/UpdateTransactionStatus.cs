namespace Ezbob.Backend.Strategies.Misc {
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
                description = result.Error;
            } else if (result.Status.ToLower().Contains("inprogress")) {
                newStatus = "InProgress";
            } else if (result.Status.ToLower().Contains("submitted")) {
                newStatus = "Done";
                description = "Done";
            } else if (result.Status.ToLower().Contains("cleared")) {
                newStatus = "Done";
                description = "Cleared";
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
