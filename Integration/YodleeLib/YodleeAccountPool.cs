﻿namespace YodleeLib
{
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using EzBob.CommonLib.Security;
	using StructureMap;
	using config;
	using log4net;

	public static class YodleeAccountPool
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(YodleeAccountPool));
		private static readonly YodleeAccountsRepository accountRepository = ObjectFactory.GetInstance<YodleeAccountsRepository>();
		private static readonly List<YodleeAccounts> accounts;
		private static readonly YodleeMain yodleeMain = new YodleeMain();
		private static readonly IYodleeMarketPlaceConfig config = ObjectFactory.GetInstance<IYodleeMarketPlaceConfig>();
		private static readonly object accountsLock = new object();
		
		static YodleeAccountPool()
		{
			accounts = accountRepository.SearchNotAllocated();

			while (accounts.Count < config.AccountPoolSize)
			{
				accounts.Add(CreateUnallocatedAccount());
			}
		}

		private static YodleeAccounts CreateUnallocatedAccount()
		{
			YodleeAccounts account = accountRepository.CreateAccount(YodleePasswordGenerator.GenerateRandomPassword);
			log.InfoFormat("Registering yodlee user: {0}", account.Username);
			yodleeMain.RegisterUser(account.Username, Encryptor.Decrypt(account.Password), account.Username);

			return account;
		}

		public static YodleeAccounts GetAccount(Customer customer, YodleeBanks bank)
		{
			lock (accountsLock)
			{
				YodleeAccounts res = accounts[0];
				accounts.RemoveAt(0);
				res.Customer = customer;
				res.Bank = bank;
				res.CreationDate = DateTime.UtcNow;
				accountRepository.SaveOrUpdate(res);
				log.InfoFormat("Allocated yodlee account: {0} to customer:{1}", res.Id, customer.Id);
				accounts.Add(CreateUnallocatedAccount());
				return res;
			}
		}
	}
}