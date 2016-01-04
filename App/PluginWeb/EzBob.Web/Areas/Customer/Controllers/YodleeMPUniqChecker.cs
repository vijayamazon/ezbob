namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using Code.MpUniq;
	using Ezbob.Utils.Serialization;
	using YodleeLib.connector;

	public class YodleeMpUniqChecker : MPUniqChecker {
		public YodleeMpUniqChecker(
			ICustomerMarketPlaceRepository customerMarketPlaceRepository,
			IMP_WhiteListRepository whiteList
		) : base(customerMarketPlaceRepository, whiteList) {
		} // constructor

		public override void Check(Guid marketplaceType, Customer customer, string token)
		{
			throw new NotImplementedException("YodleeMPUniqChecker does not support Check(market place, customer, token");
		} // Check

		public void Check(Guid marketplaceType, Customer customer, long csId)
		{
			if (this.WhiteList.IsMarketPlaceInWhiteList(marketplaceType, string.Format("{0}", csId)))
				return;

			var alreadyAdded = customer.CustomerMarketPlaces
				.Where(m =>
					m.Marketplace.InternalId == marketplaceType &&
					m.Customer.CustomerOrigin.CustomerOriginID == customer.CustomerOrigin.CustomerOriginID
				)
				.Select(m => Serialized.Deserialize<YodleeSecurityInfo>(m.SecurityData))
				.Any(s => s.CsId == csId);

			if (alreadyAdded)
				throw new MarketPlaceAddedByThisCustomerException();
		} // Check
	} // YodleeMPUniqChecker
} // namespace
