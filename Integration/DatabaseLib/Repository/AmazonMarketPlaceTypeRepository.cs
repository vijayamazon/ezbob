namespace EZBob.DatabaseLib.Repository
{
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;
	using Model.Marketplaces.Amazon;

	public class AmazonMarketPlaceTypeRepository : NHibernateRepositoryBase<MP_AmazonMarketplaceType>
	{
		public AmazonMarketPlaceTypeRepository(ISession session)
			: base(session)
		{
		}

		public MP_AmazonMarketplaceType GetByMarketPlaceId( string id )
		{
			return GetAll().FirstOrDefault(a => a.MarketplaceId == id);
		}

	}
}