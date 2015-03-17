namespace Ezbob.Backend.Strategies.Broker
{
    using System;
    using System.Linq;
    using Ezbob.Database;
    using PaymentServices.PacNet;
    using StructureMap;

    public class BrokerTransferCommission : AStrategy
    {
        public BrokerTransferCommission()
        {
            this.service = ObjectFactory.GetInstance<IPacnetService>();
        }

        public override string Name { get { return "Broker Transfer Commission"; } }

        public override void Execute()
        {
            DateTime now = DateTime.UtcNow;
            string fileName = string.Format("ezbob-brokers-{0}-{1}-{2}-{3}", now.Year, now.Month, now.Day, now.Hour);
            var brokerCommissions = DB.ExecuteEnumerable("GetBrokerCommissionsForTransfer", CommandSpecies.StoredProcedure).ToList();

            if (!brokerCommissions.Any()) {
                Log.Debug("No broker commissions to transfer");
                return;
            }

            foreach (var sr in brokerCommissions)
            {
                int loanBrokerCommissionID = sr["LoanBrokerCommissionID"];
                int brokerID = sr["BrokerID"];

                decimal commissionAmount = sr["CommissionAmount"];

                string brokerName = sr["ContactName"];
                if (brokerName.Length > 18)
                    brokerName = brokerName.Substring(0, 17);

                string accountNumber = sr["BankAccount"];
                string sortcode = sr["SortCode"];
                var response = this.service.SendMoney(brokerID, commissionAmount, sortcode, accountNumber, brokerName, fileName, "GBP", "Commission");

                Log.Debug("PacNet sending commission of {0} to broker {1} tracking {2} {3}",
                    commissionAmount, brokerID, response.TrackingNumber, response.HasError ? "error: " + response.Error : "");


                DB.ExecuteNonQuery("UpdateBrokerCommissionTransferStatus",
                    CommandSpecies.StoredProcedure,
                    new QueryParameter("LoanBrokerCommissionID", loanBrokerCommissionID),
                    new QueryParameter("TrackingNumber", response.TrackingNumber),
                    new QueryParameter("Status"),
                    new QueryParameter("Description", "Commission"),
                    new QueryParameter("Now", now)
                );
            } // foreach  

            this.service.CloseFile(1, fileName);
        }

        private readonly IPacnetService service;
    }
}
