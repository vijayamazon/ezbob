using EZBob.DatabaseLib.Model.Database;

namespace EZBob.DatabaseLib.DatabaseWrapper
{
	class DatabaseCustomerMarketPlace : IDatabaseCustomerMarketPlace
	{
		public DatabaseCustomerMarketPlace( int id, string name, byte[] securityData, Customer customer, IDatabaseMarketplace marketplace )
		{
			Id = id;
			DisplayName = name;
			SecurityData = securityData;
			Customer = customer;
			Marketplace = marketplace;
		}

		public int Id { get; private set; }
		public string DisplayName { get; private set; }

		public byte[] SecurityData { get; private set; }
		public Customer Customer { get; private set; }
		public IDatabaseMarketplace Marketplace { get; private set; }
	}
}