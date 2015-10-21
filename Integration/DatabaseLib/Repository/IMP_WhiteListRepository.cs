using System;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
	public interface IMP_WhiteListRepository : IRepository<MP_WhiteList>
    {
        bool IsMarketPlaceInWhiteList(Guid marketPlaceType, string name);
    }

	public class MP_WhiteListRepository : NHibernateRepositoryBase<MP_WhiteList>, IMP_WhiteListRepository
    {
        public MP_WhiteListRepository(ISession session) : base(session)
        {
        }

        public bool IsMarketPlaceInWhiteList(Guid marketPlaceType, string name)
        {
            return GetAll().Any(m => m.MarketplaceType == marketPlaceType && m.Name == name);
        }
    }
}