namespace FraudChecker {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Fraud;
	using Ezbob.Backend.Models;
	using NHibernate;
	using NHibernate.Linq;
	using StructureMap;
	using System.Text.RegularExpressions;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Repository;
	using NHibernate.Criterion;
	using NHibernate.Transform;

	public class InternalChecker {
		public InternalChecker(int customerID, FraudMode mode) {
			this.session = ObjectFactory.GetInstance<ISession>();
			this.whiteList = ObjectFactory.GetInstance<MP_WhiteListRepository>();
			this.fraudDetections = new List<FraudDetection>();
			this.mode = mode;
			this.customerID = customerID;
		} // constructor

		public List<FraudDetection> Decide() {
			log.Info("Starting fraud internal system check for customerId {0}...", this.customerID);

			this.customer = this.session.Get<Customer>(this.customerID);
			if (this.customer == null)
				throw new Exception("Customer not found.");

			int[] customerIds = this.session
				.CreateSQLQuery("EXEC FraudGetDetections " + this.customerID)
				.List<int>()
				.ToArray();

			log.Debug("# of potential fraud customers ids: {0}", customerIds.Count());

			// Fixing exception of too many input parameters if too many potential fraud detections returned
			if (customerIds.Count() > 2000)
				customerIds = customerIds.Take(2000).ToArray();

			this.customers = this.session
				.QueryOver<Customer>()
				.Where(x => x.Id.IsIn(customerIds))
				.List<Customer>()
				.Where(c => c.Id != this.customerID)
				.ToList();

			if (this.customers.Any()) {
				log.Debug("# of potential fraud customers: {0}", this.customers.Count());

				Origin();

				switch (this.mode) {
				case FraudMode.PersonalDetaisCheck:
					Personal();
					break;

				case FraudMode.CompanyDetailsCheck:
					Company();
					break;

				case FraudMode.MarketplacesCheck:
					Marketplaces();
					break;

				case FraudMode.FullCheck:
					Personal();
					Company();
					Marketplaces();
					BankAccount();

					break;
				} // switch
			} else
				log.Debug("None of the customers match any of parameters.");

			log.Info("Finish fraud internal system check for customerId {0}.", this.customerID);

			return this.fraudDetections;
		} // Decide

		private class SameCustomer : IEqualityComparer<Customer> {
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <returns>
			/// true if the specified objects are equal; otherwise, false.
			/// </returns>
			/// <param name="x">The first object of type <paramref name="T"/> to compare.</param>
			/// <param name="y">The second object of type <paramref name="T"/> to compare.</param>
			public bool Equals(Customer x, Customer y) {
				if (x == y)
					return true;

				return x.Id == y.Id;
			} // Equals

			/// <summary>
			/// Returns a hash code for the specified object.
			/// </summary>
			/// <returns>
			/// A hash code for the specified object.
			/// </returns>
			/// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
			/// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type
			/// and <paramref name="obj"/> is null.</exception>
			public int GetHashCode(Customer obj) {
				return obj.Id.GetHashCode();
			} // GetHashCode
		} // class SameCustomer

		private void Origin() {
			string email = this.customer.Name;
			int originID = this.customer.CustomerOrigin.CustomerOriginID;

			this.fraudDetections.AddRange(this.customers
				.Where(c => c.Name == email && c.CustomerOrigin.CustomerOriginID != originID)
				.Distinct(new SameCustomer())
				.Select(c => Helper.CreateDetection(
					"Same email, different origins",
					this.customer,
					c,
					"Same email, different origins",
					null,
					string.Format("{0}; this {1}, other {2}", email, originID, c.CustomerOrigin.CustomerOriginID)
				))
			);
		} // Origin

		private void Personal() {
			FirstMiddleLastName();
			FirstMiddleLastName(true);
			LastNameDob();
			Phone();
			Address();
			LastNamePostcode();
			LastNameRawPostcode();
			Ip();
			DobLess21();
			Iovation();
		} // Personal

