using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Fraud;
using System.Linq;

namespace EzBob.Web.Areas.Underwriter.Models.Fraud
{
    public class InputValuesModel
    {
        public InputValuesModel()
        {
            IsCombobox = false;
            Id = "";
            Caption = "";
            Value = "";
        }

        public string Id { get; set; }
        public string Caption { get; set; }
        public string Value { get; set; }
        public bool IsCombobox { get; set; }
        public IEnumerable<MP_MarketplaceType> MpType { get; set; }
    }

    public sealed class FraudModel
    {
        public FraudModel()
        {
            BankAccounts = new List<FraudBankAccount>();
            Addresses = new List<FraudAddress>();
            Companies = new List<FraudCompany>();
            Emails = new List<FraudEmail>();
            EmailDomains = new List<FraudEmailDomain>();
            Phones = new List<FraudPhone>();
            Shops = new List<FraudShopModel>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<FraudBankAccount> BankAccounts { get; set; }
        public List<FraudAddress> Addresses { get; set; }
        public List<FraudCompany> Companies { get; set; }
        public List<FraudEmail> Emails { get; set; }
        public List<FraudEmailDomain> EmailDomains { get; set; }
        public List<FraudPhone> Phones { get; set; }
        public List<FraudShopModel> Shops { get; set; }

        public static List<FraudModel> FromFraudModel(IEnumerable<FraudUser> users)
        {
            var model = new List<FraudModel>();
            model.AddRange(users.Select(user=> new FraudModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Addresses = user.Addresses.Select(x => new FraudAddress
                        {
                            County = x.County,
                            Line1 = x.Line1,
                            Line2 = x.Line2,
                            Line3 = x.Line3,
                            Postcode = x.Postcode,
                            Town = x.Town
                        }).ToList(),
                    BankAccounts = user.BankAccounts.Select(x => new FraudBankAccount
                        {
                            BankAccount = x.BankAccount,
                            SortCode = x.SortCode
                        }).ToList(),
                    Companies = user.Companies.Select(x => new FraudCompany
                        {
                            CompanyName = x.CompanyName,
                            RegistrationNumber = x.RegistrationNumber
                        }).ToList(),
                    EmailDomains = user.EmailDomains.Select(x => new FraudEmailDomain
                        {
                            EmailDomain = x.EmailDomain
                        }).ToList(),
                    Emails = user.Emails.Select(x => new FraudEmail
                        {
                            Email = x.Email
                        }).ToList(),
                    Phones = user.Phones.Select(x => new FraudPhone
                        {
                            PhoneNumber = x.PhoneNumber
                        }).ToList(),
                    Shops = user.Shops.Select(x => new FraudShopModel
                        {
                            Name = x.Name,
                            Type = x.Type.Name
                        }).ToList()
                })
            );

            return model;
        }
    }

    public class FraudShopModel
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Type { get; set; }
    }
}