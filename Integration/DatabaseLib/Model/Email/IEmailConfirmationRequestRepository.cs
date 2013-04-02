using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;

namespace EZBob.DatabaseLib.Model.Email
{
    public interface IEmailConfirmationRequestRepository : IRepository<EmailConfirmationRequest>
    {
        IQueryable<EmailConfirmationRequest> ByCustomer(Customer customer);
        IQueryable<EmailConfirmationRequest> PendingByCustomer(Customer customer);
    }
}