		private void Company() {
			CompanyName();
			DirectorFirstMiddleLastName();
			DirectorFirstMiddleLastName(true);
		} // Company

		private void Marketplaces() {
			Shop();
			PhoneFromMp();
		} // Marketplaces

		private void LastNameRawPostcode() {
			List<string> postcodes = this.customer
				.AddressInfo
				.AllAddresses
				.Select(a => a.Rawpostcode)
				.Where(rpc => !string.IsNullOrEmpty(rpc))
				.ToList();

			this.fraudDetections.AddRange(
				from ca in this.customers
					.SelectMany(c => c.AddressInfo.AllAddresses)
					.Where(address => postcodes.Contains(address.Rawpostcode))
				where ca.Customer != this.customer
				where ca.Customer.WizardStep.TheLastOne
				where ca.Customer.PersonalInfo.Surname == this.customer.PersonalInfo.Surname
				select Helper.CreateDetection(
					"Customer Last Name, Raw Postcode",
					this.customer,
					ca.Customer,
					"Customer Last Name, Raw Postcode",
					null,
					string.Format("{0}: {1}", ca.Rawpostcode, this.customer.PersonalInfo.Surname)
				)
			);
		} // LastNameRawPostcode

		private void PhoneFromMp() {
			var customerPhones = new Dictionary<string, string>();

			IEnumerable<MP_CustomerMarketPlace> customerMps = ObjectFactory.GetInstance<CustomerMarketPlaceRepository>()
				.GetAll(this.customer);

			foreach (var mp in customerMps) {
				switch (mp.Marketplace.Name) {
				case "eBay":
					MP_EbayUserData ebayUserData = mp.EbayUserData.FirstOrDefault();

					if ((ebayUserData == null) || (ebayUserData.RegistrationAddress == null))
						continue;

					if (!string.IsNullOrEmpty(ebayUserData.RegistrationAddress.Phone)) {
						customerPhones.Add(
							"Ebay phone" + ebayUserData.Id,
							ebayUserData.RegistrationAddress.Phone.Replace(" ", "")
						);
					} // if

					if (!string.IsNullOrEmpty(ebayUserData.RegistrationAddress.Phone2)) {
						customerPhones.Add(
							"Ebay phone 2" + ebayUserData.Id,
							ebayUserData.RegistrationAddress.Phone2.Replace(" ", "")
						);
					} // if

					break;

				case "Pay Pal":
					if (mp.PersonalInfo != null && !string.IsNullOrEmpty(mp.PersonalInfo.Phone))
						customerPhones.Add("PayPal phone" + mp.Id, mp.PersonalInfo.Phone.Replace(" ", ""));

					break;
				} // switch
			} // for each

			var mpPhoneDetections = new Dictionary<Customer, List<MpPhone>>();

			foreach (Customer cd in this.customers) {
				if (cd == this.customer)
					continue;

				foreach (MP_CustomerMarketPlace mp in cd.CustomerMarketPlaces) {
					bool hasPayPal =
						(mp.PersonalInfo != null) &&
						!string.IsNullOrEmpty(mp.PersonalInfo.Phone) &&
						(mp.PersonalInfo.Phone.Trim() != "0");

					if  (hasPayPal)
						AddValue(mpPhoneDetections, cd, "Pay Pal", mp.PersonalInfo.Phone.Replace(" ", ""));

					if (mp.EbayUserData == null)
						continue;

					foreach (MP_EbayUserData ebay in mp.EbayUserData.Where(eb => eb.RegistrationAddress != null)) {
						if (!string.IsNullOrEmpty(ebay.RegistrationAddress.Phone)) {
							AddValue(
								mpPhoneDetections,
								cd,
								"Ebay Phone1",
								ebay.RegistrationAddress.Phone.Replace(" ", "")
							);
						} // if

						if (!string.IsNullOrEmpty(ebay.RegistrationAddress.Phone2)) {
							AddValue(
								mpPhoneDetections,
								cd,
								"Ebay Phone2",
								ebay.RegistrationAddress.Phone2.Replace(" ", "")
							);
						} // if
					} // for each eBay user data
				} // for each customer marketplace
			} // for each customer

			foreach (KeyValuePair<Customer, List<MpPhone>> kvp in mpPhoneDetections) {
				Customer cd = kvp.Key;

				List<MpPhone> lst = kvp.Value;

				foreach (MpPhone mpPhone in lst) {
					foreach (KeyValuePair<string, string> customerPhone in customerPhones) {
						string phone = customerPhone.Value;
						if (mpPhone.Phone == phone) {
							this.fraudDetections.Add(Helper.CreateDetection(
								mpPhone.MpType,
								this.customer,
								cd,
								Regex.Replace(customerPhone.Key, @"[\d]", string.Empty),
								null,
								phone
							));
						} // if
					} // for each customer phone
				} // for each marketplace phone
			} // for each phone detection
		} // PhoneFromMp

