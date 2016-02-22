namespace EzBob.Web.Areas.Customer.Controllers {
	using Code.MpUniq;
	using EZBob.DatabaseLib.Model.Database;
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using Ezbob.Logger;
	using Integration.ChannelGrabberConfig;
	using Integration.ChannelGrabberFrontend;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;

	public class CGMPUniqChecker : MPUniqChecker {
		public CGMPUniqChecker(
			ICustomerMarketPlaceRepository customerMarketPlaceRepository,
			IMP_WhiteListRepository whiteList,
			MarketPlaceRepository mpTypes
		) : base(customerMarketPlaceRepository, whiteList) {
			this.mpTypes = mpTypes;
		} // constructor

		public override void Check(Guid marketplaceType, Customer customer, string token) {
			if (this.WhiteList.IsMarketPlaceInWhiteList(marketplaceType, token))
				return;

			var oMp = this.mpTypes.Get(marketplaceType);

			if (oMp == null)
				return;

			if (marketplaceType == Configuration.Instance.Hmrc.Guid()) {
				CheckHmrc(oMp.Id, customer, token);
				return;
			} // if

			var oMpList = this.CustomerMarketPlaceRepository.GetAll()
				.Where(mp =>
					mp.Marketplace.Id == oMp.Id &&
					mp.Customer.CustomerOrigin.CustomerOriginID == customer.CustomerOrigin.CustomerOriginID
				)
				.Select(mp => new { mp_id = mp.Id, customer_id = mp.Customer.Id, secdata = mp.SecurityData });

			foreach (var m in oMpList) {
				if (!IsSameMarketPlace(m.mp_id, m.secdata, oMp, token))
					continue;

				if (m.customer_id == customer.Id)
					throw new MarketPlaceAddedByThisCustomerException();

				throw new MarketPlaceIsAlreadyAddedException();
			} // for each marketplace
		} // Check

		private void CheckHmrc(int hmrcID, Customer customer, string token) {
			MP_CustomerMarketPlace existing = this.CustomerMarketPlaceRepository
				.GetAll()
				.FirstOrDefault(mp =>
					mp.Marketplace.Id == hmrcID &&
					mp.Customer.CustomerOrigin.CustomerOriginID == customer.CustomerOrigin.CustomerOriginID &&
					mp.DisplayName == token
				);

			if (existing == null)
				return;

			if (existing.Customer.Id == customer.Id)
				throw new MarketPlaceAddedByThisCustomerException();

			throw new MarketPlaceIsAlreadyAddedException();
		} // CheckHmrc

		private bool IsSameMarketPlace(int nMpID, byte[] oSecData, MP_MarketplaceType oMp, string sShopID) {
			VendorInfo vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(oMp.Name);

			if (vi == null)
				return false;

			try {
				var am = Serialized.Deserialize<AccountModel>(Encrypted.Decrypt(oSecData));
				return am.Fill().UniqueID() == sShopID;
			} catch (Exception e) {
				string sXml = System.Text.Encoding.Default.GetString(oSecData);

				new SafeILog(this).Warn(
					e,
					"Failed to de-serialize security data. Marketplace ID = {0}, Security data: {1}",
					nMpID,
					sXml
				);

				return false;
			} // try
		} // IsSameMarketPlace

		private readonly MarketPlaceRepository mpTypes;
	} // class CGMPUniqChecker
} // namespace
