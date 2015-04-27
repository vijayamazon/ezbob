using System.Linq;

namespace EzBob.Web.Areas.Customer.Controllers
{
	using CommonLib;
	using EZBob.DatabaseLib.Model.Database;
	using System;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using Code.MpUniq;
	using Ezbob.Utils.Serialization;
	using NHibernate;
	using YodleeLib.connector;

	public class YodleeMpUniqChecker : MPUniqChecker
	{

		public YodleeMpUniqChecker(ICustomerMarketPlaceRepository customerMarketPlaceRepository,
								   IMP_WhiteListRepository whiteList)
			: base(customerMarketPlaceRepository, whiteList)
		{
		} // constructor

		public override void Check(Guid marketplaceType, Customer customer, string token)
		{
			throw new NotImplementedException("YodleeMPUniqChecker does not support Check(market place, customer, token");
		} // Check

		public void Check(Guid marketplaceType, Customer customer, long csId)
		{
			if (_whiteList.IsMarketPlaceInWhiteList(marketplaceType, string.Format("{0}", csId)))
			{
				return;
			}

			var alreadyAdded = customer.CustomerMarketPlaces
				.Where(m => m.Marketplace.InternalId == marketplaceType)
				.Select(m => Serialized.Deserialize<YodleeSecurityInfo>(m.SecurityData))
				.Any(s => s.CsId == csId);

			if (alreadyAdded)
			{
				throw new MarketPlaceAddedByThisCustomerException();
			}
		}

	} // YodleeMPUniqChecker
} // namespace
