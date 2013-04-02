using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
        public interface ICaisFlagRepository : IRepository<CaisFlag>
        {
           List<CaisFlag> GetForStatusType(CollectionStatusType collectionStatusType);
        }

        public class CaisFlagRepository : NHibernateRepositoryBase<CaisFlag>, ICaisFlagRepository
        {
            public CaisFlagRepository(ISession session)
                : base(session)
            {
            }

            public List<CaisFlag> GetForStatusType(CollectionStatusType collectionStatusType)
            {
                return collectionStatusType == CollectionStatusType.Bankruptcy ? new List<CaisFlag> {GetAll().FirstOrDefault(f => f.FlagSetting == "T")} : GetAll().Where(f=>f.FlagSetting!="T").ToList();
            }
        }
}
