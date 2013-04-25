using System;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;

namespace EzBob.Web.Code.MpUniq
{
    public class MPUniqChecker : AMPUniqChecker
    {
        private readonly ICustomerMarketPlaceRepository _customerMarketPlaceRepository;
        private readonly IMP_WhiteListRepository _whiteList;

        public MPUniqChecker(ICustomerMarketPlaceRepository customerMarketPlaceRepository, IMP_WhiteListRepository whiteList)
        {
            _whiteList = whiteList;
            _customerMarketPlaceRepository = customerMarketPlaceRepository;
        }

        public override void Check(Guid marketplaceType, Customer customer, string token)
        {
            if (_whiteList.IsMarketPlaceInWhiteList(marketplaceType, token))
            {
                return;
            }
            if (_customerMarketPlaceRepository.Exists(marketplaceType, customer, token))
            {
                throw new MarketPlaceAddedByThisCustomerException();
            }
            if (_customerMarketPlaceRepository.Exists(marketplaceType, token))
            {
                throw new MarketPlaceIsAlreadyAddedException();
            }
        }
    }
}