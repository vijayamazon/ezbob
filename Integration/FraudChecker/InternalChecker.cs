﻿using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Fraud;
using EzBob.Web.Code;
using log4net;
using NHibernate;
using NHibernate.Linq;
using StructureMap;
using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;

namespace FraudChecker
{


	public class InternalChecker
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(InternalChecker));
		private readonly ISession _session;

		public InternalChecker()
		{
			_session = ObjectFactory.GetInstance<ISession>();
		}

		public List<FraudDetection> InternalSystemDecision(int customerId)
		{
			Log.InfoFormat("Starting fraud internal system check for customerId={0}", customerId);
			var customer = _session.Get<Customer>(customerId);
			if (customer == null)
				throw new Exception("Customer not found");
			if (!customer.WizardStep.TheLastOne)
				throw new Exception(string.Format("Customer {0} not successfully  registered", customer.Id));

			var fraudDetections = new List<FraudDetection>();

			const int pageSize = 50;
			var customers = _session.QueryOver<Customer>().Where(x => !x.IsTest && x.Id != customerId);
			var customerCount = customers.RowCount();
			var iterationCount = customerCount / pageSize;

			for (var i = 0; i <= iterationCount; i++)
			{
				var customerPortion = customers.Skip(i * pageSize).Take(pageSize).List<Customer>();
				InternalFirstMiddleLastNameCheck(customer, customerPortion, fraudDetections);
				InternalFirstMiddleLastNameCheck(customer, customerPortion, fraudDetections, true);
				InternalLastNameDobCheck(fraudDetections, customerPortion, customer);
				InternalPhoneCheck(fraudDetections, customerPortion, customer);
				InternalPhoneFromMpCheck(fraudDetections,customerPortion, customer);
				InternalCompanyNameCheck(customer, fraudDetections, customerPortion);
				InternalBankAccountCheck(fraudDetections, customerPortion, customer);
				InternalIpCheck(fraudDetections, customerPortion, customer);
			}
			InternalDobLess21(customer, fraudDetections);

			//Director
			var directorCount = _session.QueryOver<Director>().RowCount();
			iterationCount = directorCount / pageSize;
			for (var i = 0; i <= iterationCount; i++)
			{
				var directorPortion = _session.QueryOver<Director>().Skip(i * pageSize).Take(pageSize).List<Director>();
				InternalDirectorFirstMiddleLastNameCheck(fraudDetections, directorPortion, customer);
				InternalDirectorFirstMiddleLastNameCheck(fraudDetections, directorPortion, customer, true);
			}

			InternalAddressCheck(customer, fraudDetections);
			InternalLastNamePostcodeCheck(fraudDetections, customer);
			InternalLastNameRawPostcodeCheck(fraudDetections, customer);
			InternalShopCheck(customer, fraudDetections);

			Log.InfoFormat("Finish fraud internal system check for customerId={0}", customerId);
			return fraudDetections;
		}

		//---------------------------------------------------------------------------------------------------

		private void InternalLastNameRawPostcodeCheck(List<FraudDetection> fraudDetections, Customer customer)
		{
			var customerAddresses = customer.AddressInfo.AllAddresses.ToList();
			var postcodes = customerAddresses.Select(a => a.Rawpostcode).ToList();

			fraudDetections.AddRange(
				from ca in _session.Query<CustomerAddress>().Where(address => postcodes.Contains(address.Rawpostcode))
				where ca.Customer.IsTest == false || ca.Director.Customer.IsTest == false
				where ca.Customer != customer
				where ca.Customer.WizardStep.TheLastOne
				where ca.Customer.PersonalInfo.Surname == customer.PersonalInfo.Surname
				select
					Helper.CreateDetection("Customer Last Name, Raw Postcode", customer, ca.Customer,
									"Customer Last Name, Raw Postcode",
									null, string.Format("{0}: {1}", ca.Postcode, customer.PersonalInfo.Surname)));
		}


		private void InternalPhoneFromMpCheck(ICollection<FraudDetection> fraudDetections,IEnumerable<Customer> customerPortion, Customer customer)
		{
			//check from customer marketplaces info
			var customerPhones = new Dictionary<string, string>();
			var customerMps = ObjectFactory.GetInstance<CustomerMarketPlaceRepository>().GetAll(customer);
			foreach (var mp in customerMps)
			{
				switch (mp.Marketplace.Name)
				{
					case "eBay":
						var ebayUserData = mp.EbayUserData.FirstOrDefault();
						if (ebayUserData != null)
						{
							if (ebayUserData.RegistrationAddress != null && !string.IsNullOrEmpty(ebayUserData.RegistrationAddress.Phone))
							{
								customerPhones.Add("Ebay phone" + ebayUserData.Id, ebayUserData.RegistrationAddress.Phone.Replace(" ", ""));
							}
								
							if (ebayUserData.RegistrationAddress != null && !string.IsNullOrEmpty(ebayUserData.RegistrationAddress.Phone2))
							{
								customerPhones.Add("Ebay phone 2" + ebayUserData.Id, ebayUserData.RegistrationAddress.Phone2.Replace(" ", ""));
							}
						}
						break;
					case "Pay Pal":
						if (mp.PersonalInfo != null && !string.IsNullOrEmpty(mp.PersonalInfo.Phone))
						{
							customerPhones.Add("PayPal phone" + mp.Id, mp.PersonalInfo.Phone.Replace(" ", ""));
						}
						break;
				}
			}
			var phonesArray = customerPhones.Values.Distinct().ToArray();

			var mpPhoneDetections = new Dictionary<Customer, List<MpPhone>>();
			foreach (var cd in customerPortion)
			{
				if(cd == customer) continue;
				foreach (var mp in cd.CustomerMarketPlaces)
				{
					if (mp.PersonalInfo != null && !string.IsNullOrEmpty(mp.PersonalInfo.Phone))
					{
						Helper.AddValue(mpPhoneDetections, cd, "Pay Pal", mp.PersonalInfo.Phone.Replace(" ", ""));
						
					}

					if (mp.EbayUserData != null)
					{
						foreach (var ebay in mp.EbayUserData)
						{
							if (ebay.RegistrationAddress != null)
							{
								if (!string.IsNullOrEmpty(ebay.RegistrationAddress.Phone))
								{
									Helper.AddValue(mpPhoneDetections, cd, "Ebay Phone1", ebay.RegistrationAddress.Phone.Replace(" ", ""));
								}

								if (!string.IsNullOrEmpty(ebay.RegistrationAddress.Phone2))
								{
									Helper.AddValue(mpPhoneDetections, cd, "Ebay Phone2", ebay.RegistrationAddress.Phone2.Replace(" ", ""));
								}
							}
						}
					}
					
				}
			}

			foreach (var kvp in mpPhoneDetections)
			{
				var cd = kvp.Key;
				List<MpPhone> lst = kvp.Value;
				foreach (var mpPhone in lst)
				{
					foreach (var customerPhone in customerPhones)
					{
						var phone = customerPhone.Value;
						if (mpPhone.Phone == phone)
						{
							fraudDetections.Add(Helper.CreateDetection(mpPhone.MpType, customer, cd,
							                                           customerPhone.Key, null, phone));
						}
					}	
				}
			}
		}

		private void InternalPhoneCheck(ICollection<FraudDetection> fraudDetections,
										IEnumerable<Customer> customerPortion,
										Customer customer)
		{
			var customerPhones = Helper.GetCustomerPhones(customer);

			//check from customer info
			foreach (var cd in customerPortion)
			{
				foreach (var customerPhone in customerPhones)
				{
					if (cd.PersonalInfo == null) continue;

					var phone = customerPhone.Value;
					if (cd.PersonalInfo.DaytimePhone == phone)
					{
						fraudDetections.Add(Helper.CreateDetection("Customer DaytimePhone", customer, cd, customerPhone.Key,
															null, phone));
					}
					if (cd.PersonalInfo.MobilePhone == phone)
					{
						fraudDetections.Add(Helper.CreateDetection("Customer MobilePhone", customer, cd, customerPhone.Key,
															null, phone));
					}
					switch (cd.PersonalInfo.TypeOfBusiness.Reduce())
					{
						case TypeOfBusinessReduced.Limited:
							if (cd.LimitedInfo.LimitedBusinessPhone == phone)
							{
								fraudDetections.Add(Helper.CreateDetection("Customer LimitedBusinessPhone", customer, cd,
																	customerPhone.Key, null, phone));
							}
							break;
						case TypeOfBusinessReduced.NonLimited:
							if (cd.NonLimitedInfo.NonLimitedBusinessPhone == phone)
							{
								fraudDetections.Add(Helper.CreateDetection("Customer NonLimitedBusinessPhone", customer, cd,
																	customerPhone.Key, null, phone));
							}
							break;
					}
				}
			}
		}

		private void InternalLastNamePostcodeCheck(List<FraudDetection> fraudDetections, Customer customer)
		{
			var customerAddresses = customer.AddressInfo.AllAddresses.ToList();
			var postcodes = customerAddresses.Select(a => a.Postcode).ToList();

			fraudDetections.AddRange(
				from ca in _session.Query<CustomerAddress>().Where(address => postcodes.Contains(address.Postcode))
				where ca.Customer.IsTest == false || ca.Director.Customer.IsTest == false
				where ca.Customer != customer
				where ca.Customer.WizardStep.TheLastOne
				where ca.Customer.PersonalInfo.Surname == customer.PersonalInfo.Surname
				select
					Helper.CreateDetection("Customer Last Name, Postcode", customer, ca.Customer,
									"Customer Last Name, Posstcode",
									null, string.Format("{0}: {1}", ca.Postcode, customer.PersonalInfo.Surname)));
		}

		private void InternalShopCheck(Customer customer, List<FraudDetection> fraudDetections)
		{
			//Shop ID
			const int pageSize = 500;
			var customerMps = _session.QueryOver<MP_CustomerMarketPlace>().Where(x => x.Customer == customer).List<MP_CustomerMarketPlace>();
			var mps = from cmp in _session.Query<MP_CustomerMarketPlace>()
					  where cmp.Customer != customer
					  where cmp.Customer.IsTest == false
					  where cmp.Marketplace.Name != "Yodlee"
					  where cmp.Marketplace.Name != "Sage"
					  select cmp;
			var mpCount = mps.Count();
			var iterationCount = mpCount / pageSize;

			for (var i = 0; i <= iterationCount; i++)
			{
				var mpPortion = mps.Skip(i * pageSize).Take(pageSize).ToList();

				fraudDetections.AddRange(
					from m in mpPortion
					from cm in customerMps
					where m.DisplayName == cm.DisplayName
					select
						Helper.CreateDetection("Customer Marketplace Name", customer, m.Customer, "Customer Marketplace Name",
										null, string.Format("{0}: {1}", m.Marketplace.Name, m.DisplayName)));
			}

			var yodlees = customerMps.Where(x => x.Marketplace.Name == "Yodlee").ToList();
			if (yodlees.Any())
			{
				InternalYodleeMpCheck(yodlees, customer, fraudDetections);
			}

			//TODO Sage Check (not using sage mp name (always the same))
		}

		private void InternalYodleeMpCheck(IEnumerable<MP_CustomerMarketPlace> customerYodlees, Customer customer, List<FraudDetection> fraudDetections)
		{
			var yodlees = _session.Query<MP_YodleeOrderItem>()
			                      .Where(
				                      i =>
				                      i.Order.CustomerMarketPlace.Customer != customer &&
				                      i.Order.CustomerMarketPlace.Customer.IsTest == false)
			                      .Select(i => new
				                      {
					                      Name = i.accountName,
					                      Number = i.accountNumber,
					                      CustomerId = i.Order.CustomerMarketPlace.Customer.Id
				                      })
			                      .ToList()
			                      .Distinct();
			                      

			var customerYodleesItems =
				customerYodlees.SelectMany(y => y.YodleeOrders)
							   .SelectMany(o => o.OrderItems)
							   .Select(i => new { Name = i.accountName, Number = i.accountNumber })
							   .Distinct()
							   .ToList();

			var mpCount = yodlees.Count();
			const int pageSize = 500;
			var iterationCount = mpCount / pageSize;

			for (var i = 0; i <= iterationCount; i++)
			{
				var mpPortion = yodlees.Skip(i * pageSize).Take(pageSize).ToList();

				fraudDetections.AddRange(
					from m in mpPortion
					from cm in customerYodleesItems
					where m.Name == cm.Name && m.Number == cm.Number
					select
						Helper.CreateDetection("Customer Bank Account Name And Number", customer,_session.Query<Customer>().FirstOrDefault(c => c.Id == m.CustomerId), "Customer Bank Account Name And Number",
										null, string.Format("{0}: {1}", m.Name, m.Number)));
			}
		}

		private IEnumerable<CustomerAddress> GetAddressesOfOtherCustomers(Customer customer)
		{
			var otherAddresses = _session.Query<CustomerAddress>().Where(x => (x.Customer.IsTest == false && x.Customer != customer) || (x.Director.Customer.IsTest == false && x.Director.Customer != customer)).ToList();
			return otherAddresses;
		}

		private void InternalAddressCheck(Customer customer, List<FraudDetection> fraudDetections)
		{
			//Address (any of home, business, directors, previous addresses)
			var customerAddresses = customer.AddressInfo.AllAddresses.ToList();
			var postcodes = customerAddresses.Select(a => a.Postcode).ToList();
			var addresses = GetAddressesOfOtherCustomers(customer).Where(address => postcodes.Contains(address.Postcode));

			fraudDetections.AddRange(
				from ca in customerAddresses
				from a in addresses
				where
					ca.Line1 == a.Line1 && ca.Line2 == a.Line2 && ca.Line3 == a.Line3 && ca.Town == a.Town &&
					ca.County == a.County
				select
					Helper.CreateDetection("Customer " + ca.AddressType.ToString(), customer, a.Customer ?? a.Director.Customer,
									"Customer " + a.AddressType.ToString(),
									null,
									string.Format("{0}, {1}, {2}, {3}, {4}",
												  a.Line1, a.Line2, a.Line3, a.Postcode, a.County))
				);
		}

		private static void InternalDirectorFirstMiddleLastNameCheck(List<FraudDetection> fraudDetections,
																	 IEnumerable<Director> directorPortion,
																	 Customer customer,
																	 bool isSkipLast = false)
		{
			// First + Middle + Last
			fraudDetections.AddRange(
				from d in directorPortion
				where d.Customer != null
				where d.Customer.Id != customer.Id
				where
					customer.PersonalInfo.FirstName == d.Name && customer.PersonalInfo.MiddleInitial == d.Middle
				where !isSkipLast && customer.PersonalInfo.Surname == d.Surname
				select
					Helper.CreateDetection("Customer First Name, Last Name, Middle Name", customer, d.Customer,
									"Director First Name, Last Name, Middle Name", null,
									string.Format("{0}, {1}, {2}", d.Name, d.Surname, d.Middle)));
		}

		private static void InternalDobLess21(Customer customer, List<FraudDetection> fraudDetections)
		{
			//Date of birth too young ( < 21 years)
			if ((DateTime.UtcNow - customer.PersonalInfo.DateOfBirth).Value.Days / 365.25 < 21)
			{
				fraudDetections.Add(Helper.CreateDetection("Customer.DateOfBirth < 21", customer, null, "", null,
													FormattingUtils.FormatDateTimeToString(
														customer.PersonalInfo.DateOfBirth)));
			}
		}

		private static void InternalBankAccountCheck(List<FraudDetection> fraudDetections,
													 IEnumerable<Customer> customerPortion,
													 Customer customer)
		{
			//Bank Account (sort + Bank Account)
			fraudDetections.AddRange(
				from c in customerPortion
				where c.WizardStep.TheLastOne
				where c.BankAccount != null
				where !string.IsNullOrEmpty(c.BankAccount.AccountNumber) && !string.IsNullOrEmpty(c.BankAccount.SortCode)
				where
					c.BankAccount.SortCode == customer.BankAccount.SortCode &&
					c.BankAccount.AccountNumber == customer.BankAccount.AccountNumber
				select
					Helper.CreateDetection("Customer SortCode, AccountNumber", customer, c,
									"Customer SortCode, AccountNumber",
									null,
									string.Format("{0}, {1}", c.BankAccount.SortCode, c.BankAccount.AccountNumber)));
		}

		private static void InternalCompanyNameCheck(Customer customer, List<FraudDetection> fraudDetections,
													 IEnumerable<Customer> customerPortion)
		{
			//Name of company
			var typeOfBussiness = customer.PersonalInfo.TypeOfBusiness.Reduce();
			var companyName = typeOfBussiness == TypeOfBusinessReduced.NonLimited
								  ? customer.NonLimitedInfo.NonLimitedCompanyName
								  : customer.LimitedInfo.LimitedCompanyName;
			if (string.IsNullOrEmpty(companyName)) return;
			fraudDetections.AddRange(
				from c in customerPortion
				where c.WizardStep.TheLastOne && c.PersonalInfo != null
				where
					((c.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited
						  ? c.LimitedInfo.LimitedCompanyName
						  : c.NonLimitedInfo.NonLimitedCompanyName) ?? "").ToLower() == companyName.ToLower()
				select
					Helper.CreateDetection("Customer CompanyName", customer, c,
									"Customer CompanyName",
									null,
									string.Format("{0}", companyName)));
		}

		private static void InternalLastNameDobCheck(List<FraudDetection> fraudDetections,
													 IEnumerable<Customer> customerPortion,
													 Customer customer)
		{
			//Last name + date of birth
			var lastName = customer.PersonalInfo.Surname;
			fraudDetections.AddRange(
				from c in customerPortion
				where c.WizardStep.TheLastOne
				where
					c.PersonalInfo != null &&
					c.PersonalInfo.Surname == lastName &&
					c.PersonalInfo.DateOfBirth == customer.PersonalInfo.DateOfBirth
				select
					Helper.CreateDetection("Customer Last Name, Date Of Birth", customer, c,
									"Customer Last Name, Date Of Birth",
									null,
									string.Format("{0}, {1}", c.PersonalInfo.Surname, c.PersonalInfo.DateOfBirth)));
		}

		private static void InternalFirstMiddleLastNameCheck(Customer customer, IEnumerable<Customer> customerPortion,
															 List<FraudDetection> fraudDetections,
															 bool isSkipLast = false)
		{
			var firstName = customer.PersonalInfo.FirstName;
			var lastName = customer.PersonalInfo.Surname;
			var middleName = customer.PersonalInfo.MiddleInitial;

			// First + Middle + Last
			fraudDetections.AddRange(
				from c in customerPortion
				where c.WizardStep.TheLastOne
				where
					c.PersonalInfo != null &&
					c.PersonalInfo.FirstName == firstName &&
					c.PersonalInfo.MiddleInitial == middleName
				where !isSkipLast && c.PersonalInfo.Surname == lastName
				select
					Helper.CreateDetection("Customer First Name, Last Name, Middle Name", customer, c,
									"Customer First Name, Last Name, Middle Name",
									null, string.Format("{0}, {1}, {2}", firstName, lastName, middleName)));
		}

		private void InternalIpCheck(List<FraudDetection> fraudDetections,
								IEnumerable<Customer> customerPortion,
								Customer customer)
		{

			var customerSession = customer.Session.FirstOrDefault();
			if (customerSession == null)
			{
				return;
			}

			fraudDetections.AddRange(
				from c in customerPortion
				where c.Session.Select(s => s.Ip).ToList().Contains(customerSession.Ip)
				select Helper.CreateDetection("Customer IP", customer, c, "Customer IP", null, string.Format("{0}", customerSession.Ip)));
		}
	}
}
