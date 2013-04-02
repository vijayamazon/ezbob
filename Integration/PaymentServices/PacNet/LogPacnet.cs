using System;
using EZBob.DatabaseLib.Model.Database;
using StructureMap;
using log4net;

namespace PaymentServices.PacNet
{
    class LogPacnet <T> : IPacnetService where T : IPacnetService
    {
        private readonly T _t;
        private readonly PacnetPaypointServiceLogRepository _logRepository;
        private static readonly ILog log = LogManager.GetLogger("PaymentServices.PacNet");

        public LogPacnet(T t)
        {
            _t = t;
            _logRepository = ObjectFactory.GetInstance<PacnetPaypointServiceLogRepository>();
        }

        public PacnetReturnData SendMoney(int customerId, decimal amount, string bankNumber, string accountNumber, string accountName, string fileName = null, string currencyCode = "GBP", string description = null)
        {
            var pacnetSendMoney = string.Format("Pacnet Send Money. Amount = {0}, bankNumber = {1}", amount, bankNumber);
            log.InfoFormat("SendMoney: amount={0}, bankNumber={1}, accountNumber = {2}, accountName = {3}, currencyCode={4}, description={5}", amount, bankNumber, accountNumber, accountName, currencyCode, description);
            try
            {
                var result = _t.SendMoney(customerId, amount, bankNumber, accountNumber, accountName, fileName, currencyCode, description);
                Log(customerId, pacnetSendMoney);
                return result;
            }
            catch (Exception e)
            {
                log.Error(e);
                Log(customerId, pacnetSendMoney, false, e.ToString());
                throw;
            }
        }

        public PacnetReturnData CheckStatus(int customerId, string trackingNumber)
        {
            try
            {
                var result = _t.CheckStatus(customerId, trackingNumber);
                Log(customerId, "Pacnet Check Status");
                return result;
            }
            catch (Exception e)
            {
                Log(customerId, "Pacnet Check Status", false, e.ToString());
                throw;
            }
        }

        public PacnetReturnData CloseFile(int customerId, string fileName)
        {
            try
            {
                log.InfoFormat("Closing file {0}", fileName);
                var result = _t.CloseFile(customerId, fileName);
                Log(customerId, "Pacnet Close File");
                return result;
            }
            catch (Exception e)
            {
                Log(customerId, "Pacnet Close File", false, e.ToString());
                throw;
            }
        }

        private void Log(int customerId, string typeRequest, bool requestStatus = true, string errorMessage = "")
        {
            _logRepository.Log(customerId, DateTime.Now, typeRequest, requestStatus ? "Successful" : "Failed", errorMessage);
        }
    }
}
