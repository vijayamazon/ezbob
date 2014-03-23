namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using NHibernate;

	public class EbayUserAddressDataRepository : NHibernateRepositoryBase<MP_EbayUserAddressData>
	{
		public EbayUserAddressDataRepository(ISession session)
			: base(session)
		{
		}
	}
}