using System;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;

namespace EzBob.Web.Code.MpUniq
{
    public class MPUniqChecker : IMPUniqChecker
    {
		protected readonly ICustomerMarketPlaceRepository _customerMarketPlaceRepository;
        protected readonly IMP_WhiteListRepository _whiteList;

        public MPUniqChecker(ICustomerMarketPlaceRepository customerMarketPlaceRepository, IMP_WhiteListRepository whiteList)
        {
            _whiteList = whiteList;
            _customerMarketPlaceRepository = customerMarketPlaceRepository;
        }

        public virtual void Check(Guid marketplaceType, Customer customer, string token)
        {
            if (_whiteList.IsMarketPlaceInWhiteList(marketplaceType, token))
            {
                return;
            }
            if (_customerMarketPlaceRepository.Exists(marketplaceType, customer, token))
            {
                return;
                //throw new MarketPlaceAddedByThisCustomerException();
            }
            if (_customerMarketPlaceRepository.Exists(marketplaceType, token))
            {
                throw new MarketPlaceIsAlreadyAddedException();
            }
        }
    }
}