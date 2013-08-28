using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
        public interface ICaisFlagRepository : IRepository<CaisFlag>
        {
           List<CaisFlag> GetForStatusType();
        }

        public class CaisFlagRepository : NHibernateRepositoryBase<CaisFlag>, ICaisFlagRepository
        {
            public CaisFlagRepository(ISession session)
                : base(session)
            {
            }

            public List<CaisFlag> GetForStatusType()
            {
	            return GetAll().Where(f => f.FlagSetting != "T").ToList();
            }
        }
}
