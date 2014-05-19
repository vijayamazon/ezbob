namespace EzBob.Web.Areas.Customer.Models
{
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;

	public class SageAccountModel
	{
		public int id { get; set; }
		public string displayName { get; set; }

		public static SageAccountModel ToModel(IDatabaseCustomerMarketPlace account)
		{
			return new SageAccountModel
			{
				id = account.Id,
				displayName = account.DisplayName
			};
		}

		public static SageAccountModel ToModel(MP_CustomerMarketPlace account)
		{
			return new SageAccountModel
			{
				id = account.Id,
				displayName = account.DisplayName
			};
		} // ToModel
	}
}