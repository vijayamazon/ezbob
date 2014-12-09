using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Fraud;
using EZBob.DatabaseLib.Repository;
using log4net;
using NHibernate;
using StructureMap;

namespace FraudChecker
{
	public class ExternalChecker
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ExternalChecker));
		private readonly List<FraudUser> _fu;
		private readonly ISession _session;

		public ExternalChecker()
		{
			_fu = (ObjectFactory.GetInstance<FraudUserRepository>()).GetAll().ToList();
			_session = ObjectFactory.GetInstance<ISession>();
		}

		public List<FraudDetection> ExternalSystemDecision(int customerId)
		{
			Log.InfoFormat("Starting fraud external system check for customerId={0}", customerId);
			var customer = _session.Load<Customer>(customerId);
			//if (!customer.WizardStep.TheLastOne)
			//	throw new Exception(string.Format("Customer {0} not successfully  registered", customer.Id));

			var fraudDetections = new List<FraudDetection>();

			ExternalFirstLastCheck(fraudDetections, customer);
			ExternalAddressCheck(customer, fraudDetections);
			ExternalBankAccountCheck(customer, fraudDetections);
			ExternalCompanyCheck(customer, fraudDetections);
			ExternalEmailCheck(customer, fraudDetections);
			ExternalEmailDomainCheck(customer, fraudDetections);
			ExternalPhoneCheck(customer, fraudDetections);
			ExternalShopCheck(fraudDetections, customer);

			Log.InfoFormat("Finish fraud internal system check for customerId={0}", customerId);
			return fraudDetections;
		}

		private void ExternalShopCheck(List<FraudDetection> fraudDetections, Customer customer)
		{
			//shops check
			fraudDetections.AddRange(from f in _fu
									 from d in f.Shops
									 from s in customer.CustomerMarketPlaces
									 where
										!string.IsNullOrEmpty(s.DisplayName)
									 where
										 d.Name == s.DisplayName && d.Type == s.Marketplace
									 select
										 Helper.CreateDetection("Customer ShopDisplayName, ShopMarketplaceType", customer, null,
														 "Fraud ShopDisplayName, ShopMarketplaceType", f,
														 string.Format("{0}, {1}", d.Name, d.Type.Name
															 )));
		}

		private void ExternalPhoneCheck(Customer customer, List<FraudDetection> fraudDetections)
		{
			var phone = string.Empty;
			var phoneType = string.Empty;
			if (customer.Company != null)
			{
				phone = customer.Company.BusinessPhone;
				phoneType = customer.Company.TypeOfBusiness.Reduce().ToString();
			}
			//phones check

			if (string.IsNullOrEmpty(phoneType)) return;
			fraudDetections.AddRange(_fu.SelectMany(f => f.Phones, (f, d) => new { f, d })
										.Where(@t => @t.d.PhoneNumber == customer.PersonalInfo.DaytimePhone ||
													 @t.d.PhoneNumber == customer.PersonalInfo.MobilePhone ||
													 (!string.IsNullOrEmpty(phone) && @t.d.PhoneNumber == phone))
										.Select(
											@t =>
											{
												if (@t.d.PhoneNumber == customer.PersonalInfo.DaytimePhone)
													phoneType = "DaytimePhone";
												if (@t.d.PhoneNumber == customer.PersonalInfo.MobilePhone)
													phoneType = "MobilePhone";
												return Helper.CreateDetection("Customer " + phoneType, customer,
																	   null, "Fraud PhoneNumber", @t.f,
																	   @t.d.PhoneNumber);
											}
										 ));
		}

		private void ExternalEmailDomainCheck(Customer customer, List<FraudDetection> fraudDetections)
		{
			//email domains check
			var customerEmailDomain =
				customer.Name.Substring(customer.Name.IndexOf("@", StringComparison.Ordinal) + 1);
			fraudDetections.AddRange(from f in _fu
									 from d in f.EmailDomains
									 where
										 d.EmailDomain == customerEmailDomain
									 select
										 Helper.CreateDetection("Customer Email", customer, null, "Fraud Email Domain", f,
														 d.EmailDomain));
		}

		private void ExternalEmailCheck(Customer customer, List<FraudDetection> fraudDetections)
		{
			//emails check
			var email = customer.Name;
			fraudDetections.AddRange(from f in _fu
									 from d in f.Emails
									 where
										 d.Email == email
									 select
										 Helper.CreateDetection("Customer Email", customer, null, "Fraud Email", f, email));
		}

		private void ExternalCompanyCheck(Customer customer, List<FraudDetection> fraudDetections)
		{
			//companys check
			var company = customer.Company;
			string companyName = null;
			if (company != null)
			{
				companyName = company.CompanyName;
			}

			if (string.IsNullOrEmpty(companyName)) return;

			var companyRegNum = company.ExperianRefNum ?? company.CompanyNumber;
			fraudDetections.AddRange(from f in _fu
									 from c in f.Companies
									 where
										!string.IsNullOrEmpty(c.CompanyName) && !string.IsNullOrEmpty(c.RegistrationNumber)
									 where
										 c.CompanyName == companyName && c.RegistrationNumber == companyRegNum
									 select
										 Helper.CreateDetection("Customer CompanyName, RegistrationNumber", customer, null,
														 "Fraud CompanyName, RegistrationNumber", f,
														 string.Format("{0}, {1}", companyName, companyRegNum)));
		}

		private void ExternalBankAccountCheck(Customer customer, List<FraudDetection> fraudDetections)
		{
			//bank accounts check
			var sortCode = customer.BankAccount.SortCode;
			var accountNumber = customer.BankAccount.AccountNumber;
			if (string.IsNullOrEmpty(accountNumber))
			{
				return;
			}
			fraudDetections.AddRange(from f in _fu
									 from d in f.BankAccounts
									 where
										!string.IsNullOrEmpty(d.SortCode) && !string.IsNullOrEmpty(d.BankAccount)
									 where
										 d.SortCode == sortCode && d.BankAccount == accountNumber
									 select
										 Helper.CreateDetection("Customer SortCode, AccountNumber", customer, null,
														 "Fraud SortCode, AccountNumber", f,
														 string.Format("{0}, {1}", sortCode, accountNumber)));
		}

		private void ExternalAddressCheck(Customer customer, List<FraudDetection> fraudDetections)
		{
			//addresses check
			var address = customer.AddressInfo.AllAddresses;
			fraudDetections.AddRange(from f in _fu
									 from d in f.Addresses
									 from add in address
									 where
										 d.Line1 == add.Line1 && d.Line2 == add.Line2 && d.Line3 == add.Line3 &&
										 d.Postcode == add.Postcode && d.County == add.County
									 select
										 Helper.CreateDetection("Customer " + add.AddressType.ToString(), customer, null,
														 "Fraud address",
														 f,
														 string.Format("{0}, {1}, {2}, {3}, {4}",
																	   d.Line1, d.Line2, d.Line3, d.Postcode, d.County)));
		}

		private void ExternalFirstLastCheck(List<FraudDetection> fraudDetections, Customer customer)
		{
			//First name + Last name check
			fraudDetections.AddRange(
				_fu.Where(
					x =>
					String.Equals(x.FirstName, customer.PersonalInfo.FirstName, StringComparison.CurrentCultureIgnoreCase) &&
					String.Equals(x.LastName, customer.PersonalInfo.Surname, StringComparison.CurrentCultureIgnoreCase))
				   .Select(
					   x =>
					   Helper.CreateDetection("Customer FirstName, LastName", customer, null, "Fraud FirstName, LastName", x,
									   string.Format("{0}, {1}", x.FirstName, x.LastName))));
		}

	}
}
