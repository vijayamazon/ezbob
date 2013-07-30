using EZBob.DatabaseLib.Model.Database;

namespace EZBob.DatabaseLib.DatabaseWrapper
{
	public interface IDatabaseCustomerMarketPlace
	{
		int Id { get; }
		string DisplayName { get; }
		byte[] SecurityData { get; }

		Customer Customer {get; }
		IMarketplaceType Marketplace { get; }
        bool Disabled { get; }
	}
}