namespace FraudChecker
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Fraud;
	using Ezbob.Backend.Models;
	using log4net;
	using NHibernate;
	using NHibernate.Linq;
	using StructureMap;
	using System.Text.RegularExpressions;
	using Ezbob.Utils.Extensions;
	using NHibernate.Criterion;
	using NHibernate.Transform;

	public class InternalChecker
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(InternalChecker));
		private readonly ISession _session;

		public InternalChecker()
		{
			this._session = ObjectFactory.GetInstance<ISession>();
		}

		public List<FraudDetection> InternalSystemDecision(int customerId, FraudMode mode)
		{
			Log.InfoFormat("Starting fraud internal system check for customerId={0}", customerId);
			var customer = this._session.Get<Customer>(customerId);
			if (customer == null)
				throw new Exception("Customer not found");

			var fraudDetections = new List<FraudDetection>();

			int[] customerIds = this._session.CreateSQLQuery("EXEC FraudGetDetections " + customerId).List<int>().ToArray();
			Log.DebugFormat("Num of potential fraud customer ids: {0}", customerIds.Count());
			
			//fixing exception of too many input parameters if too many potential fraud detections returned
			if (customerIds.Count() > 2000) {
				customerIds = customerIds.Take(2000).ToArray();
			}

			var customers = this._session.QueryOver<Customer>().Where(x => x.Id.IsIn(customerIds)).List<Customer>();
			if (customers.Any())
			{
				Log.DebugFormat("Num of potential fraud customers: {0}", customers.Count());

				switch (mode)
				{
					case FraudMode.PersonalDetaisCheck:
						PersonalCheck(customer, customers, fraudDetections);
						break;
					case FraudMode.CompanyDetailsCheck:
						CompanyCheck(customer, customers, fraudDetections);
						break;
					case FraudMode.MarketplacesCheck:
						MpCheck(customer, customers, fraudDetections);
						break;
					case FraudMode.FullCheck:
						PersonalCheck(customer, customers, fraudDetections);
						CompanyCheck(customer, customers, fraudDetections);
						MpCheck(customer, customers, fraudDetections);
						InternalBankAccountCheck(fraudDetections, customers, customer);
				        
						break;
				}
			}
			else
			{
				Log.DebugFormat("None of the customers match any of parameters");
			}

			Log.InfoFormat("Finish fraud internal system check for customerId={0}", customerId);
			return fraudDetections;
		}


	    private void PersonalCheck(Customer customer, IList<Customer> customers, List<FraudDetection> fraudDetections)
		{
			InternalFirstMiddleLastNameCheck(customer, customers, fraudDetections);
			InternalFirstMiddleLastNameCheck(customer, customers, fraudDetections, true);
			InternalLastNameDobCheck(fraudDetections, customers, customer);
			InternalPhoneCheck(fraudDetections, customers, customer);
			InternalAddressCheck(customer, fraudDetections, customers);
			InternalLastNamePostcodeCheck(fraudDetections, customer, customers);
			InternalLastNameRawPostcodeCheck(fraudDetections, customer, customers);
			InternalIpCheck(fraudDetections, customers, customer);
			InternalDobLess21(customer, fraudDetections);
            IovationCheck(fraudDetections, customer);
		}

		private void CompanyCheck(Customer customer, IList<Customer> customers, List<FraudDetection> fraudDetections)
		{
			InternalCompanyCheck(customer, fraudDetections, customers);
			InternalDirectorFirstMiddleLastNameCheck(fraudDetections, customers, customer);
			InternalDirectorFirstMiddleLastNameCheck(fraudDetections, customers, customer, true);
		}

		private void MpCheck(Customer customer, IList<Customer> customers, List<FraudDetection> fraudDetections)
		{
			InternalShopCheck(customer, fraudDetections, customers);
			InternalPhoneFromMpCheck(fraudDetections, customers, customer);
		}

		//---------------------------------------------------------------------------------------------------

		private void InternalLastNameRawPostcodeCheck(List<FraudDetection> fraudDetections, Customer customer, IEnumerable<Customer> customers)
		{
			var customerAddresses = customer.AddressInfo.AllAddresses.ToList();
			var postcodes = customerAddresses.Select(a => a.Rawpostcode).Where(rpc => !string.IsNullOrEmpty(rpc)).ToList();

			fraudDetections.AddRange(
				from ca in customers.SelectMany(c => c.AddressInfo.AllAddresses).Where(address => postcodes.Contains(address.Rawpostcode))
				where ca.Customer != customer
				where ca.Customer.WizardStep.TheLastOne
				where ca.Customer.PersonalInfo.Surname == customer.PersonalInfo.Surname
				select
					Helper.CreateDetection("Customer Last Name, Raw Postcode", customer, ca.Customer,
									"Customer Last Name, Raw Postcode",
									null, string.Format("{0}: {1}", ca.Rawpostcode, customer.PersonalInfo.Surname)));
		}

		private void InternalPhoneFromMpCheck(ICollection<FraudDetection> fraudDetections, IEnumerable<Customer> customerPortion, Customer customer)
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

			var mpPhoneDetections = new Dictionary<Customer, List<MpPhone>>();
			foreach (var cd in customerPortion)
			{
				if (cd == customer) continue;
				foreach (var mp in cd.CustomerMarketPlaces)
				{
					if (mp.PersonalInfo != null && !string.IsNullOrEmpty(mp.PersonalInfo.Phone) && mp.PersonalInfo.Phone.Trim() != "0")
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
																	   Regex.Replace(customerPhone.Key, @"[\d]", string.Empty), null, phone));
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
				var company = cd.Company;
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

					if (company == null) continue;
					if (string.IsNullOrEmpty(company.BusinessPhone)) continue;

					switch (company.TypeOfBusiness.Reduce())
					{
						case TypeOfBusinessReduced.Limited:
							if (company.BusinessPhone == phone)
							{
								fraudDetections.Add(Helper.CreateDetection("Customer LimitedBusinessPhone", customer, cd,
																		   customerPhone.Key, null, phone));
							}
							break;
						case TypeOfBusinessReduced.NonLimited:
							if (company.BusinessPhone == phone)
							{
								fraudDetections.Add(Helper.CreateDetection("Customer NonLimitedBusinessPhone", customer, cd,
																		   customerPhone.Key, null, phone));
							}
							break;
					}
				}
			}
		}

		private void InternalLastNamePostcodeCheck(List<FraudDetection> fraudDetections, Customer customer, IEnumerable<Customer> customers)
		{
			var customerAddresses = customer.AddressInfo.AllAddresses.ToList();
			var postcodes = customerAddresses.Select(a => a.Postcode).Where(pc => !string.IsNullOrEmpty(pc)).ToList();

			fraudDetections.AddRange(
				from ca in customers.SelectMany(c => c.AddressInfo.AllAddresses).Where(address => postcodes.Contains(address.Postcode))
				where ca.Customer != customer
				where ca.Customer.WizardStep.TheLastOne
				where ca.Customer.PersonalInfo.Surname == customer.PersonalInfo.Surname
				select
					Helper.CreateDetection("Customer Last Name, Postcode", customer, ca.Customer,
									"Customer Last Name, Posstcode",
									null, string.Format("{0}: {1}", ca.Postcode, customer.PersonalInfo.Surname)));
		}

		private void InternalShopCheck(Customer customer, List<FraudDetection> fraudDetections, IList<Customer> customers)
		{
			//Shop ID
			var customerMps = customer.CustomerMarketPlaces.Select(m => m.DisplayName).ToList();

			var mps = customers.SelectMany(c => c.CustomerMarketPlaces).Where(m => m.Marketplace.Name != "Yodlee" && m.Marketplace.Name != "Sage").ToList();

			fraudDetections.AddRange(
				from m in mps
				from cm in customerMps
				where m.DisplayName == cm
				select
					Helper.CreateDetection("Customer Marketplace Name", customer, m.Customer, "Customer Marketplace Name",
									null, string.Format("{0}: {1}", m.Marketplace.Name, m.DisplayName)));

			var yodlees = customer.CustomerMarketPlaces.Where(x => x.Marketplace.Name == "Yodlee").ToList();
			if (yodlees.Any())
			{
				InternalYodleeMpCheck(yodlees, customer, fraudDetections, customers.ToList());
			}

			//TODO Sage Check (not using sage mp name (always the same))
		}
		
		private void InternalAddressCheck(Customer customer, List<FraudDetection> fraudDetections, IEnumerable<Customer> customers)
		{
			//Address (any of home, business, directors, previous addresses)
			var customerAddresses = customer.AddressInfo.AllAddresses.Where(
					a => a.County != null || a.Director != null ||
						 a.Line1 != null || a.Line2 != null || a.Line3 != null ||
						 a.Postcode != null || a.Town != null).ToList();
			var postcodes = customerAddresses.Select(a => a.Postcode).ToList();
			var addresses = customers.SelectMany(c => c.AddressInfo.AllAddresses).Where(address => postcodes.Contains(address.Postcode)).ToList();

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
																	 IEnumerable<Customer> customers,
																	 Customer customer,
																	 bool isSkipLast = false)
		{

			var directorPortion = customers.Where(c => c.Company != null).SelectMany(c => c.Company.Directors).ToList();
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
            if (!customer.PersonalInfo.DateOfBirth.HasValue) {
                fraudDetections.Add(Helper.CreateDetection("Customer.DateOfBirth is null", customer, null, "", null, null));
                return;
            }

			//Date of birth too young ( < 21 years)
			if ((DateTime.UtcNow - customer.PersonalInfo.DateOfBirth).Value.Days / 365.25 < 21)
			{
				fraudDetections.Add(Helper.CreateDetection("Customer.DateOfBirth < 21", customer, null, "", null,
													FormattingUtils.FormatDateTimeToString(
														customer.PersonalInfo.DateOfBirth)));
			}
		}

		private void InternalYodleeMpCheck(IEnumerable<MP_CustomerMarketPlace> customerYodlees, Customer customer, List<FraudDetection> fraudDetections, IEnumerable<Customer> customers) {
			var yodlees = customers
				.SelectMany(c => c.CustomerMarketPlaces).Where(x => x.Marketplace.Name == "Yodlee")
				.SelectMany(y => y.YodleeOrders)
				.SelectMany(o => o.OrderItems)
				.Select(i => new { Name = i.accountName, Number = i.accountNumber, CustomerId = i.Order.CustomerMarketPlace.Customer.Id })
				.Distinct()
				.ToList();

			var customerYodleesItems =
				customerYodlees.SelectMany(y => y.YodleeOrders)
							   .SelectMany(o => o.OrderItems)
							   .Select(i => new { Name = i.accountName, Number = i.accountNumber })
							   .Distinct()
							   .ToList();

			fraudDetections.AddRange(
				from m in yodlees
				from cm in customerYodleesItems
				where m.Name == cm.Name && m.Number == cm.Number
				select
					Helper.CreateDetection("Customer Bank Account Name And Number", customer, this._session.Query<Customer>().FirstOrDefault(c => c.Id == m.CustomerId), "Customer Bank Account Name And Number",
									null, string.Format("{0}: {1}", m.Name, m.Number)));

			if (customer.CustomerMarketPlaces.Any(x => x.DisplayName == "ParsedBank")) {
				var parsedBankMatchedCustomers = this._session.CreateSQLQuery("EXEC FraudGetDetectionsParsedBank :CustomerID")
					.SetParameter("CustomerID", customer.Id)
					.SetResultTransformer(new AliasToBeanResultTransformer(typeof(ParsedBank)))
					.List<ParsedBank>().ToList();
				foreach (var match in parsedBankMatchedCustomers) {
					var matchCustomer = this._session.Get<Customer>(match.CustomerId);
					fraudDetections.Add(Helper.CreateDetection("Customer ParsedBank", customer, matchCustomer, "Customer ParsedBank transactions ",
									null, string.Format("same transactions {0}", match.MatchedTransactions)));
				}
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

		private static void InternalCompanyCheck(Customer customer, List<FraudDetection> fraudDetections,
													 IList<Customer> customerPortion)
		{
			
			var company = customer.Company;
			if (company != null)
			{
				//Company Name
				var companyName = company.CompanyName;
				if (!string.IsNullOrEmpty(companyName))
				{
					fraudDetections.AddRange(
						from c in customerPortion
						where c.WizardStep.TheLastOne && c.Company != null
						where
							(c.Company.CompanyName ?? "").ToLower() == companyName.ToLower()
						select
							Helper.CreateDetection("Customer CompanyName", customer, c,
							                       "Customer CompanyName",
							                       null,
							                       string.Format("{0}", companyName)));
				}
				//Company Experian Ref Number
				var companyRefNum = company.ExperianRefNum;
				if (companyRefNum != "NotFound" && string.IsNullOrEmpty(companyRefNum))
				{
					fraudDetections.AddRange(
						customerPortion.Where(c => c.Company != null && c.Company.ExperianRefNum == companyRefNum)
						               .Select(c => Helper.CreateDetection("Customer CompanyRefNumber", customer, c,
						                                                   "Customer CompanyRefNumber",
						                                                   null,
						                                                   string.Format("{0}", companyName))));
				}
			}
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
			var customerIps = customer.Session.Select(x => x.Ip).Distinct().ToList();
			if (!customerIps.Any())
			{
				return;
			}

			if (customer.FilledByBroker && customer.Broker != null)
			{
				// filled by broker ip like of other that not this brokers client, or ip like non broker client
				// also broker clients from the same firm not considered as fraud (same ip for several brokers in the same firm)

				var customersList = customerPortion.ToList();
				var brokerFilledClients = customersList.Where(c => c.Broker != null && c.FilledByBroker);
				var otherClients = customersList.Where(c => !c.FilledByBroker);

				fraudDetections.AddRange(
					brokerFilledClients
						.Where(c => c.Broker.FirmName != customer.Broker.FirmName && c.Session.Any(s => customerIps.Contains(s.Ip)))
						.Select(c => Helper.CreateDetection("Customer IP", customer, c, "Customer IP", null, c.Session.First(s => customerIps.Contains(s.Ip)).Ip)));

				fraudDetections.AddRange(
					otherClients
						.Where(c => c.Session.Any(x => customerIps.Contains(x.Ip)))
						.Select(c => Helper.CreateDetection("Customer IP", customer, c, "Customer IP", null, c.Session.First(x => customerIps.Contains(x.Ip)).Ip)));
			}
			else
			{
				fraudDetections.AddRange(
					customerPortion
					.Where(c => c.Session.Any(s => customerIps.Contains(s.Ip)))
					.Select(c => Helper.CreateDetection("Customer IP", customer, c, "Customer IP", null, c.Session.First(s => customerIps.Contains(s.Ip)).Ip)));
			}
		}

        private void IovationCheck(List<FraudDetection> fraudDetections, Customer customer) {

            var iovationChecks = this._session.QueryOver<FraudIovation>()
                .Where(x => x.CustomerID == customer.Id && (x.Result == IovationResult.D || x.Result == IovationResult.R))
                .List();
            fraudDetections.AddRange(iovationChecks
                .Select(io => Helper.CreateDetection("Iovation Transaction Check on " + io.Origin, customer, null, 
                    string.Format("Result: {0}, Reason: {1}, Score: {2}", io.Result.DescriptionAttr(), io.Reason, io.Score) , null, io.FraudIovationID.ToString())));
        }
	}

    
}
