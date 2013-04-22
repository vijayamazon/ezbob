using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Fraud;
using EZBob.DatabaseLib.Repository;
using StructureMap;

namespace EzBob.Web.Code
{
    public class FraudDetectionChecker
    {
        private readonly List<FraudUser> _fu;
        private readonly FraudDetectionRepository _fraudDetectioRepository;

        public FraudDetectionChecker()
        {
            _fu = (ObjectFactory.GetInstance<FraudUserRepository>()).GetAll().ToList();
            _fraudDetectioRepository = ObjectFactory.GetInstance<FraudDetectionRepository>();
        }

        public string ExternalSystemDecision(Customer customer)
        {
            var fraudDetections = new List<FraudDetection>();

            //First name + Last name check
            fraudDetections.AddRange(
                _fu.Where(
                    x =>
                    x.FirstName.ToLower() == customer.PersonalInfo.FirstName.ToLower() &&
                    x.LastName.ToLower() == customer.PersonalInfo.Surname.ToLower())
                   .Select(
                       x =>
                       CreateDetection("FirstName + LastName", customer, null, "", x,
                                       string.Format("{0} + {1}", x.FirstName, x.LastName))));

            //addresses check
            var address = customer.AddressInfo.AllAddresses;
            fraudDetections.AddRange(from f in _fu
                                     from d in f.Addresses
                                     from add in address
                                     where
                                         d.Line1 == add.Line1 && d.Line2 == add.Line2 && d.Line3 == add.Line3 &&
                                         d.Postcode == add.Postcode && d.County == add.County
                                     select
                                         CreateDetection("Line1 + Line2 + Line3 + Postcode + County", customer, null, "",
                                                         f,
                                                         string.Format("{0} + {1} + {2} + {3} + {4}",
                                                                       d.Line1, d.Line2, d.Line3, d.Postcode, d.County)));

            //bank accounts check
            var sortCode = customer.BankAccount.SortCode;
            var accountNumber = customer.BankAccount.AccountNumber;
            fraudDetections.AddRange(from f in _fu
                                     from d in f.BankAccounts
                                     where
                                         d.SortCode == sortCode && d.BankAccount == accountNumber
                                     select
                                         CreateDetection("SortCode + AccountNumber", customer, null, "", f,
                                                         string.Format("{0} + {1}", sortCode, accountNumber)));

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
                                             CreateDetection("CompanyName + RegistrationNumber", customer, null, "", f,
                                                             string.Format("{0} + {1}", companyName, companyRegNum)));
            }

            //emails check
            var email = customer.Name;
            fraudDetections.AddRange(from f in _fu
                                     from d in f.Emails
                                     where
                                         d.Email == email
                                     select
                                         CreateDetection("Email", customer, null, "", f, email));

            //email domains check
            var customerEmailDomain =
                customer.Name.Substring(customer.Name.IndexOf("@", System.StringComparison.Ordinal));
            fraudDetections.AddRange(from f in _fu
                                     from d in f.EmailDomains
                                     where
                                         d.EmailDomain == customerEmailDomain
                                     select CreateDetection("Email domain", customer, null, "", f, d.EmailDomain));

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
                                                    return CreateDetection(phoneType, customer,
                                                                           null, "", @t.f, @t.d.PhoneNumber);
                                                }
                                         ));
            //shops check
            fraudDetections.AddRange(from f in _fu
                                     from d in f.Shops
                                     from s in customer.CustomerMarketPlaces
                                     where
                                         d.Name == s.DisplayName && d.Type == s.Marketplace
                                     select
                                         CreateDetection("ShopDisplayName + ShopMarketplaceType", customer, null, "", f,
                                                         string.Format("{0} + {1}", d.Name, d.Type.Name
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
            return resultString;
        }

        public string InternalSystemDecision(Customer customer)
        {
            return "";
        }

        private FraudDetection CreateDetection(string compareField, Customer currentCustomer,
                                               Customer internalCustomer, string currentField, FraudUser externalUser,
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