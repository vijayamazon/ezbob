﻿using System;
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

        public PacnetReturnData SendMoney(int userId, decimal amount, string bankNumber, string accountNumber, string accountName, string fileName = null, string currencyCode = "GBP", string description = null)
        {
            var pacnetSendMoney = string.Format("Pacnet Send Money. Amount = {0}, bankNumber = {1}, file name {2} description {3}", amount, bankNumber, fileName, description);
            log.InfoFormat("SendMoney: amount={0}, bankNumber={1}, accountNumber = {2}, accountName = {3}, currencyCode={4}, description={5}", amount, bankNumber, accountNumber, accountName, currencyCode, description);
            try
            {
                var result = _t.SendMoney(userId, amount, bankNumber, accountNumber, accountName, fileName, currencyCode, description);
                Log(userId, pacnetSendMoney);
                return result;
            }
            catch (Exception e)
            {
                log.Error(e);
                Log(userId, pacnetSendMoney, false, e.ToString());
                throw;
            }
        }

        public PacnetReturnData CheckStatus(int userId, string trackingNumber)
        {
            try
            {
                var result = _t.CheckStatus(userId, trackingNumber);
                Log(userId, string.Format("Pacnet Check Status of track number {0} result {1} {2}", trackingNumber, result.Status, result.Error));
                return result;
            }
            catch (Exception e)
            {
                Log(userId, "Pacnet Check Status", false, e.ToString());
                throw;
            }
        }

        public PacnetReturnData CloseFile(int userId, string fileName)
        {
            try
            {
                log.InfoFormat("Closing file {0}", fileName);
                var result = _t.CloseFile(userId, fileName);
                Log(userId, string.Format("Pacnet Close File tracking {0} status {1} {2}", fileName, result.TrackingNumber, result.Status, result.Error));
                return result;
            }
            catch (Exception e)
            {
                Log(userId, string.Format("Pacnet Close File file name {0}", fileName), false, e.ToString());
                throw;
            }
        }

        private void Log(int userId, string typeRequest, bool requestStatus = true, string errorMessage = "")
        {
            _logRepository.Log(userId, DateTime.Now, typeRequest, requestStatus ? "Successful" : "Failed", errorMessage);
        }
    }
}
