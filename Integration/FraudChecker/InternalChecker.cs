using System;
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
            if (customer.WizardStep != WizardStepType.AllStep)
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
                InternalPhoneFromMpCheck(fraudDetections, customer);
                InternalCompanyNameCheck(customer, fraudDetections, customerPortion);
                InternalBankAccountCheck(fraudDetections, customerPortion, customer);
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
                where ca.Customer.WizardStep == WizardStepType.AllStep
                where ca.Customer.PersonalInfo.Surname == customer.PersonalInfo.Surname
                select
                    Helper.CreateDetection("Customer Last Name, Raw Postcode", customer, ca.Customer,
                                    "Customer Last Name, Raw Postcode",
                                    null, string.Format("{0}: {1}", ca.Postcode, customer.PersonalInfo.Surname)));
        }


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
                //Get phone's from ebay and paypal
                where mp.EbayUserData.All(e => e != null) || (mp.PersonalInfo != null && mp.PersonalInfo.Phone != null && mp.PersonalInfo.Phone != "0")
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
                        fraudDetections.Add(Helper.CreateDetection("Paypal Phone", customer, mpDetection.Customer,
                                                            customerPhone.Key, null, phone));
                    }

                    var ebayUserData = mpDetection.EbayUserData;
                    if (ebayUserData.Any() &&
                        ebayUserData.Last().RegistrationAddress.Phone == phone)
                    {
                        fraudDetections.Add(Helper.CreateDetection("Ebay Phone", customer, mpDetection.Customer,
                                                            customerPhone.Key, null, phone));
                    }
                    if (ebayUserData.Any() &&
                        ebayUserData.Last().RegistrationAddress.Phone2 == phone)
                    {
                        fraudDetections.Add(Helper.CreateDetection("Ebay Phone2", customer, mpDetection.Customer,
                                                            customerPhone.Key, null, phone));
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
                where ca.Customer.WizardStep == WizardStepType.AllStep
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
        }

        private void InternalAddressCheck(Customer customer, List<FraudDetection> fraudDetections)
        {
            //Address (any of home, business, directors, previous addresses)
            var customerAddresses = customer.AddressInfo.AllAddresses.ToList();
            var postcodes = customerAddresses.Select(a => a.Postcode).ToList();
            var addresses = _session.Query<CustomerAddress>().Where(address => postcodes.Contains(address.Postcode)).Where(x => x.Customer.IsTest == false || x.Director.Customer.IsTest == false);

            fraudDetections.AddRange(
                from ca in customerAddresses
                from a in addresses
                where a.Customer != ca.Customer
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
                where c.WizardStep == WizardStepType.AllStep
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
                where c.WizardStep == WizardStepType.AllStep &&
                      c.PersonalInfo != null
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
                where c.WizardStep == WizardStepType.AllStep
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
                where c.WizardStep == WizardStepType.AllStep
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
    }
}
