namespace EzBob.Web.Areas.Customer.Models
{
	using CommonLib;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using YodleeLib.connector;

	public class YodleeAccountModel
	{
		public int bankId { get; set; }
		public string displayName { get; set; }

		public static YodleeAccountModel ToModel(IDatabaseCustomerMarketPlace marketplace, YodleeBanksRepository yodleeBanksRepository)
		{
			var securityInfo = SerializeDataHelper.DeserializeType<YodleeSecurityInfo>(marketplace.SecurityData);

			var yodleeBank = yodleeBanksRepository.Search(securityInfo.CsId);
			return new YodleeAccountModel
			{
				bankId = yodleeBank.Id,
				displayName = yodleeBank.Name
			};
		}

		public static YodleeAccountModel ToModel(MP_CustomerMarketPlace marketplace, YodleeBanksRepository yodleeBanksRepository)
		{
			var securityInfo = SerializeDataHelper.DeserializeType<YodleeSecurityInfo>(marketplace.SecurityData);

			var yodleeBank = yodleeBanksRepository.Search(securityInfo.CsId);
			return new YodleeAccountModel
			{
				bankId = yodleeBank.Id,
				displayName = yodleeBank.Name
			};
		}
	}
}