using System;

namespace PaymentServices.PacNet
{
    public class FakePacnetService : IPacnetService
    {
        private Random _random = new Random();
        public virtual PacnetReturnData SendMoney(int customerId, decimal amount, string bankNumber, string accountNumber, string accountName, string fileName = null, string currencyCode = "GBP", string description = null)
        {
            var trackingNumber = _random.Next(111111111, 999999999);
            var pacnetReturnData = new PacnetReturnData()
                                       {
                                           Status = "Submited",
                                           TrackingNumber = trackingNumber.ToString()
                                       };
            return pacnetReturnData;
        }

        public virtual PacnetReturnData CheckStatus(int customerId, string trackingNumber)
        {
            var pacnetReturnData = new PacnetReturnData()
                                       {
                                           Status = "Submited",
                                           TrackingNumber = trackingNumber
                                       };
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