		private Dictionary<string, string> GetCustomerPhones() {
			var retVal = new Dictionary<string, string>();
			if (this.customer.PersonalInfo == null)
				return retVal;

			if (this.customer.Company != null && !string.IsNullOrEmpty(this.customer.Company.BusinessPhone))
				retVal.Add("BusinessPhone", this.customer.Company.BusinessPhone);

			if (this.customer.PersonalInfo == null)
				return retVal;

			if (!string.IsNullOrEmpty(this.customer.PersonalInfo.DaytimePhone))
				retVal.Add("DaytimePhone", this.customer.PersonalInfo.DaytimePhone);

			if (!string.IsNullOrEmpty(this.customer.PersonalInfo.MobilePhone))
				retVal.Add("MobilePhone", this.customer.PersonalInfo.MobilePhone);

			return retVal;
		} // GetCustomerPhones

		private void Phone() {
			Dictionary<string, string> customerPhones = GetCustomerPhones();

			foreach (Customer cd in this.customers) {
				Company company = cd.Company;

				foreach (KeyValuePair<string, string> customerPhone in customerPhones) {
					string phoneName = customerPhone.Key;
					string phone = customerPhone.Value;

					if (cd.PersonalInfo != null) {
						if (cd.PersonalInfo.DaytimePhone == phone) {
							this.fraudDetections.Add(Helper.CreateDetection(
								"Customer DaytimePhone",
								this.customer,
								cd,
								phoneName,
								null,
								phone
							));
						} // if

						if (cd.PersonalInfo.MobilePhone == phone) {
							this.fraudDetections.Add(Helper.CreateDetection(
								"Customer MobilePhone",
								this.customer,
								cd,
								phoneName,
								null,
								phone
							));
						} // if
					} // if

					if (company == null)
						continue;

					if (string.IsNullOrEmpty(company.BusinessPhone))
						continue;

					switch (company.TypeOfBusiness.Reduce()) {
					case TypeOfBusinessReduced.Limited:
						if (company.BusinessPhone == phone) {
							this.fraudDetections.Add(Helper.CreateDetection(
								"Customer LimitedBusinessPhone",
								this.customer,
								cd,
								phoneName,
								null,
								phone
							));
						} // if
						break;

					case TypeOfBusinessReduced.NonLimited:
						if (company.BusinessPhone == phone) {
							this.fraudDetections.Add(Helper.CreateDetection(
								"Customer NonLimitedBusinessPhone",
								this.customer,
								cd,
								phoneName,
								null,
								phone
							));
						} // if
						break;
					} // switch
				} // for each customer phone
			} // for each customer
		} // Phone

