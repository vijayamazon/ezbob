using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.CommonLib;
using Integration.ChannelGrabberConfig;
using Integration.ChannelGrabberFrontend;
using log4net;

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

			ILog oLog = LogManager.GetLogger(typeof(CGMPUniqChecker));

			foreach (MP_CustomerMarketPlace m in _customerMarketPlaceRepository.GetAll()) {
				if (!IsSameMarketPlace(m, token, oLog))
					continue;

				if (m.Customer.Id == customer.Id)
					throw new MarketPlaceAddedByThisCustomerException();

				throw new MarketPlaceIsAlreadyAddedException();
			} // for each marketplace
		} // Check

		#endregion method Check

		#region method IsSameMarketPlace

		private bool IsSameMarketPlace(MP_CustomerMarketPlace m, string sShopID, ILog oLog) {
			VendorInfo vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(m.Marketplace.Name);

			if (vi == null)
				return false;

			try {
				var am = SerializeDataHelper.DeserializeType<AccountModel>(m.SecurityData);
				return am.Fill().UniqueID() == sShopID;
			}
			catch (Exception e) {
				string sXml = System.Text.Encoding.Default.GetString(m.SecurityData);
				string s = string.Format("Failed to deserialise security data. Marketplace ID = {0}, Security data: {1}", m.Id, sXml);
				oLog.Error(s, e);
				return false;
			}
		} // IsSameMarketPlace

		#endregion method IsSameMarketPlace
	} // class CGMPUniqChecker
} // namespace
