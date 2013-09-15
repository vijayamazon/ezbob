namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using Database;
	using EzBob.CommonLib.Security;
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

		public List<YodleeAccounts> SearchNotAllocated()
		{
			return GetAll().OrderBy(b => b.Id).Where(b => b.Customer == null).ToList();
		}

		public YodleeAccounts CreateAccount(Func<string> generatePassword)
		{
			decimal maxId = (decimal)_session.CreateSQLQuery("SELECT IDENT_CURRENT('YodleeAccounts')").UniqueResult();
			var account = new YodleeAccounts
			{
				CreationDate = DateTime.UtcNow,
				Customer = null,
				Username = string.Format("EZBOB+{0}@ezbob.com", maxId + 1),
				Password = Encryptor.Encrypt(generatePassword()),
				Bank = null
			};

			SaveOrUpdate(account);
			return account;
		}
	}
}