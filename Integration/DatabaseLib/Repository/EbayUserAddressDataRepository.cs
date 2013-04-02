using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public class EbayUserAddressDataRepository : NHibernateRepositoryBase<MP_EbayUserAddressData>
	{
		public EbayUserAddressDataRepository(ISession session)
			: base(session)
		{
		}
	}
}