		private void LastNamePostcode() {
			List<string> postcodes = this.customer
				.AddressInfo
				.AllAddresses
				.Select(a => a.Postcode)
				.Where(pc => !string.IsNullOrEmpty(pc))
				.ToList();

			this.fraudDetections.AddRange(
				from ca in this.customers
					.SelectMany(c => c.AddressInfo.AllAddresses)
					.Where(address => postcodes.Contains(address.Postcode))
				where ca.Customer != this.customer
				where ca.Customer.WizardStep.TheLastOne
				where ca.Customer.PersonalInfo.Surname == this.customer.PersonalInfo.Surname
				select Helper.CreateDetection(
					"Customer Last Name, Postcode",
					this.customer,
					ca.Customer,
					"Customer Last Name, Postcode",
					null,
					string.Format("{0}: {1}", ca.Postcode, this.customer.PersonalInfo.Surname)
				)
			);
		} // LastNamePostcode

		private void Shop() {
			Func<MP_CustomerMarketPlace, bool> isRelevant = (m =>
				m.Marketplace.Name != "Yodlee" &&
				m.Marketplace.Name != "Sage" &&
				m.DisplayName != "Can't get company's name"
			);

			var customerMps = this.customer.CustomerMarketPlaces
				.Where(isRelevant)
				.Select(m => new { m.DisplayName, m.Marketplace.InternalId })
				.ToList();

			customerMps.RemoveAll(m => this.whiteList.IsMarketPlaceInWhiteList(m.InternalId, m.DisplayName));

			var mps = this.customers
				.SelectMany(c => c.CustomerMarketPlaces)
				.Where(isRelevant)
				.ToList();

			this.fraudDetections.AddRange(
				from m in mps
				from cm in customerMps
				where m.DisplayName == cm.DisplayName
				select Helper.CreateDetection(
					"Customer Marketplace Name",
					this.customer,
					m.Customer,
					"Customer Marketplace Name",
					null,
					string.Format("{0}: {1}", m.Marketplace.Name, m.DisplayName)
				)
			);

			var yodlees = this.customer.CustomerMarketPlaces.Where(x => x.Marketplace.Name == "Yodlee").ToList();

			if (yodlees.Any())
				YodleeMpCheck(yodlees);

			// TODO Sage Check (not using sage mp name (always the same))
		} // Shop

		private void YodleeMpCheck(IEnumerable<MP_CustomerMarketPlace> customerYodlees) {
			var yodlees = this.customers
				.SelectMany(c => c.CustomerMarketPlaces).Where(x => x.Marketplace.Name == "Yodlee")
				.SelectMany(y => y.YodleeOrders)
				.SelectMany(o => o.OrderItems)
				.Select(i => new {
					Name = i.accountName,
					Number = i.accountNumber,
					CustomerId = i.Order.CustomerMarketPlace.Customer.Id
				})
				.Distinct()
				.ToList();

			var customerYodleesItems = customerYodlees
				.SelectMany(y => y.YodleeOrders)
				.SelectMany(o => o.OrderItems)
				.Select(i => new {
					Name = i.accountName,
					Number = i.accountNumber
				})
				.Distinct()
				.ToList();

			this.fraudDetections.AddRange(
				from m in yodlees
				from cm in customerYodleesItems
				where m.Name == cm.Name && m.Number == cm.Number
				select Helper.CreateDetection(
					"Customer Bank Account Name And Number",
					this.customer,
					this.session.Query<Customer>().FirstOrDefault(c => c.Id == m.CustomerId),
					"Customer Bank Account Name And Number",
					null,
					string.Format("{0}: {1}", m.Name, m.Number)
				)
			);

			if (this.customer.CustomerMarketPlaces.Any(x => x.DisplayName == "ParsedBank")) {
				List<ParsedBank> parsedBankMatchedCustomers = this.session
					.CreateSQLQuery("EXEC FraudGetDetectionsParsedBank :CustomerID")
					.SetParameter("CustomerID", this.customer.Id)
					.SetResultTransformer(new AliasToBeanResultTransformer(typeof(ParsedBank)))
					.List<ParsedBank>()
					.ToList();

				foreach (ParsedBank match in parsedBankMatchedCustomers) {
					Customer matchCustomer = this.session.Get<Customer>(match.CustomerId);

					this.fraudDetections.Add(Helper.CreateDetection(
						"Customer ParsedBank",
						this.customer,
						matchCustomer,
						"Customer ParsedBank transactions ",
						null,
						string.Format("same transactions {0}", match.MatchedTransactions)
					));
				} // for each
			} // if
		} // YodleeMpCheck

