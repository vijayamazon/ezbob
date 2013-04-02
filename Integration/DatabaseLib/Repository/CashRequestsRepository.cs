using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
    public interface ICashRequestsRepository : IRepository<CashRequest>
    {
    }

    public class CashRequestsRepository : NHibernateRepositoryBase<CashRequest>, ICashRequestsRepository
    {

        public CashRequestsRepository(ISession session)
            : base(session)
        {
        }
    }
}