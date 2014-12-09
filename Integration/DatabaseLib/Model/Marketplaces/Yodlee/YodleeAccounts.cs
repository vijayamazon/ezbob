namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using ConfigManager;
	using Database;
	using NHibernate;
	using Ezbob.Utils.Security;

	public class YodleeAccounts {
		public YodleeAccounts() {
			Customer = new Customer();
			Bank = new YodleeBanks();
		} // constructor

		public virtual int Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual YodleeBanks Bank { get; set; }

		public virtual string Username { get; set; }
		public virtual string Password { get; set; }
		public virtual DateTime? CreationDate { get; set; }
	} // class YodleeAccounts

	public interface IYodleeAccountsRepository : IRepository<YodleeAccounts> {
		YodleeAccounts Search(int customerId);
	} // interface IYodleeAccountsRepository

	public class YodleeAccountsRepository : NHibernateRepositoryBase<YodleeAccounts>, IYodleeAccountsRepository {
		private readonly string accountPrefix;

		public YodleeAccountsRepository(ISession session) : base(session) {
			accountPrefix = CurrentValues.Instance.YodleeAccountPrefix;
		} // constructor

		public YodleeAccounts Search(int customerId) {
			return GetAll().FirstOrDefault(b => b.Customer.Id == customerId);
		} // Search

		public List<YodleeAccounts> SearchNotAllocated() {
			return GetAll().OrderBy(b => b.Id).Where(b => b.Customer == null).ToList();
		} // SearchNotAllocated

		public YodleeAccounts CreateAccount(Func<string> generatePassword) {
			decimal maxId = (decimal)_session.CreateSQLQuery("SELECT IDENT_CURRENT('YodleeAccounts')").UniqueResult();
			var account = new YodleeAccounts {
				CreationDate = DateTime.UtcNow,
				Customer = null,
				Username = string.Format("{0}+{1}@ezbob.com", accountPrefix, maxId + 1),
				Password = new Encrypted(generatePassword()),
				Bank = null
			};

			SaveOrUpdate(account);
			return account;
		} // CreateAccount
	} // class YodleeAccountsRepository 

} // namespace
