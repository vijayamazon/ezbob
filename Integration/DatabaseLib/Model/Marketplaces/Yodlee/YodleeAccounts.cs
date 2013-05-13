namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using System;
	using Database;

	public class YodleeAccounts
	{
		public YodleeAccounts()
		{
			Customer = new Customer();
			Bank = new YodleeBanks();
        }

		public virtual int Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual YodleeBanks Bank { get; set; }

		public virtual string Username { get; set; }
		public virtual string Password { get; set; }
		public virtual DateTime? CreationDate { get; set; }
	}
}