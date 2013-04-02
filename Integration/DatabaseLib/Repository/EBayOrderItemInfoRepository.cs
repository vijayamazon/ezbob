using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public class EBayOrderItemInfoRepository : NHibernateRepositoryBase<MP_EBayOrderItemDetail>
	{
		public EBayOrderItemInfoRepository(ISession session) 
			: base(session)
		{
		}

		public MP_EBayOrderItemDetail FindItem( eBayFindOrderItemInfoData data )
		{
			return GetAll().FirstOrDefault( i => i.ItemID == data.ItemId );

		}
	}
}