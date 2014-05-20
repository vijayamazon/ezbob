namespace EzBob.Web.Areas.Customer.Controllers {
	using Code.MpUniq;
	using EZBob.DatabaseLib.Model.Database;
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using CommonLib;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using log4net;

	public class CGMPUniqChecker : MPUniqChecker {
		#region constructor

		public CGMPUniqChecker(ICustomerMarketPlaceRepository customerMarketPlaceRepository, IMP_WhiteListRepository whiteList, MarketPlaceRepository mpTypes)
			: base(customerMarketPlaceRepository, whiteList) {
			_mpTypes = mpTypes;
		} // constructor

		#endregion constructor

		#region method Check

		public override void Check(Guid marketplaceType, Customer customer, string token) {
			if (_whiteList.IsMarketPlaceInWhiteList(marketplaceType, token))
				return;

			ILog oLog = LogManager.GetLogger(typeof(CGMPUniqChecker));

			var oMp = _mpTypes.Get(marketplaceType);

			if (oMp == null)
				return;

			var oMpList = _customerMarketPlaceRepository.GetAll()
				.Where(mp => mp.Marketplace.Id == oMp.Id)
				.Select(mp => new { mp_id = mp.Id, customer_id = mp.Customer.Id, secdata = mp.SecurityData });

			foreach (var m in oMpList) {
				if (!IsSameMarketPlace(m.mp_id, m.secdata, oMp, token, oLog))
					continue;

				if (m.customer_id == customer.Id)
					throw new MarketPlaceAddedByThisCustomerException();

				throw new MarketPlaceIsAlreadyAddedException();
			} // for each marketplace
		} // Check

		#endregion method Check

		#region method IsSameMarketPlace

		private bool IsSameMarketPlace(int nMpID, byte[] oSecData, MP_MarketplaceType oMp, string sShopID, ILog oLog) {
			VendorInfo vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(oMp.Name);

			if (vi == null)
				return false;

			try {
				var am = SerializeDataHelper.DeserializeType<AccountModel>(oSecData);
				return am.Fill().UniqueID() == sShopID;
			}
			catch (Exception e) {
				string sXml = System.Text.Encoding.Default.GetString(oSecData);
				string s = string.Format("Failed to deserialise security data. Marketplace ID = {0}, Security data: {1}", nMpID, sXml);
				oLog.Error(s, e);
				return false;
			}
		} // IsSameMarketPlace

		#endregion method IsSameMarketPlace

		private readonly MarketPlaceRepository _mpTypes;
	} // class CGMPUniqChecker
} // namespace
