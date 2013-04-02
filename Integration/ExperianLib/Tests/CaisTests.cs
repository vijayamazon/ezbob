using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ExperianLib.CaisFile;
using NUnit.Framework;
using Newtonsoft.Json;

namespace ExperianLib.Tests
{
    class CaisTests : BaseTest
    {
        [Test]
        public void CaisConsumerTest()
        {
            CaisFileData f = new CaisFileData();
            f.Header.CompanyPortfolioName = "sdf sdaf";
            f.Header.DateCreation = DateTime.Now;
            f.Header.IsCardsBehaviouralSharing = false;
            f.Header.OverdraftReportingCutOff = 312;
            f.Header.SourceCodeNumber = 231;
            var account = new AccountRecord
            {
                AccountNumber = "fa2341",
                AccountStatus = "A",
                AccountType = "sdf",
                AirtimeFlag = "-",
                BalanceType = "s",
                CloseDate = DateTime.Now,
                CreditBalanceIndicator = "d",
                CreditLimit = 1234,
                CreditPaymentIndicator = "A",
                CreditTurnover = 41231,
                TransferredCollectionAccountFlag = "s",
                CurrentBalance = 4325,
                DateBirth = DateTime.Now,
                DefaultSatisfactionDate = DateTime.Now,
                FlagSettings = "S",
                MonthlyPayment = 1234,
                NameAndAddress = new NameAndAddressData{AddressLine1 = "Road 1", AddressLine2 = "Town Gart", AddressLine3 = "Test Line 3", Name = "NICKOLAY TEST", AddressLine4 = "Address line 4", Postcode = "NU CK54T"},
                NewAccountNumber = "sdfg",
                NumberCashAdvances = 2345
            };
            f.Accounts.Add(account);

            string data = f.WriteToString();
            Log.DebugFormat("Cais consumer data: {0}", data);
        }

        //-----------------------------------------------------------------------------------
        [Test]
        public void TestBusinessSerialize()
        {
            BusinessCaisFileData cais = new BusinessCaisFileData();
            cais.Header.CompanyPortfolioName = "Test Company";
            cais.Header.CreditCardBehaviouralSharingFlag = "";
            cais.Header.DateOfCreation = DateTime.Now;

            var record = new BusinessAccountRecord();
            record.AccountNumber = "12345B6789B";
            record.ProprietorPartnerDirectorNumber = 31;
            record.LimitedNonlimitedAndOtherFlag = "L";

            record.NameAddressRegisteredOfficeTradingAddress.Name = "Patrick O'Neil Nurseries";
            record.NameAddressRegisteredOfficeTradingAddress.AddressLine1 = "Rose Cottage";
            record.NameAddressRegisteredOfficeTradingAddress.AddressLine2 = "5 Main Street";
            record.NameAddressRegisteredOfficeTradingAddress.AddressLine3 = "Wollaton";
            record.NameAddressRegisteredOfficeTradingAddress.AddressLine4 = "Nottingham Nottinghamshire";
            record.NameAddressRegisteredOfficeTradingAddress.PostCode = "NG2 3SD";

            record.AddressType = "T";
            record.NameChange = "N";
            record.CompanyRegisteredNumberBusinessNumber = "SC000123";
            record.SICCode = 201;
            record.VATNumber = "111222333";
            record.YearBusinessStarted = 1980;
            record.AdditionalTradingStyle = "Fishing Accessories Sales";
            record.BusinessCompanyTelephoneNumber = "01999996789";
            record.BusinessCompanyWebsite = "www.abc.com";
            record.PointOfContactName = "John Smith";
            record.PointOfContactEmailAddress = "JohnSmith@abc.com";
            record.PointOfContactTelephoneNumber = "01999996789";
            record.PointOfContactJobTitle = "Managing Director";

            record.ParentCompanyNameAddress.Name = "General Finance Co. Ltd.";
            record.ParentCompanyNameAddress.AddressLine1 = "Rose Cottage";
            record.ParentCompanyNameAddress.AddressLine2 = "5 Main Street";
            record.ParentCompanyNameAddress.AddressLine3 = "Wollaton";
            record.ParentCompanyNameAddress.AddressLine4 = "Nottingham Nottinghamshire";
            record.ParentCompanyNameAddress.PostCode = "NG2 3SD";

            record.ParentCompanyRegisteredNumber = "";
            record.ParentCompanyTelephoneNumber = "";
            record.ParentCompanyVATNumber = "";

            record.PreviousNameandAddress.Name = "";
            record.PreviousNameandAddress.AddressLine1 = "";
            record.PreviousNameandAddress.AddressLine2 = "";
            record.PreviousNameandAddress.AddressLine3 = "";
            record.PreviousNameandAddress.AddressLine4 = "";
            record.PreviousNameandAddress.PostCode = "";

            record.ProprietorPartnerDirectororOtherFlag = "P";
            record.SignatoryontheAccountFlag = "Y";
            record.ShareholdersFlag = "";
            record.CountryofRegistration = "FR";
            record.DateofBirth = DateTime.MinValue;
            record.ProprietorsDirectorsGuarantee = "";
            record.ProprietorsDirectorsGuaranteeCancelledDischarged = "";
            record.AccountType = 2;
            record.StartDateofAgreement = new DateTime(1970, 6, 12);
            record.CloseDateofAgreement = DateTime.MinValue;
            record.MonthlyPayment = 200;
            record.RepaymentPeriod = 48;
            record.CurrentBalance = 3600;
            record.CreditBalanceIndicator = "";
            record.AccountStatus = "0";
            record.SpecialInstructionIndicator = "";
            record.CreditLimit = 5000;
            record.FlagSettings = "A";
            record.Debenture = "";
            record.MortgageFlags = "";
            record.AirtimeStatusFlag = "L";
            record.TransferredtoCollectionAccountFlag = "";
            record.BalanceType = "A";
            record.CreditTurnover = 2000;
            record.PrimaryAccountIndicator = "N";
            record.DefaultSatisfactionDate = new DateTime(2012, 1, 1);
            record.RejectionFlag = "";
            record.BankerDetailsSortCode = 19901;
            record.OriginalDefaultBalance = 5000;
            record.PaymentFrequencyIndicator = "W";
            record.NumberofCreditCardsissued = 0;
            record.PaymentAmount = 0;
            record.PaymentCreditIndicator = "";
            record.PreviousStatementBalance = 0;
            record.PreviousStatementBalanceIndicator = "";
            record.NumberofCashAdvances = 0;
            record.ValueofCashAdvances = 0;
            record.PaymentCode = "";
            record.PromotionActivityFlag = "";
            record.PaymentType = "C";
            record.NewAccountNumber = "";
            record.NewProprietorPartnerDirectorNumber = "";

            cais.Accounts.Add(record);
            Log.DebugFormat("Cais business data: {0}", cais.WriteToString());
            //cais.WriteToFile(@"f:\temp\PIF1160a.txt");
        }

