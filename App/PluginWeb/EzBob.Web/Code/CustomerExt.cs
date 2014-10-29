﻿namespace EzBob.Web.Code {
	using System;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Logger;
	using PostcodeAnywhere;

	public static class CustomerExt {
		#region method AddBankAccount

		public static int AddBankAccount(this Customer customer, string bankAccount, string sortCode, BankAccountType accountType, ISortCodeChecker sortCodeChecker = null) {
			if (customer == null) { // should never happen
				ms_oLog.Debug("Customer not specified for adding an account (#{0}, code {1}, type {2}).",
					bankAccount,
					sortCode,
					accountType
				);

				return -1;
			} // if

			if (customer.BankAccounts.Any(a => a.BankAccount == bankAccount && a.SortCode == sortCode)) {
				ms_oLog.Debug(
					"Bank account (#{1}, code {2}, type {3}) already exists at customer {0}.",
					customer.Stringify(),
					bankAccount,
					sortCode,
					accountType
				);

				return -2;
			} // if

			if (sortCodeChecker == null) {
				sortCodeChecker = CurrentValues.Instance.PostcodeAnywhereEnabled
					? (ISortCodeChecker)new SortCodeChecker(CurrentValues.Instance.PostcodeAnywhereMaxBankAccountValidationAttempts)
					: (ISortCodeChecker)new FakeSortCodeChecker();
			} // if

			var card = new CardInfo {
				BankAccount = bankAccount,
				SortCode = sortCode,
				Customer = customer,
				Type = accountType,
			};

			try {
				sortCodeChecker.Check(card);
			}
			catch (Exception e) {
				ms_oLog.Debug(
					e,
					"Just FYI: exception caught while checking a sort code for new bank account (#{1}, code {2}, type {3}) of customer {0}.",
					customer.Stringify(),
					bankAccount,
					sortCode,
					accountType
				);
			} // try

			customer.BankAccounts.Add(card);

			customer.SetDefaultCard(card);

			return card.Id;
		} // AddBankAccount

		#endregion method AddBankAccount

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof (CustomerExt));
	} // class CustomerExt
} // namespace
