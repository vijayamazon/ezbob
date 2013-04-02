namespace EZBob.DatabaseLib.DatabaseWrapper
{
	class DatabaseCustomerMarketPlace : IDatabaseCustomerMarketPlace
	{
		public DatabaseCustomerMarketPlace( int id, string name, byte[] securityData, IDatabaseCustomer customer, IDatabaseMarketplace marketplace )
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
		public IDatabaseCustomer Customer { get; private set; }
		public IDatabaseMarketplace Marketplace { get; private set; }
	}
}