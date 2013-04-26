using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Fraud;
using EZBob.DatabaseLib.Repository;
using NHibernate;
using NHibernate.Linq;
using StructureMap;

namespace EzBob.Web.Code
{
    public class FraudDetectionChecker
    {
        private readonly List<FraudUser> _fu;
        private readonly ISession _session;

        public FraudDetectionChecker()
        {
            _fu = (ObjectFactory.GetInstance<FraudUserRepository>()).GetAll().ToList();
            _session = ObjectFactory.GetInstance<ISession>();
        }

        public string ExternalSystemDecision(int customerId)
        {
            var customer = _session.Get<Customer>(customerId);
            if (!customer.IsSuccessfullyRegistered)
                throw new Exception(string.Format("Customer {0} not successfully  registered", customer.Id));

            var fraudDetections = new List<FraudDetection>();

            //First name + Last name check
            fraudDetections.AddRange(
                _fu.Where(
                    x =>
                    x.FirstName.ToLower() == customer.PersonalInfo.FirstName.ToLower() &&
                    x.LastName.ToLower() == customer.PersonalInfo.Surname.ToLower())
                   .Select(
                       x =>
                       CreateDetection("Customer FirstName, LastName", customer, null, "Fraud FirstName, LastName", x,
                                       string.Format("{0}, {1}", x.FirstName, x.LastName))));

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

            //bank accounts check
            var sortCode = customer.BankAccount.SortCode;
            var accountNumber = customer.BankAccount.AccountNumber;
            fraudDetections.AddRange(from f in _fu
                                     from d in f.BankAccounts
                                     where
                                         d.SortCode == sortCode && d.BankAccount == accountNumber
                                     select
                                         CreateDetection("Customer SortCode, AccountNumber", customer, null,
                                                         "Fraud SortCode, AccountNumber", f,
                                                         string.Format("{0}, {1}", sortCode, accountNumber)));

            //companys check
            var typeOfBussiness = customer.PersonalInfo.TypeOfBusiness.Reduce();
            if (typeOfBussiness != TypeOfBusinessReduced.Personal)
            {
                var companyName = typeOfBussiness == TypeOfBusinessReduced.Limited
                                      ? customer.LimitedInfo.LimitedCompanyName
                                      : customer.NonLimitedInfo.NonLimitedCompanyName;
                var companyRegNum = typeOfBussiness == TypeOfBusinessReduced.Limited
                                        ? customer.LimitedInfo.LimitedRefNum
                                        : customer.NonLimitedInfo.NonLimitedRefNum;
                fraudDetections.AddRange(from f in _fu
                                         from c in f.Companies
                                         where
                                             c.CompanyName == companyName && c.RegistrationNumber == companyRegNum
                                         select
                                             CreateDetection("Customer CompanyName, RegistrationNumber", customer, null,
                                                             "Fraud CompanyName, RegistrationNumber", f,
                                                             string.Format("{0}, {1}", companyName, companyRegNum)));
            }

            //emails check
            var email = customer.Name;
            fraudDetections.AddRange(from f in _fu
                                     from d in f.Emails
                                     where
                                         d.Email == email
                                     select
                                         CreateDetection("Customer Email", customer, null, "Fraud Email", f, email));

            //email domains check
            var customerEmailDomain =
                customer.Name.Substring(customer.Name.IndexOf("@", System.StringComparison.Ordinal));
            fraudDetections.AddRange(from f in _fu
                                     from d in f.EmailDomains
                                     where
                                         d.EmailDomain == customerEmailDomain
                                     select
                                         CreateDetection("Customer Email", customer, null, "Fraud Email Domain", f,
                                                         d.EmailDomain));

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
            //shops check
            fraudDetections.AddRange(from f in _fu
                                     from d in f.Shops
                                     from s in customer.CustomerMarketPlaces
                                     where
                                         d.Name == s.DisplayName && d.Type == s.Marketplace
                                     select
                                         CreateDetection("Customer ShopDisplayName, ShopMarketplaceType", customer, null,
                                                         "Fraud ShopDisplayName, ShopMarketplaceType", f,
                                                         string.Format("{0}, {1}", d.Name, d.Type.Name
                                                             )));


            var resultString =
                string.Join("\n", fraudDetections.Select(x =>
                                                         x.CompareField + ",  " +
                                                         x.CurrentCustomer.Id + ",  " +
                                                         x.CurrentField + ",  " +
                                                         x.ExternalUser.Id + ",  " +
                                                         x.Id + ",  " +
                                                         x.InternalCustomer + ",  " +
                                                         x.Value));


            for (var i = 0; i < fraudDetections.Count; i++)
            {
                _session.Save(fraudDetections[i]);
                if (i%20 != 0) continue;
                _session.Flush();
                _session.Clear();
            }

            return resultString;
        }

