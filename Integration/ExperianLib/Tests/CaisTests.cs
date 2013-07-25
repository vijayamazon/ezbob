using System;
using ExperianLib.CaisFile;
using NUnit.Framework;

namespace ExperianLib.Tests
{
    class CaisTests : BaseTest
    {
        [Test]
        public void CaisConsumerTest()
        {
            var f = new CaisFileData
            {
                Header =
                {
                    CompanyPortfolioName = "sdf sdaf",
                    DateCreation = DateTime.Now,
                    IsCardsBehaviouralSharing = false,
                    OverdraftReportingCutOff = 312,
                    SourceCodeNumber = 231
                }
            };
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

            var data = f.WriteToString();
            Log.DebugFormat("Cais consumer data: {0}", data);
            Assert.That(!string.IsNullOrEmpty(data));
        }

        //-----------------------------------------------------------------------------------
        [Test]
        public void TestBusinessSerialize()
        {
            var cais = new BusinessCaisFileData
            {
                Header =
                {
                    CompanyPortfolioName = "Test Company",
                    CreditCardBehaviouralSharingFlag = "",
                    DateOfCreation = DateTime.Now
                }
            };

            var record = new BusinessAccountRecord
            {
                AccountNumber = "12345B6789B",
                ProprietorPartnerDirectorNumber = 31,
                LimitedNonlimitedAndOtherFlag = "L",
                NameAddressRegisteredOfficeTradingAddress =
                {
                    Name = "Patrick O'Neil Nurseries",
                    AddressLine1 = "Rose Cottage",
                    AddressLine2 = "5 Main Street",
                    AddressLine3 = "Wollaton",
                    AddressLine4 = "Nottingham Nottinghamshire",
                    PostCode = "NG2 3SD"
                },
                AddressType = "T",
                NameChange = "N",
                CompanyRegisteredNumberBusinessNumber = "SC000123",
                SICCode = 201,
                VATNumber = "111222333",
                YearBusinessStarted = 1980,
                AdditionalTradingStyle = "Fishing Accessories Sales",
                BusinessCompanyTelephoneNumber = "01999996789",
                BusinessCompanyWebsite = "www.abc.com",
                PointOfContactName = "John Smith",
                PointOfContactEmailAddress = "JohnSmith@abc.com",
                PointOfContactTelephoneNumber = "01999996789",
                PointOfContactJobTitle = "Managing Director",
                ParentCompanyNameAddress =
                {
                    Name = "General Finance Co. Ltd.",
                    AddressLine1 = "Rose Cottage",
                    AddressLine2 = "5 Main Street",
                    AddressLine3 = "Wollaton",
                    AddressLine4 = "Nottingham Nottinghamshire",
                    PostCode = "NG2 3SD"
                },
                ParentCompanyRegisteredNumber = "",
                ParentCompanyTelephoneNumber = "",
                ParentCompanyVATNumber = "",
                PreviousNameandAddress =
                {
                    Name = "",
                    AddressLine1 = "",
                    AddressLine2 = "",
                    AddressLine3 = "",
                    AddressLine4 = "",
                    PostCode = ""
                },
                ProprietorPartnerDirectororOtherFlag = "P",
                SignatoryontheAccountFlag = "Y",
                ShareholdersFlag = "",
                CountryofRegistration = "FR",
                DateofBirth = DateTime.MinValue,
                ProprietorsDirectorsGuarantee = "",
                ProprietorsDirectorsGuaranteeCancelledDischarged = "",
                AccountType = 2,
                StartDateofAgreement = new DateTime(1970, 6, 12),
                CloseDateofAgreement = DateTime.MinValue,
                MonthlyPayment = 200,
                RepaymentPeriod = 48,
                CurrentBalance = 3600,
                CreditBalanceIndicator = "",
                AccountStatus = "0",
                SpecialInstructionIndicator = "",
                CreditLimit = 5000,
                FlagSettings = "A",
                Debenture = "",
                MortgageFlags = "",
                AirtimeStatusFlag = "L",
                TransferredtoCollectionAccountFlag = "",
                BalanceType = "A",
                CreditTurnover = 2000,
                PrimaryAccountIndicator = "N",
                DefaultSatisfactionDate = new DateTime(2012, 1, 1),
                RejectionFlag = "",
                BankerDetailsSortCode = 19901,
                OriginalDefaultBalance = 5000,
                PaymentFrequencyIndicator = "W",
                NumberofCreditCardsissued = 0,
                PaymentAmount = 0,
                PaymentCreditIndicator = "",
                PreviousStatementBalance = 0,
                PreviousStatementBalanceIndicator = "",
                NumberofCashAdvances = 0,
                ValueofCashAdvances = 0,
                PaymentCode = "",
                PromotionActivityFlag = "",
                PaymentType = "C",
                NewAccountNumber = "",
                NewProprietorPartnerDirectorNumber = ""
            };

            cais.Accounts.Add(record);
            var data = cais.WriteToString();
            Log.DebugFormat("Cais business data: {0}", data);
            Assert.That(!string.IsNullOrEmpty(data));
            //cais.WriteToFile(@"f:\temp\PIF1160a.txt");
            
        }
    }
}
