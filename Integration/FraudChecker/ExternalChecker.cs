namespace FraudChecker {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Fraud;
	using EZBob.DatabaseLib.Repository;
	using NHibernate;
	using StructureMap;

	public class ExternalChecker {
		public ExternalChecker(int customerID) {
			this.fraudUsers = (ObjectFactory.GetInstance<FraudUserRepository>()).GetAll().ToList();
			this.session = ObjectFactory.GetInstance<ISession>();
			this.fraudDetections = new List<FraudDetection>();
			this.customerID = customerID;
		} // constructor

		public List<FraudDetection> Decide() {
			log.Info("Starting fraud external system check for customerId {0}...", customerID);

			this.customer = this.session.Load<Customer>(this.customerID);

			FirstLast();
			Address();
			BankAccount();
			Company();
			Email();
			EmailDomain();
			Phone();
			Shop();

			log.Info("Finish fraud external system check for customerId {0}.", customerID);
			return this.fraudDetections;
		} // Decide

		private void Shop() {
			this.fraudDetections.AddRange(
				from f in this.fraudUsers
				from d in f.Shops
				from s in this.customer.CustomerMarketPlaces
				where !string.IsNullOrEmpty(s.DisplayName)
				where d.Name == s.DisplayName && d.Type == s.Marketplace
				select Helper.CreateDetection(
					"Customer ShopDisplayName, ShopMarketplaceType", this.customer,
					null,
					"Fraud ShopDisplayName, ShopMarketplaceType",
					f,
					string.Format("{0}, {1}", d.Name, d.Type.Name)
				)
			);
		} // Shop

		private void Phone() {
			string phone = string.Empty;
			string phoneType = string.Empty;

			if (this.customer.Company != null) {
				phone = this.customer.Company.BusinessPhone;
				phoneType = this.customer.Company.TypeOfBusiness.Reduce().ToString();
			} // if

			if (string.IsNullOrEmpty(phoneType))
				return;

			this.fraudDetections.AddRange(
				this.fraudUsers
				.SelectMany(f => f.Phones, (f, d) => new { f, d })
				.Where(@t =>
					@t.d.PhoneNumber == this.customer.PersonalInfo.DaytimePhone ||
					@t.d.PhoneNumber == this.customer.PersonalInfo.MobilePhone ||
					(!string.IsNullOrEmpty(phone) && @t.d.PhoneNumber == phone)
				)
				.Select(@t => {
					if (@t.d.PhoneNumber == this.customer.PersonalInfo.DaytimePhone)
						phoneType = "DaytimePhone";

					if (@t.d.PhoneNumber == this.customer.PersonalInfo.MobilePhone)
						phoneType = "MobilePhone";

					return Helper.CreateDetection(
						"Customer " + phoneType, this.customer,
						null,
						"Fraud PhoneNumber",
						@t.f,
						@t.d.PhoneNumber
					);
				})
			);
		} // Phone

		private void EmailDomain() {
			string customerEmailDomain = this.customer.Name.Substring(
				this.customer.Name.IndexOf("@", StringComparison.Ordinal) + 1
			);

			this.fraudDetections.AddRange(
				from f in this.fraudUsers
				from d in f.EmailDomains
				where d.EmailDomain == customerEmailDomain
				select Helper.CreateDetection(
					"Customer Email", this.customer,
					null,
					"Fraud Email Domain",
					f,
					d.EmailDomain
				)
			);
		} // EmailDomain

		private void Email() {
			string email = this.customer.Name;

			this.fraudDetections.AddRange(
				from f in this.fraudUsers
				from d in f.Emails
				where d.Email == email
				select Helper.CreateDetection("Customer Email", this.customer, null, "Fraud Email", f, email)
			);
		} // Email

		private void Company() {
			Company company = this.customer.Company;

			if (company == null)
				return;

			string companyName = company.CompanyName;

			if (string.IsNullOrEmpty(companyName))
				return;

			string companyRegNum = company.ExperianRefNum ?? company.CompanyNumber;

			this.fraudDetections.AddRange(
				from f in this.fraudUsers
				from c in f.Companies
				where !string.IsNullOrEmpty(c.CompanyName) && !string.IsNullOrEmpty(c.RegistrationNumber)
				where c.CompanyName == companyName && c.RegistrationNumber == companyRegNum
				select Helper.CreateDetection(
					"Customer CompanyName, RegistrationNumber", this.customer,
					null,
					"Fraud CompanyName, RegistrationNumber",
					f,
					string.Format("{0}, {1}", companyName, companyRegNum)
				)
			);
		} // Company

		private void BankAccount() {
			string accountNumber = this.customer.BankAccount.AccountNumber;

			if (string.IsNullOrEmpty(accountNumber))
				return;

			string sortCode = this.customer.BankAccount.SortCode;

			this.fraudDetections.AddRange(
				from f in this.fraudUsers
				from d in f.BankAccounts
				where !string.IsNullOrEmpty(d.SortCode) && !string.IsNullOrEmpty(d.BankAccount)
				where d.SortCode == sortCode && d.BankAccount == accountNumber
				select Helper.CreateDetection(
					"Customer SortCode, AccountNumber", this.customer,
					null,
					"Fraud SortCode, AccountNumber",
					f,
					string.Format("{0}, {1}", sortCode, accountNumber)
				)
			);
		} // BankAccount

		private void Address() {
			IEnumerable<CustomerAddress> address = this.customer.AddressInfo.AllAddresses;

			this.fraudDetections.AddRange(
				from f in this.fraudUsers
				from d in f.Addresses
				from add in address
				where (
					d.Line1 == add.Line1 &&
					d.Line2 == add.Line2 &&
					d.Line3 == add.Line3 &&
					d.Postcode == add.Postcode &&
					d.County == add.County
				)
				select Helper.CreateDetection(
					"Customer " + add.AddressType, this.customer,
					null,
					"Fraud address",
					f,
					string.Format("{0}, {1}, {2}, {3}, {4}", d.Line1, d.Line2, d.Line3, d.Postcode, d.County)
				)
			);
		} // Address

		private void FirstLast() {
			if (this.customer.PersonalInfo == null)
				return;

			string firstName = this.customer.PersonalInfo.FirstName;
			string lastName = this.customer.PersonalInfo.Surname;

			this.fraudDetections.AddRange(
				this.fraudUsers.Where(x =>
					String.Equals(x.FirstName, firstName, StringComparison.CurrentCultureIgnoreCase) &&
					String.Equals(x.LastName, lastName, StringComparison.CurrentCultureIgnoreCase)
				)
				.Select(x => Helper.CreateDetection(
					"Customer FirstName, LastName", this.customer,
					null,
					"Fraud FirstName, LastName",
					x,
					string.Format("{0}, {1}", x.FirstName, x.LastName)
				))
			);
		} // FirstLast

		private readonly List<FraudUser> fraudUsers;
		private readonly ISession session;
		private readonly List<FraudDetection> fraudDetections;
		private readonly int customerID;

		private Customer customer;

		private static readonly ASafeLog log = new SafeILog(typeof(ExternalChecker));
	} // class ExternalChecker
} // namespace