		private void Address() {
			List<CustomerAddress> customerAddresses = this.customer.AddressInfo.AllAddresses.Where(a =>
				a.Director != null ||
				!AddressIsEmpty(a)
			).ToList();

			List<string> postcodes = customerAddresses.Select(a => a.Postcode).ToList();

			List<CustomerAddress> addresses = this.customers
				.SelectMany(c => c.AddressInfo.AllAddresses)
				.Where(address => postcodes.Contains(address.Postcode))
				.ToList();

			this.fraudDetections.AddRange(
				from ca in customerAddresses
				from a in addresses
				where IsSameAddress(ca, a)
				select Helper.CreateDetection(
					"Customer " + ca.AddressType,
					this.customer,
					a.Customer ?? a.Director.Customer,
					"Customer " + a.AddressType,
					null,
					string.Format("{0}, {1}, {2}, {3}, {4}", a.Line1, a.Line2, a.Line3, a.Postcode, a.County)
				)
			);
		} // Address

		private static bool AddressIsEmpty(CustomerAddress a) {
			return
				string.IsNullOrWhiteSpace(a.County) &&
				string.IsNullOrWhiteSpace(a.Line1) &&
				string.IsNullOrWhiteSpace(a.Line2) &&
				string.IsNullOrWhiteSpace(a.Line3) &&
				string.IsNullOrWhiteSpace(a.Postcode) &&
				string.IsNullOrWhiteSpace(a.Town);
		} // AddressIsEmpty

		private static bool IsSameAddress(CustomerAddress ca, CustomerAddress a) {
			return
				IsSameAddressComponent(ca.Line1, a.Line1) &&
				IsSameAddressComponent(ca.Line2, a.Line2) &&
				IsSameAddressComponent(ca.Line3, a.Line3) &&
				IsSameAddressComponent(ca.Town, a.Town) &&
				IsSameAddressComponent(ca.County, a.County);
		} // IsSameAddress

		private static bool IsSameAddressComponent(string a, string b) {
			return (a ?? string.Empty).Trim() == (b ?? string.Empty).Trim();
		} // IsSameAddressComponent

		private void DirectorFirstMiddleLastName(bool isSkipLastName = false) {
			List<Director> directorPortion = this.customers
				.Where(c => c.Company != null)
				.SelectMany(c => c.Company.Directors)
				.ToList();

			string currentFieldName =
				"Customer First Name, " +
				(isSkipLastName ? string.Empty : "Last Name, ") +
				"Middle Name";

			string compareFieldName =
				"Director First Name, " +
				(isSkipLastName ? string.Empty : "Last Name, ") +
				"Middle Name";

			this.fraudDetections.AddRange(
				from d in directorPortion
				where d.Customer != null
				where d.Customer.Id != this.customer.Id
				where this.customer.PersonalInfo.FirstName == d.Name && this.customer.PersonalInfo.MiddleInitial == d.Middle
				where isSkipLastName || this.customer.PersonalInfo.Surname == d.Surname
				select Helper.CreateDetection(
					currentFieldName,
					this.customer,
					d.Customer,
					compareFieldName,
					null,
					string.Format("{0}, {1}, {2}", d.Name, d.Surname, d.Middle)
				)
			);
		} // DirectorFirstMiddleLastName

		private void DobLess21() {
			if (!this.customer.PersonalInfo.DateOfBirth.HasValue) {
				this.fraudDetections.Add(Helper.CreateDetection(
					"Customer.DateOfBirth is null",
					this.customer,
					null,
					"",
					null,
					null
				));

				return;
			} // if

			if ((DateTime.UtcNow - this.customer.PersonalInfo.DateOfBirth).Value.Days / 365.25 < 21) {
				this.fraudDetections.Add(Helper.CreateDetection(
					"Customer.DateOfBirth < 21",
					this.customer,
					null,
					"",
					null,
					FormattingUtils.FormatDateTimeToString(this.customer.PersonalInfo.DateOfBirth)
				));
			} // if
		} // DobLess21

