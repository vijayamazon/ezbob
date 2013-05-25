namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using Database;
	using NHibernate;

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

	public interface IYodleeAccountsRepository : IRepository<YodleeAccounts>
    {
    }

	public class YodleeAccountsRepository : NHibernateRepositoryBase<YodleeAccounts>, IYodleeAccountsRepository
	{
		public YodleeAccountsRepository(ISession session)
			: base(session)
		{
		}

		public YodleeAccounts Search(int customerId)
		{
			return GetAll().FirstOrDefault(b => b.Customer.Id == customerId);
		}
	}
}