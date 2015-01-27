using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database
{
    public interface IPacnetPaypointServiceLogRepository : IRepository<PacnetPaypointServiceLog>
    {
        void Log(int customerId, DateTime? insertDate, string requestType, string status, string errorMessage);
    }

    public class PacnetPaypointServiceLogRepository : NHibernateRepositoryBase<PacnetPaypointServiceLog>, IPacnetPaypointServiceLogRepository
    {
        public PacnetPaypointServiceLogRepository(ISession session) : base(session)
        {
        }

        public void Log(int customerId, DateTime? insertDate, string requestType, string status, string errorMessage)
        {
            Save(new PacnetPaypointServiceLog
                {
                    Customer = Session.Load<Customer>(customerId),
                    InsertDate = insertDate,
                    RequestType = requestType,
                    Status = status,
                    ErrorMessage = errorMessage
                });
        }

        public IEnumerable<PacnetPaypointServiceLog> GetByCustomer(Customer customer)
        {
            return GetAll().Where(x => x.Customer.Id == customer.Id).ToFuture();
        }
    }
}