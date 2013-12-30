using System;

namespace PaymentServices.PacNet
{
	using log4net;

	public class FakePacnetService : IPacnetService
    {
		private static readonly ILog Log = LogManager.GetLogger(typeof(FakePacnetService));

        private Random _random = new Random();
        public virtual PacnetReturnData SendMoney(int customerId, decimal amount, string bankNumber, string accountNumber, string accountName, string fileName = null, string currencyCode = "GBP", string description = null)
        {
			Log.DebugFormat("Fake SendMoney customerId {0} amount {1} bankNumber {2} accountNumber {3} accountName {4} fileName {5} currencyCode {6} description {7}", customerId, amount, bankNumber, accountNumber, accountName, fileName, currencyCode, description);
            var trackingNumber = _random.Next(111111111, 999999999);
            var pacnetReturnData = new PacnetReturnData()
                                       {
                                           Status = "Submitted",
                                           TrackingNumber = trackingNumber.ToString()
                                       };
            return pacnetReturnData;
        }

        public virtual PacnetReturnData CheckStatus(int customerId, string trackingNumber)
        {
	        string[] statuses = {"inprogress", "submitted", "error"};
	        int r = new Random(DateTime.Now.Millisecond).Next(0, 2);
            var pacnetReturnData = new PacnetReturnData()
                                       {
                                           Status = statuses[r],
                                           TrackingNumber = trackingNumber,
										   Error = "Fake " + statuses[r]
                                       };

			Log.DebugFormat("Fake CheckStatus customerId {0} trackingNumber {1} Status {2}", customerId, trackingNumber, pacnetReturnData.Status);

            return pacnetReturnData;
        }

        public virtual PacnetReturnData CloseFile(int customerId, string fileName)
        {
            var pacnetReturnData = new PacnetReturnData()
            {
            };
            return pacnetReturnData;
        }
    }

    public class FailingPacnetService : FakePacnetService
    {
        public override PacnetReturnData SendMoney(int customerId, decimal amount, string bankNumber, string accountNumber, string accountName, string fileName = null, string currencyCode = "GBP", string description = null)
        {
            throw new PacnetException("SendMoney Failed");
        }        
    }
}
