using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Fraud;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Code;
using log4net;
using NHibernate;
using NHibernate.Linq;
using StructureMap;

namespace FraudChecker
{
    public class FraudDetectionChecker
    {
        private readonly List<FraudUser> _fu;
        private readonly ISession _session;
        private static readonly ILog Log = LogManager.GetLogger(typeof(FraudDetectionChecker));

        public FraudDetectionChecker()
        {
            _fu = (ObjectFactory.GetInstance<FraudUserRepository>()).GetAll().ToList();
            _session = ObjectFactory.GetInstance<ISession>();
        }
        /// <summary>
        /// run fraud all checks
        /// </summary>
        /// <param name="customerId">Customer.Id for check</param>
        /// <returns></returns>
        public string Check(int customerId)
        {
            var retVal = new StringBuilder();
            var startDate = DateTime.UtcNow;
            retVal.AppendLine(InternalSystemDecision(customerId, startDate));
            retVal.AppendLine(ExternalSystemDecision(customerId, startDate));
            retVal.AppendLine(SpecialBussinesRulesSystemDecision(customerId, startDate));
            return retVal.ToString();
        }

        public string SpecialBussinesRulesSystemDecision(int customerId, DateTime startDate)
        {
            //TODO: not implemented now
            return string.Empty;
        }

        public string ExternalSystemDecision(int customerId, DateTime startDate)
        {
            Log.InfoFormat("Starting fraud external system check for customerId={0}", customerId);
            var customer = _session.Load<Customer>(customerId);
			if (customer.WizardStep != WizardStepType.AllStep)
                throw new Exception(string.Format("Customer {0} not successfully  registered", customer.Id));

            var fraudDetections = new List<FraudDetection>();

            ExternalFirstLastCheck(fraudDetections, customer);
            ExternalAddressCheck(customer, fraudDetections);
            ExternalBankAccountCheck(customer, fraudDetections);
            ExternalCompanyCheck(customer, fraudDetections);
            ExternalEmailCheck(customer, fraudDetections);
            ExternalEmailDomainCheck(customer, fraudDetections);
            ExternalPhoneCheck(customer, fraudDetections);
            ExternalShopCheck(fraudDetections, customer);

            SaveInDB(fraudDetections,startDate,customer);

            Log.InfoFormat("Finish fraud internal system check for customerId={0}", customerId);
            return PrepareResultForOutput(fraudDetections);
        }

        public string InternalSystemDecision(int customerId, DateTime startDate)
        {
            Log.InfoFormat("Starting fraud internal system check for customerId={0}", customerId);
            var customer = _session.Get<Customer>(customerId);
            if (customer.WizardStep != WizardStepType.AllStep)
                throw new Exception(string.Format("Customer {0} not successfully  registered", customer.Id));

            var fraudDetections = new List<FraudDetection>();

            const int pageSize = 50;
            var customers = _session.QueryOver<Customer>().Where(x => !x.IsTest && x.Id != customerId);
            var customerCount = customers.RowCount();
            var iterationCount = customerCount/pageSize;

            for (var i = 0; i <= iterationCount; i++)
            {
                var customerPortion = customers.Skip(i*pageSize).Take(pageSize).List<Customer>();
                InternalFirstMiddleLastNameCheck(customer, customerPortion, fraudDetections);
                InternalFirstMiddleLastNameCheck(customer, customerPortion, fraudDetections, true);
                InternalLastNameDobCheck(fraudDetections, customerPortion, customer);
                InternalPhoneCheck(fraudDetections, customerPortion, customer);
                InternalPhoneFromMpCheck(fraudDetections, customer);
                InternalCompanyNameCheck(customer, fraudDetections, customerPortion);
                InternalBankAccountCheck(fraudDetections, customerPortion, customer);
                InternalDobLess21(customer, fraudDetections);
            }

            //Director
            var directorCount = _session.QueryOver<Director>().RowCount();
            iterationCount = directorCount/pageSize;
            for (var i = 0; i <= iterationCount; i++)
            {
                var directorPortion = _session.QueryOver<Director>().Skip(i*pageSize).Take(pageSize).List<Director>();
                InternalDirectorFirstMiddleLastNameCheck(fraudDetections, directorPortion, customer);
                InternalDirectorFirstMiddleLastNameCheck(fraudDetections, directorPortion, customer, true);
            }

            InternalAddressCheck(customer, fraudDetections);
            InternalLastNamePostcodeCheck(fraudDetections, customer);
            InternalShopCheck(customer, fraudDetections);

            SaveInDB(fraudDetections, startDate, customer);

            Log.InfoFormat("Finish fraud internal system check for customerId={0}", customerId);
            return PrepareResultForOutput(fraudDetections);
        }

