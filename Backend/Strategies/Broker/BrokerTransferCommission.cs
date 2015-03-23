namespace Ezbob.Backend.Strategies.Broker {
    using System;
    using Ezbob.Database;
    using PaymentServices.PacNet;
    using StructureMap;

    public class BrokerTransferCommission : AStrategy {
        public BrokerTransferCommission() {
            this.service = ObjectFactory.GetInstance<IPacnetService>();
        }

        public override string Name { get { return "Broker Transfer Commission"; } }

        public override void Execute() {
            this.now = DateTime.UtcNow;
            this.fileName = string.Format("ezbob-brokers-{0}-{1}-{2}-{3}", this.now.Year, this.now.Month, this.now.Day, this.now.Hour);
            this.anyCommission = false;
            DB.ForEachRowSafe(HandleOneCommissionTransfer, "GetBrokerCommissionsForTransfer", CommandSpecies.StoredProcedure);

            if (this.anyCommission) {
                this.service.CloseFile(1, this.fileName);
            } else {
                Log.Info("No commissions where transferred");
            }//if
        }//Execute

        private ActionResult HandleOneCommissionTransfer(SafeReader sr, bool b) {
            int loanBrokerCommissionID = sr["LoanBrokerCommissionID"];
            int brokerID = sr["BrokerID"];

            decimal commissionAmount = sr["CommissionAmount"];

            string brokerName = sr["ContactName"];
            if (brokerName.Length > 18)
                brokerName = brokerName.Substring(0, 17);

            string accountNumber = sr["BankAccount"];
            string sortcode = sr["SortCode"];
            var response = this.service.SendMoney(brokerID, commissionAmount, sortcode, accountNumber, brokerName, this.fileName, "GBP", "Commission");

            Log.Info("PacNet sending commission of {0} to broker {1} tracking {2} {3}",
                commissionAmount, brokerID, response.TrackingNumber, response.HasError ? "error: " + response.Error : "");


            DB.ExecuteNonQuery("UpdateBrokerCommissionTransferStatus",
                CommandSpecies.StoredProcedure,
                new QueryParameter("LoanBrokerCommissionID", loanBrokerCommissionID),
                new QueryParameter("TrackingNumber", response.TrackingNumber),
                new QueryParameter("TransactionStatus"),
                new QueryParameter("Description", "Commission"),
                new QueryParameter("Now", this.now)
            );

            this.anyCommission = true;
            return ActionResult.Continue;
        }//HandleOneCommissionTransfer

        private readonly IPacnetService service;
        private string fileName;
        private bool anyCommission;
        private DateTime now;
    }//class
}//ns