		private void BankAccount() {
			this.fraudDetections.AddRange(
				from c in this.customers
				where c.WizardStep.TheLastOne
				where c.BankAccount != null
				where
					!string.IsNullOrEmpty(c.BankAccount.AccountNumber) &&
					!string.IsNullOrEmpty(c.BankAccount.SortCode) &&
					c.BankAccount.AccountNumber != "00000000" &&
					c.BankAccount.SortCode != "000000"
				where
					c.BankAccount.SortCode == this.customer.BankAccount.SortCode &&
					c.BankAccount.AccountNumber == this.customer.BankAccount.AccountNumber
				select Helper.CreateDetection(
					"Customer SortCode, AccountNumber",
					this.customer,
					c,
					"Customer SortCode, AccountNumber",
					null,
					string.Format("{0}, {1}", c.BankAccount.SortCode, c.BankAccount.AccountNumber)
				)
			);
		} // BankAccount

		private void CompanyName() {
			var company = this.customer.Company;

			if (company == null)
				return;

			//Company Name
			string companyName = company.CompanyName;

			if (!string.IsNullOrEmpty(companyName)) {
				this.fraudDetections.AddRange(
					from c in this.customers
					where c.WizardStep.TheLastOne && c.Company != null
					where String.Equals(
						(c.Company.CompanyName ?? "").Trim(),
						companyName.Trim(),
						StringComparison.InvariantCultureIgnoreCase
					)
					select Helper.CreateDetection(
						"Customer CompanyName",
						this.customer,
						c,
						"Customer CompanyName",
						null,
						string.Format("{0}", companyName)
					)
				);
			} // if

			// Company Experian Ref Number
			string companyRefNum = company.ExperianRefNum;

			if (companyRefNum != "NotFound" && string.IsNullOrEmpty(companyRefNum)) {
				this.fraudDetections.AddRange(
					this.customers.Where(c => c.Company != null && c.Company.ExperianRefNum == companyRefNum)
					.Select(c => Helper.CreateDetection(
						"Customer CompanyRefNumber",
						this.customer,
						c,
						"Customer CompanyRefNumber",
						null,
						string.Format("{0}", companyName)
					))
				);
			} // if
		} // CompanyName

		private void LastNameDob() {
			string lastName = this.customer.PersonalInfo.Surname;

			this.fraudDetections.AddRange(
				from c in this.customers
				where c.WizardStep.TheLastOne
				where
					c.PersonalInfo != null &&
					c.PersonalInfo.Surname == lastName &&
					c.PersonalInfo.DateOfBirth == this.customer.PersonalInfo.DateOfBirth
				select Helper.CreateDetection(
					"Customer Last Name, Date Of Birth",
					this.customer,
					c,
					"Customer Last Name, Date Of Birth",
					null,
					string.Format("{0}, {1}", c.PersonalInfo.Surname, c.PersonalInfo.DateOfBirth)
				)
			);
		} // LastNameDob

		private void FirstMiddleLastName(bool isSkipLast = false) {
			string firstName = this.customer.PersonalInfo.FirstName;
			string lastName = this.customer.PersonalInfo.Surname;
			string middleName = this.customer.PersonalInfo.MiddleInitial;

			this.fraudDetections.AddRange(
				from c in this.customers
				where c.WizardStep.TheLastOne
				where
					c.PersonalInfo != null &&
					c.PersonalInfo.FirstName == firstName &&
					c.PersonalInfo.MiddleInitial == middleName
				where isSkipLast || c.PersonalInfo.Surname == lastName
				select Helper.CreateDetection(
					"Customer First Name, Last Name, Middle Name",
					this.customer,
					c,
					"Customer First Name, Last Name, Middle Name",
					null,
					string.Format("{0}, {1}, {2}", firstName, lastName, middleName)
				)
			);
		} // FirstMiddleLastName

