namespace EZBob.DatabaseLib.DatabaseWrapper
{
	public interface IDatabaseCustomerMarketPlace
	{
		int Id { get; }
		string DisplayName { get; }
		byte[] SecurityData { get; }

		IDatabaseCustomer Customer {get; }
		IDatabaseMarketplace Marketplace { get; }
	}
}