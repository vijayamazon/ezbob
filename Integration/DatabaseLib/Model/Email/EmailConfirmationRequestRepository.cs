using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Email
{
    public class EmailConfirmationRequestRepository : NHibernateRepositoryBase<EmailConfirmationRequest>, IEmailConfirmationRequestRepository
    {
        public EmailConfirmationRequestRepository(ISession session) : base(session)
        {
        }

        public virtual IQueryable<EmailConfirmationRequest> ByCustomer(Customer customer)
        {
            return GetAll().Where(r => r.Customer.Id == customer.Id);
        }

        public virtual IQueryable<EmailConfirmationRequest> PendingByCustomer(Customer customer)
        {
            return ByCustomer(customer).Where(r => r.State == EmailConfirmationRequestState.Pending);
        }
    }
}