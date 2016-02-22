namespace EzBob.Web.Code.MpUniq {
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;

	public class MPUniqChecker : IMPUniqChecker {
		public MPUniqChecker(
			ICustomerMarketPlaceRepository customerMarketPlaceRepository,
			IMP_WhiteListRepository whiteList
		) {
			this.WhiteList = whiteList;
			this.CustomerMarketPlaceRepository = customerMarketPlaceRepository;
		} // constructor

		public virtual void Check(Guid marketplaceType, Customer customer, string token) {
			if (this.WhiteList.IsMarketPlaceInWhiteList(marketplaceType, token))
				return;

			if (this.CustomerMarketPlaceRepository.Exists(marketplaceType, customer, token))
				return;
				//throw new MarketPlaceAddedByThisCustomerException();

			if (this.CustomerMarketPlaceRepository.Exists(marketplaceType, customer.CustomerOrigin.CustomerOriginID, token))
				throw new MarketPlaceIsAlreadyAddedException();
		} // Check

		protected readonly ICustomerMarketPlaceRepository CustomerMarketPlaceRepository;
		protected readonly IMP_WhiteListRepository WhiteList;
	} // class MPUniqChecker
} // namespace
