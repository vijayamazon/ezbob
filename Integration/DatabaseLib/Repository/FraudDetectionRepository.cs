namespace EZBob.DatabaseLib.Repository {
    using System.Collections.Generic;
    using System.Linq;
    using ApplicationMng.Repository;
    using EZBob.DatabaseLib.Model.Fraud;
    using NHibernate;
    using NHibernate.Linq;
    using System;

    public class FraudDetectionRepository : NHibernateRepositoryBase<FraudDetection> {
        public FraudDetectionRepository(ISession session)
            : base(session) {
        }

        public IEnumerable<FraudDetection> GetByCustomerId(int customerId) {
            return GetAll().Where(x => x.CurrentCustomer.Id == customerId);
        }

        public IEnumerable<FraudDetection> GetLastDetections(int customerId, out DateTime? lastDateCheck, out string customerRefNumber) {
            var lastCheck = Session
                .Query<FraudRequest>()
                .OrderByDescending(x => x.CheckDate)
                .FirstOrDefault(x => x.Customer.Id == customerId);

            if (lastCheck != null) {
                lastDateCheck = lastCheck.CheckDate;
                customerRefNumber = lastCheck.Customer.RefNumber;
                return lastCheck.FraudDetections.ToList();
            }

            lastDateCheck = null;
            customerRefNumber = null;
            return new List<FraudDetection>();

        }
    }
}