        private void SaveInDB(IList<FraudDetection> fraudDetections, DateTime startDate, Customer customer)
        {
            var count = fraudDetections.Count;
            for (var i = 0; i < count; i++)
            {
                var fraud = fraudDetections[i];
                fraud.DateOfCheck = startDate;
                fraud.Concurrence = ConcurrencePrepare(fraud);
                _session.Save(fraud);

                //for disabling lock db
                if (i % 20 != 0) continue;
                _session.Flush();
                _session.Clear();
            }
            if (count != 0)
            {
                //All other statuses will be set manually. See documentation
                var currentCustomer = _session.Get<Customer>(customer.Id); //object customer is CustomerProxy so we need customer from DB to update him
                currentCustomer.FraudStatus = FraudStatus.FraudSuspect;
                _session.Save(currentCustomer);
            }
        }

        #region external helpers

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
                                         CreateDetection("Customer ShopDisplayName, ShopMarketplaceType", customer, null,
                                                         "Fraud ShopDisplayName, ShopMarketplaceType", f,
                                                         string.Format("{0}, {1}", d.Name, d.Type.Name
                                                             )));
        }


        private void ExternalPhoneCheck(Customer customer, List<FraudDetection> fraudDetections)
        {
            var typeOfBussiness = customer.PersonalInfo.TypeOfBusiness.Reduce();
            //phones check
            var phone = string.Empty;
            var phoneType = string.Empty;
            if (typeOfBussiness != TypeOfBusinessReduced.Personal)
                if (typeOfBussiness == TypeOfBusinessReduced.Limited)
                {
                    phone = customer.LimitedInfo.LimitedBusinessPhone;
                    phoneType = "LimitedBusinessPhone";
                }
                else
                {
                    phone = customer.NonLimitedInfo.NonLimitedBusinessPhone;
                    phoneType = "NonLimitedBusinessPhone";
                }
            if (string.IsNullOrEmpty(phoneType)) return;
            fraudDetections.AddRange(_fu.SelectMany(f => f.Phones, (f, d) => new {f, d})
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
                                                    return CreateDetection("Customer " + phoneType, customer,
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
                                         CreateDetection("Customer Email", customer, null, "Fraud Email Domain", f,
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
                                         CreateDetection("Customer Email", customer, null, "Fraud Email", f, email));
        }

        private void ExternalCompanyCheck(Customer customer, List<FraudDetection> fraudDetections)
        {
            //companys check
            var typeOfBussiness = customer.PersonalInfo.TypeOfBusiness.Reduce();
            var companyName = customer.LimitedInfo.LimitedCompanyName;

            if (typeOfBussiness != TypeOfBusinessReduced.Limited || string.IsNullOrEmpty(companyName)) return;
            var companyRegNum = customer.LimitedInfo.LimitedCompanyNumber;
            fraudDetections.AddRange(from f in _fu
                                     from c in f.Companies
                                     where
                                        !string.IsNullOrEmpty(c.CompanyName) && !string.IsNullOrEmpty(c.RegistrationNumber)
                                     where
                                         c.CompanyName == companyName && c.RegistrationNumber == companyRegNum
                                     select
                                         CreateDetection("Customer CompanyName, RegistrationNumber", customer, null,
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
                                         CreateDetection("Customer SortCode, AccountNumber", customer, null,
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
                                         CreateDetection("Customer " + add.AddressType.ToString(), customer, null,
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
                       CreateDetection("Customer FirstName, LastName", customer, null, "Fraud FirstName, LastName", x,
                                       string.Format("{0}, {1}", x.FirstName, x.LastName))));
        }

        #endregion

        #region internal helpers

        private void InternalPhoneFromMpCheck(ICollection<FraudDetection> fraudDetections, Customer customer)
        {
            //check from customer marketplaces info
            var customerPhones = new Dictionary<string, string>();
            var customerMps = ObjectFactory.GetInstance<CustomerMarketPlaceRepository>().GetAll(customer);
            foreach (var mp in customerMps)
            {
                switch (mp.Marketplace.Name)
                {
                    case "eBay":
                        var ebaUserData = mp.EbayUserData.FirstOrDefault();
                        if (ebaUserData != null)
                        {
                            if (ebaUserData.RegistrationAddress != null)
                                customerPhones.Add("Ebay phone", ebaUserData.RegistrationAddress.Phone);
                            if (ebaUserData.RegistrationAddress != null)
                                customerPhones.Add("Ebay phone 2", ebaUserData.RegistrationAddress.Phone2);
                        }
                        break;
                    case "Pay Pal":
                        if (mp.PersonalInfo != null) customerPhones.Add("PayPal phone", mp.PersonalInfo.Phone);
                        break;
                }
            }
            var phonesArray = customerPhones.Values.ToArray();

            var customerMpDetections =
                from mp in _session.Query<MP_CustomerMarketPlace>().Fetch(mp => mp.PersonalInfo)
                where mp.Customer.IsTest == false
                where mp.Customer != customer
                where mp.EbayUserData.All(e => e != null) || mp.PersonalInfo != null
                where phonesArray.Contains(mp.PersonalInfo.Phone) ||
                      mp.EbayUserData.Any(
                          x =>
                          phonesArray.Contains(x.RegistrationAddress.Phone) ||
                          phonesArray.Contains(x.RegistrationAddress.Phone2))
                select mp;

            foreach (var mpDetection in customerMpDetections)
            {
                foreach (var customerPhone in customerPhones)
                {
                    var phone = customerPhone.Value;
                    if (mpDetection.PersonalInfo.Phone == phone)
                    {
                        fraudDetections.Add(CreateDetection("Paypal Phone", customer, mpDetection.Customer,
                                                            customerPhone.Key, null, phone));
                    }

                    var ebayUserData = mpDetection.EbayUserData;
                    if (ebayUserData.Any() &&
                        ebayUserData.Last().RegistrationAddress.Phone == phone)
                    {
                        fraudDetections.Add(CreateDetection("Ebay Phone", customer, mpDetection.Customer,
                                                            customerPhone.Key, null, phone));
                    }
                    if (ebayUserData.Any() &&
                        ebayUserData.Last().RegistrationAddress.Phone2 == phone)
                    {
                        fraudDetections.Add(CreateDetection("Ebay Phone2", customer, mpDetection.Customer,
                                                            customerPhone.Key, null, phone));
                    }
                }
            }
        }

        private void InternalPhoneCheck(ICollection<FraudDetection> fraudDetections,
                                        IEnumerable<Customer> customerPortion,
                                        Customer customer)
        {
            var customerPhones = GetCustomerPhones(customer);

            //check from customer info
            foreach (var cd in customerPortion)
            {
                foreach (var customerPhone in customerPhones)
                {
                    if (cd.PersonalInfo == null) continue;

                    var phone = customerPhone.Value;
                    if (cd.PersonalInfo.DaytimePhone == phone)
                    {
                        fraudDetections.Add(CreateDetection("Customer DaytimePhone", customer, cd, customerPhone.Key,
                                                            null, phone));
                    }
                    if (cd.PersonalInfo.MobilePhone == phone)
                    {
                        fraudDetections.Add(CreateDetection("Customer MobilePhone", customer, cd, customerPhone.Key,
                                                            null, phone));
                    }
                    switch (cd.PersonalInfo.TypeOfBusiness.Reduce())
                    {
                        case TypeOfBusinessReduced.Limited:
                            if (cd.LimitedInfo.LimitedBusinessPhone == phone)
                            {
                                fraudDetections.Add(CreateDetection("Customer LimitedBusinessPhone", customer, cd,
                                                                    customerPhone.Key, null, phone));
                            }
                            break;
                        case TypeOfBusinessReduced.NonLimited:
                            if (cd.NonLimitedInfo.NonLimitedBusinessPhone == phone)
                            {
                                fraudDetections.Add(CreateDetection("Customer NonLimitedBusinessPhone", customer, cd,
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
                where ca.Customer.WizardStep == WizardStepType.AllStep
                where ca.Customer.PersonalInfo.Surname == customer.PersonalInfo.Surname
                select
                    CreateDetection("Customer Last Name, Postcode", customer, ca.Customer,
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
                      select cmp;
            var mpCount = mps.Count();
            var iterationCount = mpCount/pageSize;

            for (var i = 0; i <= iterationCount; i++)
            {
                var mpPortion = mps.Skip(i*pageSize).Take(pageSize).ToList();

                fraudDetections.AddRange(
                    from m in mpPortion
                    from cm in customerMps
                    where m.DisplayName == cm.DisplayName
                    select
                        CreateDetection("Customer Marketplace Name", customer, m.Customer, "Customer Marketplace Name",
                                        null, string.Format("{0}: {1}", m.Marketplace.Name, m.DisplayName)));
            }
        }

        private void InternalAddressCheck(Customer customer, List<FraudDetection> fraudDetections)
        {
            //Address (any of home, business, directors, previous addresses)
            var customerAddresses = customer.AddressInfo.AllAddresses.ToList();
            var postcodes = customerAddresses.Select(a => a.Postcode).ToList();
            var addresses = _session.Query<CustomerAddress>().Where(address => postcodes.Contains(address.Postcode)).Where(x=>x.Customer.IsTest == false || x.Director.Customer.IsTest);

            fraudDetections.AddRange(
                from ca in customerAddresses
                from a in addresses
                where a.Customer != ca.Customer
                where
                    ca.Line1 == a.Line1 && ca.Line2 == a.Line2 && ca.Line3 == a.Line3 && ca.Town == a.Town &&
                    ca.County == a.County
                select
                    CreateDetection("Customer " + ca.AddressType.ToString(), customer, a.Customer ?? a.Director.Customer,
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
                    CreateDetection("Customer First Name, Last Name, Middle Name", customer, d.Customer,
                                    "Director First Name, Last Name, Middle Name", null,
                                    string.Format("{0}, {1}, {2}", d.Name, d.Surname, d.Middle)));
        }

        private static void InternalDobLess21(Customer customer, List<FraudDetection> fraudDetections)
        {
            //Date of birth too young ( < 21 years)
            if ((DateTime.UtcNow - customer.PersonalInfo.DateOfBirth).Value.Days/365.25 < 21)
            {
                fraudDetections.Add(CreateDetection("Customer.DateOfBirth < 21", customer, null, "", null,
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
				where c.WizardStep == WizardStepType.AllStep
                where c.BankAccount != null
                where
                    c.BankAccount.SortCode == customer.BankAccount.SortCode &&
                    c.BankAccount.AccountNumber == customer.BankAccount.AccountNumber
                select
                    CreateDetection("Customer SortCode, AccountNumber", customer, c,
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
				where c.WizardStep == WizardStepType.AllStep &&
					  c.PersonalInfo != null
                where
                    ((c.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited
                          ? c.LimitedInfo.LimitedCompanyName
                          : c.NonLimitedInfo.NonLimitedCompanyName) ?? "").ToLower() == companyName.ToLower()
                select
                    CreateDetection("Customer CompanyName", customer, c,
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
				where c.WizardStep == WizardStepType.AllStep
				where
					c.PersonalInfo != null &&
                    c.PersonalInfo.Surname == lastName &&
                    c.PersonalInfo.DateOfBirth == customer.PersonalInfo.DateOfBirth
                select
                    CreateDetection("Customer Last Name, Date Of Birth", customer, c,
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
				where c.WizardStep == WizardStepType.AllStep
                where
					c.PersonalInfo != null &&
                    c.PersonalInfo.FirstName == firstName &&
                    c.PersonalInfo.MiddleInitial == middleName
                where !isSkipLast && c.PersonalInfo.Surname == lastName
                select
                    CreateDetection("Customer First Name, Last Name, Middle Name", customer, c,
                                    "Customer First Name, Last Name, Middle Name",
                                    null, string.Format("{0}, {1}, {2}", firstName, lastName, middleName)));
        }

        #endregion

        private static FraudDetection CreateDetection(string currentField, Customer currentCustomer,
                                                      Customer internalCustomer, string compareField,
                                                      FraudUser externalUser,
                                                      string value)
        {
            return new FraudDetection
                {
                    CompareField = compareField,
                    CurrentCustomer = currentCustomer,
                    InternalCustomer = internalCustomer,
                    CurrentField = currentField,
                    ExternalUser = externalUser,
                    Value = value ?? String.Empty
                };
        }

        private static Dictionary<string, string> GetCustomerPhones(Customer customer)
        {
            var retVal = new Dictionary<string, string>();
            if (customer.PersonalInfo == null)
            {
                return retVal;
            }
            var typeOfBussiness = customer.PersonalInfo.TypeOfBusiness.Reduce();
            if (typeOfBussiness != TypeOfBusinessReduced.Personal)
            {
                if (typeOfBussiness == TypeOfBusinessReduced.Limited)
                {
                    retVal.Add("LimitedBusinessPhone", customer.LimitedInfo.LimitedBusinessPhone);
                }
                else
                {
                    retVal.Add("NonLimitedBusinessPhone", customer.NonLimitedInfo.NonLimitedBusinessPhone);
                }
            }
            if (customer.PersonalInfo != null && !string.IsNullOrEmpty(customer.PersonalInfo.DaytimePhone))
                retVal.Add("DaytimePhone", customer.PersonalInfo.DaytimePhone);

            if (customer.PersonalInfo != null && !string.IsNullOrEmpty(customer.PersonalInfo.MobilePhone))
                retVal.Add("MobilePhone", customer.PersonalInfo.MobilePhone);

            return retVal;
        }

        private static string PrepareResultForOutput(IEnumerable<FraudDetection> fraudDetections)
        {
            return string.Join("\n", fraudDetections.Select(x =>
                                                            x.CompareField + ",  " +
                                                            x.CurrentCustomer.Id + ",  " +
                                                            x.CurrentField + ",  " +
                                                            (x.ExternalUser != null ? x.ExternalUser.Id.ToString(CultureInfo.InvariantCulture) : "") + ",  " +
                                                            x.Id + ",  " +
                                                            (x.InternalCustomer != null ? x.InternalCustomer.Id.ToString(CultureInfo.InvariantCulture) : "") + ",  " +
                                                            x.Value));
        }

        private static string ConcurrencePrepare(FraudDetection val)
        {
            if (val.ExternalUser != null)
            {
                return string.Format("{0} {1} (id={2})",
                    val.ExternalUser.FirstName,
                    val.ExternalUser.LastName,
                    val.ExternalUser.Id);
            }

            var fullname = val.InternalCustomer.PersonalInfo != null ? val.InternalCustomer.PersonalInfo.Fullname : "-";
            return string.Format("{0} (id={1})",
                    fullname,
                    val.InternalCustomer.Id);
        }
    }


    internal class FraudDetectionModel
    {
        public string CompareField { get; set; }
        public string CurrentCustomer { get; set; }
        public string InternalCustomer { get; set; }
        public string CurrentField { get; set; }
        public string ExternalUser { get; set; }
        public string Value { get; set; }

        public FraudDetectionModel()
        {
            CompareField = "";
            CurrentCustomer = null;
            InternalCustomer = null;
            CurrentField = "";
            ExternalUser = null;
            Value = "";
        }
    }
}