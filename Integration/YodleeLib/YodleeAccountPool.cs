namespace YodleeLib
{
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using Ezbob.Utils.Security;
	using StructureMap;
	using log4net;

	public static class YodleeAccountPool
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(YodleeAccountPool));
		private static readonly List<YodleeAccounts> accounts;
		private static readonly YodleeMain yodleeMain = new YodleeMain();
		private static readonly object accountsLock = new object();

		static YodleeAccountPool()
		{
			accounts = AccountRepository.SearchNotAllocated();

			while (accounts.Count < CurrentValues.Instance.YodleeAccountPoolSize)
			{
				accounts.Add(CreateUnallocatedAccount());
			}
		}

		public static YodleeAccountsRepository AccountRepository
		{
			get { return ObjectFactory.GetInstance<YodleeAccountsRepository>(); }
		}

		private static YodleeAccounts CreateUnallocatedAccount()
		{
			YodleeAccounts account = AccountRepository.CreateAccount(YodleePasswordGenerator.GenerateRandomPassword);
			log.InfoFormat("Registering yodlee user: {0}", account.Username);
			if (!yodleeMain.RegisterUser(account.Username, Encrypted.Decrypt(account.Password), account.Username))
			{
				AccountRepository.Delete(account);
			}

			return account;
		}

		public static YodleeAccounts GetAccount(Customer customer, YodleeBanks bank)
		{
			YodleeAccounts res = null;
			bool verifiedAccount = false;
			int attemptsCounter = 0;
			while (!verifiedAccount && attemptsCounter < 50)
			{
				attemptsCounter++;
				lock (accountsLock)
				{
					res = accounts[0];
					accounts.RemoveAt(0);
					res.Bank = bank;
					res.CreationDate = DateTime.UtcNow;
					AccountRepository.SaveOrUpdate(res);
					accounts.Add(CreateUnallocatedAccount());
				}
				log.InfoFormat("Trying to verify Yodlee account:{0}. Attempt number:{1} for customer:{2}", res.Username, attemptsCounter, customer.Id);
				verifiedAccount = VerifyAccount(res);
				if (verifiedAccount)
				{
					res.Customer = customer;
					log.InfoFormat("Allocated yodlee account: {0} to customer:{1}", res.Id, customer.Id);
					AccountRepository.SaveOrUpdate(res);
				}
				else
				{
					log.InfoFormat("Failed to allocate yodlee account: {0} to customer:{1}. The account will be deleted from our DB", res.Id, customer.Id);
					AccountRepository.Delete(res);
				}
			}
			return res;
		}

		private static bool VerifyAccount(YodleeAccounts res)
		{
			LoginUser lu = null;
			try
			{
				lu = yodleeMain.LoginUser(res.Username, Encrypted.Decrypt(res.Password));
			}
			catch (Exception e)
			{
				log.WarnFormat("Exception while trying to verify Yodlee account:{0}. The exception:{1}", res.Username, e);
			}

			if (lu == null)
			{
				log.ErrorFormat("Can't verify Yodlee account:{0}", res.Username);
				return false;
			}

			log.InfoFormat("Verified Yodlee account:{0} successfully", res.Username);
			return true;
		}
	}
}