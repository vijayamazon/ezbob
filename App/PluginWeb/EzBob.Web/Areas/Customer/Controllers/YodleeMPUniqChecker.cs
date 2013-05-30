namespace EzBob.Web.Areas.Customer.Controllers
{
	using CommonLib;
	using EZBob.DatabaseLib.Model.Database;
	using System;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using Code.MpUniq;
	using NHibernate;
	using YodleeLib.connector;

	public class YodleeMpUniqChecker : MPUniqChecker
	{
		#region constructor

		public YodleeMpUniqChecker(ICustomerMarketPlaceRepository customerMarketPlaceRepository,
		                           IMP_WhiteListRepository whiteList)
			: base(customerMarketPlaceRepository, whiteList)
		{
		} // constructor

		#endregion constructor

		#region method Check

		public override void Check(Guid marketplaceType, Customer customer, string token)
		{
			throw new NotImplementedException("YodleeMPUniqChecker does not support Check(market place, customer, token");
		} // Check

		#endregion method Check

		#region method Check

		public override void Check(Guid marketplaceType, Customer customer, string login, string url)
		{
			throw new NotImplementedException("YodleeMPUniqChecker does not support Check(market place, customer, login, url");
		} // Check

		public void Check(Guid marketplaceType, Customer customer, long csId, ISession _session)
		{
			if (_whiteList.IsMarketPlaceInWhiteList(marketplaceType, string.Format("{0}", csId)))
			{
				return;
			}

			var queryOverMarketplaces = _session.QueryOver<MP_CustomerMarketPlace>();

			foreach (MP_CustomerMarketPlace mpCustomerMarketPlace in queryOverMarketplaces.List())
			{
				if (mpCustomerMarketPlace.Customer.Id == customer.Id && mpCustomerMarketPlace.Marketplace.InternalId == marketplaceType)
				{
					var securityInfo = SerializeDataHelper.DeserializeType<YodleeSecurityInfo>(mpCustomerMarketPlace.SecurityData);
					if (securityInfo.CsId == csId)
					{
						throw new MarketPlaceAddedByThisCustomerException();
					}
				}
			}
		}

		#endregion method Check
	} // YodleeMPUniqChecker
} // namespace