        public string InternalSystemDecision(int customerId)
        {
            var customer = _session.Get<Customer>(customerId);
            if (!customer.IsSuccessfullyRegistered)
                throw new Exception(string.Format("Customer {0} not successfully  registered", customer.Id));

            var fraudDetections = new List<FraudDetection>();

            const int PAGE_SIZE = 50;
            var customerCount = _session.QueryOver<Customer>().RowCount();
            var iterationCount = customerCount/PAGE_SIZE;

            for (var i = 0; i <= iterationCount; i++)
            {
                var customerPortion = _session.QueryOver<Customer>().Skip(i*PAGE_SIZE).Take(PAGE_SIZE).List<Customer>();

                //Name
                var firstName = customer.PersonalInfo.FirstName;
                var lastName = customer.PersonalInfo.Surname;
                var middleName = customer.PersonalInfo.MiddleInitial;

                // First + Middle + Last
                fraudDetections.AddRange(
                    from c in customerPortion
                    where c.IsSuccessfullyRegistered
                    where
                        c.PersonalInfo.FirstName == firstName && c.PersonalInfo.Surname == lastName &&
                        c.PersonalInfo.MiddleInitial == middleName
                    select
                        CreateDetection("Customer First Name, Last Name, Middle Name", customer, c,
                                        "Customer First Name, Last Name, Middle Name",
                                        null, string.Format("{0}, {1}, {2}", firstName, lastName, middleName)));
                // First + Last
                fraudDetections.AddRange(
                    from c in customerPortion
                    where c.IsSuccessfullyRegistered
                    where
                        c.PersonalInfo.FirstName == firstName && c.PersonalInfo.Surname == lastName &&
                        c.PersonalInfo.MiddleInitial == middleName
                    select
                        CreateDetection("Customer First Name, Last Name", customer, c,
                                        "Customer First Name, Last Name",
                                        null, string.Format("{0}, {1}", firstName, lastName)));

                //Last name + date of birth
                fraudDetections.AddRange(
                    from c in customerPortion
                    where c.IsSuccessfullyRegistered
                    where
                        c.PersonalInfo.Surname == lastName &&
                        c.PersonalInfo.DateOfBirth == customer.PersonalInfo.DateOfBirth
                    select
                        CreateDetection("Customer Last Name, Date Of Birth", customer, c,
                                        "Customer Last Name, Date Of Birth",
                                        null,
                                        string.Format("{0}, {1}", c.PersonalInfo.Surname, c.PersonalInfo.DateOfBirth)));

                //Phone (any of them, mobile, land line) - eBay, PayPal, Application Mobile, Application Daytime, Application Limited Business Phone, Application Non-Limited Business Phone


                //Name of company
                var typeOfBussiness = customer.PersonalInfo.TypeOfBusiness.Reduce();
                var companyName = typeOfBussiness == TypeOfBusinessReduced.NonLimited
                                      ? customer.NonLimitedInfo.NonLimitedCompanyName
                                      : customer.LimitedInfo.LimitedCompanyName;
                fraudDetections.AddRange(
                    from c in customerPortion
                    where c.IsSuccessfullyRegistered
                    where
                        ((c.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited
                              ? c.LimitedInfo.LimitedCompanyName
                              : c.NonLimitedInfo.NonLimitedCompanyName) ?? "").ToLower() == companyName.ToLower()
                    select
                        CreateDetection("Customer CompanyName", customer, c,
                                        "Customer CompanyName",
                                        null,
                                        string.Format("{0}", companyName)));

                //Bank Account (sort + Bank Account)
                fraudDetections.AddRange(
                    from c in customerPortion
                    where c.IsSuccessfullyRegistered
                    where c.BankAccount != null
                    where
                        c.BankAccount.SortCode == customer.BankAccount.SortCode &&
                        c.BankAccount.AccountNumber == customer.BankAccount.AccountNumber
                    select
                        CreateDetection("Customer SortCode, AccountNumber", customer, c,
                                        "Customer SortCode, AccountNumber",
                                        null,
                                        string.Format("{0}, {1}", c.BankAccount.SortCode, c.BankAccount.AccountNumber)));

                //Last name + post code


                //Date of birth too young ( < 21 years)
                if ((DateTime.UtcNow - customer.PersonalInfo.DateOfBirth).Value.Days/365.25 < 21)
                {
                    fraudDetections.Add(CreateDetection("Customer.DateOfBirth < 21", customer, null, "", null,
                                                        FormattingUtils.FormatDateTimeToString(
                                                            customer.PersonalInfo.DateOfBirth)));
                }
            }

            //Direcor
            var directorCount = _session.QueryOver<Director>().RowCount();
            iterationCount = directorCount/PAGE_SIZE;
            for (var i = 0; i <= iterationCount; i++)
            {
                var directorPortion = _session.QueryOver<Director>().Skip(i*PAGE_SIZE).Take(PAGE_SIZE).List<Director>();
                // First + Last
                fraudDetections.AddRange(
                    from d in directorPortion
                    where d.Customer != null
                    where d.Customer.Id != customer.Id
                    where customer.PersonalInfo.FirstName == d.Name && customer.PersonalInfo.Surname == d.Surname
                    select
                        CreateDetection("Customer First Name, Last Name", customer, d.Customer,
                                        "Director First Name, Last Name", null,
                                        string.Format("{0}, {1}", d.Name, d.Surname)));
                // First + Middle + Last
                fraudDetections.AddRange(
                    from d in directorPortion
                    where d.Customer != null
                    where d.Customer.Id != customer.Id
                    where
                        customer.PersonalInfo.FirstName == d.Name && customer.PersonalInfo.Surname == d.Surname &&
                        customer.PersonalInfo.MiddleInitial == d.Middle
                    select
                        CreateDetection("Customer First Name, Last Name, Middle Name", customer, d.Customer,
                                        "Director First Name, Last Name, Middle Name", null,
                                        string.Format("{0}, {1}, {2}", d.Name, d.Surname, d.Middle)));
            }

            //Address (any of home, business, directors, previous addresses)
            var customerAddresses = customer.AddressInfo.AllAddresses.ToList();
            var postcodes = customerAddresses.Select(a => a.Postcode).ToList();
            var addresses = _session.Query<CustomerAddress>().Where(address => postcodes.Contains(address.Postcode));

            fraudDetections.AddRange(
                from allAddresses in addresses
                from customerAddress in customerAddresses
                where customerAddress.Id != allAddresses.Id
                where
                    allAddresses.Line1 == customerAddress.Line1 && allAddresses.Line2 == customerAddress.Line2 &&
                    allAddresses.Line3 == customerAddress.Line3 &&
                    allAddresses.Postcode == customerAddress.Postcode && allAddresses.County == customerAddress.County
                select
                    CreateDetection(customerAddress.AddressType.ToString(), customer,
                                    allAddresses.Customer ?? allAddresses.Director.Customer,
                                    allAddresses.AddressType.ToString(),
                                    null,
                                    string.Format("{0}, {1}, {2}, {3}, {4}",
                                                  customerAddress.Line1, customerAddress.Line2, customerAddress.Line3,
                                                  customerAddress.Postcode, customerAddress.County)));

            //Shop ID
            var customerMps = ObjectFactory.GetInstance<CustomerMarketPlaceRepository>().GetAll(customer);
            var mpCount = _session.QueryOver<MP_CustomerMarketPlace>().RowCount();
            iterationCount = mpCount/PAGE_SIZE;
            for (var i = 0; i <= iterationCount; i++)
            {
                var mpPortion =
                    _session.QueryOver<MP_CustomerMarketPlace>()
                            .Skip(i*PAGE_SIZE)
                            .Take(PAGE_SIZE)
                            .List<MP_CustomerMarketPlace>();

                fraudDetections.AddRange(
                    from m in mpPortion
                    from cm in customerMps
                    where m.Customer.Id != customer.Id
                    where m.DisplayName == cm.DisplayName
                    select
                        CreateDetection("Customer Marketplace Name", customer, m.Customer, "Customer Marketplace Name",
                                        null, string.Format("{0}: {1}", m.Marketplace.Name, m.DisplayName)));

                fraudDetections = fraudDetections.Distinct().ToList();
            }

            var resultString =
                string.Join("\n", fraudDetections.Select(x =>
                                                         x.CompareField + ",  " +
                                                         x.CurrentCustomer.Id + ",  " +
                                                         x.CurrentField + ",  " +
                                                         (x.InternalCustomer != null ? x.InternalCustomer.Id : 0) +
                                                         ",  " +
                                                         x.Value));
            return resultString;
        }

        private FraudDetection CreateDetection(string currentField, Customer currentCustomer,
                                               Customer internalCustomer, string compareField, FraudUser externalUser,
                                               string value)
        {
            return new FraudDetection
                {
                    CompareField = compareField,
                    CurrentCustomer = currentCustomer,
                    InternalCustomer = internalCustomer,
                    CurrentField = currentField,
                    ExternalUser = externalUser,
                    Value = value
                };
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