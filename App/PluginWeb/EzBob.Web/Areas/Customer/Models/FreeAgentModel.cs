namespace EzBob.Web.Areas.Customer.Models
{
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;

	public class FreeAgentAccountModel
	{
		public int id { get; set; }
		public string displayName { get; set; }

		public static FreeAgentAccountModel ToModel(IDatabaseCustomerMarketPlace account)
		{
			return new FreeAgentAccountModel
			{
				id = account.Id,
				displayName = account.DisplayName
			};
		}

		public static FreeAgentAccountModel ToModel(MP_CustomerMarketPlace account)
		{
			return new FreeAgentAccountModel
			{
				id = account.Id,
				displayName = account.DisplayName
			};
		} // ToModel
	}
}