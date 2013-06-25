using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.CommonLib;
using Integration.ChannelGrabberConfig;
using Integration.ChannelGrabberFrontend;

namespace EzBob.Web.Areas.Customer.Controllers {
	using Code.MpUniq;
	using EZBob.DatabaseLib.Model.Database;

	public class CGMPUniqChecker : MPUniqChecker {
		#region constructor

		public CGMPUniqChecker(ICustomerMarketPlaceRepository customerMarketPlaceRepository, IMP_WhiteListRepository whiteList)
			: base(customerMarketPlaceRepository, whiteList) {
		} // constructor

		#endregion constructor

		#region method Check

		public override void Check(Guid marketplaceType, Customer customer, string token) {
			if (_whiteList.IsMarketPlaceInWhiteList(marketplaceType, token))
				return;

			foreach (MP_CustomerMarketPlace m in _customerMarketPlaceRepository.GetAll()) {
				if (!IsSameMarketPlace(m, token))
					continue;

				if (m.Customer.Id == customer.Id)
					throw new MarketPlaceAddedByThisCustomerException();

				throw new MarketPlaceIsAlreadyAddedException();
			} // for each marketplace
		} // Check

		#endregion method Check

		#region method IsSameMarketPlace

		private bool IsSameMarketPlace(MP_CustomerMarketPlace m, string sShopID) {
			VendorInfo vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(m.Marketplace.Name);

			if (vi == null)
				return false;

			var am = SerializeDataHelper.DeserializeType<AccountModel>(m.SecurityData);

			return am.Fill().UniqueID() == sShopID;
		} // IsSameMarketPlace

		#endregion method IsSameMarketPlace
	} // class CGMPUniqChecker
} // namespace
