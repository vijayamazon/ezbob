using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database
{
    using EZBob.DatabaseLib.Model.Database.UserManagement;

    public interface IPacnetPaypointServiceLogRepository : IRepository<PacnetPaypointServiceLog>
    {
        void Log(int customerId, DateTime? insertDate, string requestType, string status, string errorMessage);
    }

    public class PacnetPaypointServiceLogRepository : NHibernateRepositoryBase<PacnetPaypointServiceLog>, IPacnetPaypointServiceLogRepository
    {
        public PacnetPaypointServiceLogRepository(ISession session) : base(session)
        {
        }

        public void Log(int userId, DateTime? insertDate, string requestType, string status, string errorMessage)
        {
            Save(new PacnetPaypointServiceLog
                {
                    User = Session.Load<User>(userId),
                    InsertDate = insertDate,
                    RequestType = requestType,
                    Status = status,
                    ErrorMessage = errorMessage
                });
        }

        public IEnumerable<PacnetPaypointServiceLog> GetByCustomerId(int customerId)
        {
            return GetAll().Where(x => x.User.Id == customerId).ToFuture();
        }
    }
}