		private void Ip() {
			List<SessionInfo> thisCustomerSessions = this.customer.Session
				.Where(x => x.Ip != "127.0.0.1" && x.Ip != "::1")
				.Select(x => new SessionInfo(x))
				.Distinct(sessionInfoComparer)
				.ToList();

			if (thisCustomerSessions.Count < 1)
				return;

			// Filled by broker IP like of other that not this brokers client, or IP like non broker client;
			// broker clients from the same firm (by NAME!!!) are not considered as fraud
			// (same IP for several brokers in the same firm).
			if (this.customer.FilledByBroker && this.customer.Broker != null) {
				IpAgainstList(
					thisCustomerSessions, 
					this.customers.Where(c =>
						c.Broker != null &&
						c.FilledByBroker &&
						this.customer.Broker.FirmName != c.Broker.FirmName
					)
				);

				IpAgainstList(thisCustomerSessions, this.customers.Where(c => !c.FilledByBroker));
			} else
				IpAgainstList(thisCustomerSessions, this.customers);
		} // Ip

		private void IpAgainstList(List<SessionInfo> thisCustomerSessions, IEnumerable<Customer> otherCustomers) {
			foreach (Customer otherCustomer in otherCustomers) {
				foreach (CustomerSession otherCustomerSession in otherCustomer.Session) {
					CustomerSession ocs = otherCustomerSession; // Avoid "access foreach variable in closure".

					IEnumerable<SessionInfo> suspicious = thisCustomerSessions.Where(tcs => tcs.IsSuspicious(ocs));

					foreach (SessionInfo tcs in suspicious) {
						this.fraudDetections.Add(Helper.CreateDetection(
							"Customer IP",
							this.customer,
							otherCustomer,
							"Customer IP",
							null,
							string.Format(
								"{0} (this) and (id {2}) {1} (other)",
								tcs.Ip,
								otherCustomerSession.StartSession.ToString(
									"d/MMM/yyyy H:mm:ss",
									CultureInfo.InvariantCulture
								),
								otherCustomerSession.Id
							)
						));
					} // for each session of this.customer
				} // for each other customer's session
			} // for each other customer
		} // IpAgainstList

		private void Iovation() {
			var iovationChecks = this.session.QueryOver<FraudIovation>()
				.Where(x =>
					x.CustomerID == this.customer.Id && (
						x.Result == IovationResult.D ||
						x.Result == IovationResult.R
					)
				)
				.List();

			this.fraudDetections.AddRange(iovationChecks.Select(io => Helper.CreateDetection(
				"Iovation Transaction Check on " + io.Origin,
				this.customer,
				null,
				string.Format("Result: {0}, Reason: {1}, Score: {2}", io.Result.DescriptionAttr(), io.Reason, io.Score),
				null,
				io.FraudIovationID.ToString()
			)));
		} // Iovation

		private static void AddValue(
			Dictionary<Customer, List<MpPhone>> mpPhoneDetections,
			Customer cd,
			string mpType,
			string phone
		) {
			if (mpPhoneDetections.ContainsKey(cd))
				mpPhoneDetections[cd].Add(new MpPhone { MpType = mpType, Phone = phone });
			else
				mpPhoneDetections[cd] = new List<MpPhone> { new MpPhone { MpType = mpType, Phone = phone } };
		} // AddValue

		private readonly int customerID;
		private readonly FraudMode mode;
		private readonly List<FraudDetection> fraudDetections;
		private readonly MP_WhiteListRepository whiteList;
		private readonly ISession session;

		private Customer customer;
		private List<Customer> customers;

		private static readonly SessionInfoComparer sessionInfoComparer = new SessionInfoComparer();
		private static readonly ASafeLog log = new SafeILog(typeof(InternalChecker));
	} // class InternalChecker
} // namespace
