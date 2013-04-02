using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
   public class PacnetTransactiontRepository : NHibernateRepositoryBase<PacnetTransaction>
    {
       public PacnetTransactiontRepository(ISession session) : base(session)
       {
       }

       public IList<PacnetTransaction> GetByPacnetStatus(string pacnetStatus)
      {
          return GetAll().Where(x => x.PacnetStatus ==pacnetStatus  ).ToList();
      }
    }
}