        /*[Test]
        public void Test2()
        {
            CaisFileData f = new CaisFileData();
            f.ReadFromFile(@"f:\TEMP\ezbob\exper cb\CAIS-TestPortfolio.txt");
            File.WriteAllText(@"f:\temp\1.json", JsonConvert.SerializeObject(f, Formatting.Indented));
            f.WriteToFile(@"f:\temp\1.txt");
        }        

        [Test]
        public void TestBusinessDeserialize()
        {
            var f = new BusinessCaisFileData();
            f.ReadFromFile(@"f:\temp\CaisBusiness.txt");
            f.WriteToFile(@"f:\temp\CaisBusiness2.txt");
        }

        [Test]
        public void TestBusiness5Records()
        {
            BusinessCaisFileData cais = new BusinessCaisFileData();
            cais.Header.CompanyPortfolioName = "EzBob OrangeMoney";
            cais.Header.CreditCardBehaviouralSharingFlag = "";
            cais.Header.DateOfCreation = DateTime.Now;


            for (int i = 0; i < 5; i++)
            {
                var record = new BusinessAccountRecord();
                record.AccountNumber = i.ToString(CultureInfo.InvariantCulture);
                record.ProprietorPartnerDirectorNumber = 2+i;
                record.LimitedNonlimitedAndOtherFlag = "L";

                record.NameAddressRegisteredOfficeTradingAddress.Name = "Company"+i;
                record.NameAddressRegisteredOfficeTradingAddress.AddressLine1 = "Rose Cottage";
                record.NameAddressRegisteredOfficeTradingAddress.AddressLine2 = i+2+" Main Street";
                record.NameAddressRegisteredOfficeTradingAddress.AddressLine3 = "Wollaton";
                record.NameAddressRegisteredOfficeTradingAddress.AddressLine4 = "Nottingham Nottinghamshire";
                record.NameAddressRegisteredOfficeTradingAddress.PostCode = "NG2 3SD";

                record.AddressType = "R";
                record.NameChange = "N";
                record.CompanyRegisteredNumberBusinessNumber = "SC000" + String.Format("{0:000}", i);
                record.BusinessCompanyTelephoneNumber = "01999996789";

                record.CountryofRegistration = "GB";
                record.AccountType = 2;
                record.StartDateofAgreement = new DateTime(1970, 6, 12);
                record.CloseDateofAgreement = DateTime.MinValue;
                record.MonthlyPayment = 200;
                record.RepaymentPeriod = 48;
                record.CurrentBalance = 3600;
                record.AccountStatus = "0";
                record.PaymentType = "T";

                record.BankerDetailsSortCode = 19901;

                cais.Accounts.Add(record);
            }

            

            cais.WriteToFile(@"f:\temp\PIF1160a.txt");
        }*/
    }
}
