namespace EzBob.Web.Areas.Customer.Models {
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using Ezbob.Utils.Serialization;
	using YodleeLib.connector;

	public class YodleeAccountModel {
		public int bankId { get; set; }
		public string displayName { get; set; }

		public static YodleeAccountModel ToModel(IDatabaseCustomerMarketPlace marketplace, YodleeBanksRepository yodleeBanksRepository) {
			var securityInfo = Serialized.Deserialize<YodleeSecurityInfo>(marketplace.SecurityData);

			var yodleeBank = yodleeBanksRepository.Search(securityInfo.CsId);
			return new YodleeAccountModel {
				bankId = yodleeBank.Id,
				displayName = yodleeBank.Name
			};
		} // ToModel

		public static YodleeAccountModel ToModel(MP_CustomerMarketPlace marketplace, YodleeBanksRepository yodleeBanksRepository = null) {
			var securityInfo = Serialized.Deserialize<YodleeSecurityInfo>(marketplace.SecurityData);

			var yodleeBank = yodleeBanksRepository != null ? yodleeBanksRepository.Search(securityInfo.CsId) : null;
			return new YodleeAccountModel {
				bankId = yodleeBank != null ? yodleeBank.Id : 0,
				displayName = yodleeBank != null ? yodleeBank.Name : "Uploaded bank"
			};
		} // ToModel
	} // class YodleeAccountModel
} // namespace
