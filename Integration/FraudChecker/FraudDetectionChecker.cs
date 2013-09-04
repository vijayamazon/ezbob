using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Fraud;
using NHibernate;
using StructureMap;

namespace FraudChecker
{
    public class FraudDetectionChecker
    {
        private readonly ISession _session;
        private readonly InternalChecker _internalChecker;
        private readonly ExternalChecker _externalChecker;
        private readonly BussinessChecker _bussinessChecker;

        public FraudDetectionChecker()
        {
            _bussinessChecker = new BussinessChecker();
            _internalChecker = new InternalChecker();
            _externalChecker = new ExternalChecker();
            _session = ObjectFactory.GetInstance<ISession>();
        }

        /// <summary>
        /// run fraud all checks
        /// </summary>
        /// <param name="customerId">Customer.Id for check</param>
        /// <returns></returns>
        public string Check(int customerId)
        {
            var startDate = DateTime.UtcNow;
            var detections = new List<FraudDetection>();
            detections.AddRange(_internalChecker.InternalSystemDecision(customerId));
            detections.AddRange(_externalChecker.ExternalSystemDecision(customerId));
            detections.AddRange(_bussinessChecker.SpecialBussinesRulesSystemDecision(customerId));

            SaveInDB(detections, startDate, customerId);
            return Helper.PrepareResultForOutput(detections);
        }

        private void SaveInDB(IList<FraudDetection> fraudDetections, DateTime startDate, int customerId)
        {
            var count = fraudDetections.Count;
            for (var i = 0; i < count; i++)
            {
                var fraud = fraudDetections[i];
                fraud.DateOfCheck = startDate;
                fraud.Concurrence = Helper.ConcurrencePrepare(fraud);
                _session.Save(fraud);

                //for disabling lock db
                if (i % 20 != 0) continue;
                _session.Flush();
                _session.Clear();
            }
            if (count != 0)
            {
                //All other statuses will be set manually. See documentation
                var currentCustomer = _session.Get<Customer>(customerId); //object customer is CustomerProxy so we need customer from DB to update him
                currentCustomer.FraudStatus = FraudStatus.FraudSuspect;
                _session.Save(currentCustomer);
                _session.Flush();
            }
        }

    }

}