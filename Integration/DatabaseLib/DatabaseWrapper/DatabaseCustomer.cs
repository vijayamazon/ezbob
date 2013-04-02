namespace EZBob.DatabaseLib.DatabaseWrapper
{
	public class DatabaseCustomer : IDatabaseCustomer
	{
		public DatabaseCustomer(string name)
		{
			Name = name;
		}

		public DatabaseCustomer(int id, string name)
		{
			Id = id;
			Name = name;
		}

		public int Id { get; set; }
		public string Name { get; set; }
	}
}