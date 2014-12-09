using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperianLib.CaisFile
{
    public class BusinessAccountRecord
    {
        public string AccountNumber { get; set; }
        public int ProprietorPartnerDirectorNumber { get; set; }
        public string LimitedNonlimitedAndOtherFlag { get; set; }
        public BusinessAddressData NameAddressRegisteredOfficeTradingAddress { get; private set; }
        public string AddressType { get; set; }
        public string NameChange { get; set; }
        public string CompanyRegisteredNumberBusinessNumber { get; set; }
        public int SICCode { get; set; }
        public string VATNumber { get; set; }
        public int YearBusinessStarted { get; set; }
        public string AdditionalTradingStyle { get; set; }
        public string BusinessCompanyTelephoneNumber { get; set; }
        public string BusinessCompanyWebsite { get; set; }
        public string PointOfContactName { get; set; }
        public string PointOfContactEmailAddress { get; set; }
        public string PointOfContactTelephoneNumber { get; set; }
        public string PointOfContactJobTitle { get; set; }
        public BusinessAddressData ParentCompanyNameAddress { get; private set; }
        public string ParentCompanyRegisteredNumber { get; set; }
        public string ParentCompanyTelephoneNumber { get; set; }
        public string ParentCompanyVATNumber { get; set; }
        public BusinessAddressData PreviousNameandAddress { get; private set; }
        public string ProprietorPartnerDirectororOtherFlag { get; set; }
        public string SignatoryontheAccountFlag { get; set; }
        public string ShareholdersFlag { get; set; }
        public string CountryofRegistration { get; set; }
        public DateTime? DateofBirth { get; set; }
        public string ProprietorsDirectorsGuarantee { get; set; }
        public string ProprietorsDirectorsGuaranteeCancelledDischarged { get; set; }
        public int AccountType { get; set; }
        public DateTime? StartDateofAgreement { get; set; }
        public DateTime? CloseDateofAgreement { get; set; }
        public decimal MonthlyPayment { get; set; }
        public int RepaymentPeriod { get; set; }
        public decimal CurrentBalance { get; set; }
        public string CreditBalanceIndicator { get; set; }
        public string AccountStatus { get; set; }
        public string SpecialInstructionIndicator { get; set; }
        public decimal CreditLimit { get; set; }
        public string FlagSettings { get; set; }
        public string Debenture { get; set; }
        public string MortgageFlags { get; set; }
        public string AirtimeStatusFlag { get; set; }
        public string TransferredtoCollectionAccountFlag { get; set; }
        public string BalanceType { get; set; }
        public decimal CreditTurnover { get; set; }
        public string PrimaryAccountIndicator { get; set; }
        public DateTime? DefaultSatisfactionDate { get; set; }
        public string RejectionFlag { get; set; }
        public decimal BankerDetailsSortCode { get; set; }
        public decimal OriginalDefaultBalance { get; set; }
        public string PaymentFrequencyIndicator { get; set; }
        public decimal NumberofCreditCardsissued { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentCreditIndicator { get; set; }
        public decimal PreviousStatementBalance { get; set; }
        public string PreviousStatementBalanceIndicator { get; set; }
        public decimal NumberofCashAdvances { get; set; }
        public decimal ValueofCashAdvances { get; set; }
        public string PaymentCode { get; set; }
        public string PromotionActivityFlag { get; set; }
        public string PaymentType { get; set; }
        public string NewAccountNumber { get; set; }
        public string NewProprietorPartnerDirectorNumber { get; set; }

        public BusinessAccountRecord()
        {
            NameAddressRegisteredOfficeTradingAddress = new BusinessAddressData();
            ParentCompanyNameAddress = new BusinessAddressData();
            PreviousNameandAddress = new BusinessAddressData();
        }

        //-----------------------------------------------------------------------------------
        public string Serialize()
        {
			//fix for empty OriginalDefaultBalance for default accounts todo remove once the strategies translated
			if (OriginalDefaultBalance == 0 && AccountStatus == "8")
			{
				OriginalDefaultBalance = CurrentBalance;
			}
			//fix for empty address type todo remove once the strategies translated add enum retrieve the type (if possible)
			//‘R’ - Registered
			//‘H’ - Head Office
			//‘T’ - Trading
			//‘B’ - Branch
			//‘D’ - Delivery
			//‘O’ - Other
			if (string.IsNullOrEmpty(AddressType))
			{
				AddressType = "R";
			}

            var ret = new StringBuilder();
            ret.Append(Utils.GetPaddingString(AccountNumber, 19, true));
            ret.Append(Utils.GetPaddingString(ProprietorPartnerDirectorNumber, 4, false));
            ret.Append(Utils.GetPaddingString(LimitedNonlimitedAndOtherFlag, 1, false));
            ret.Append(NameAddressRegisteredOfficeTradingAddress.Serialize());
            ret.Append(Utils.GetPaddingString(AddressType, 1, false));
            ret.Append(Utils.GetPaddingString(NameChange, 1, false));
            ret.Append(Utils.GetPaddingString(CompanyRegisteredNumberBusinessNumber, 8, true, true));
            ret.Append(Utils.GetPaddingString(SICCode, 4, false));
            ret.Append(Utils.GetPaddingString(VATNumber, 9, false));
            ret.Append(Utils.GetPaddingString(YearBusinessStarted, 4, false));
            ret.Append(Utils.GetPaddingString(AdditionalTradingStyle, 45, false));
            ret.Append(Utils.GetPaddingString(BusinessCompanyTelephoneNumber, 16, true));
            ret.Append(Utils.GetPaddingString(BusinessCompanyWebsite, 100, true));
            ret.Append(Utils.GetPaddingString(PointOfContactName, 39, true));
            ret.Append(Utils.GetPaddingString(PointOfContactEmailAddress, 100, true));
            ret.Append(Utils.GetPaddingString(PointOfContactTelephoneNumber, 16, true));
            ret.Append(Utils.GetPaddingString(PointOfContactJobTitle, 40, true));
            ret.Append(ParentCompanyNameAddress.Serialize());
            ret.Append(Utils.GetPaddingString(ParentCompanyRegisteredNumber, 8, false));
            ret.Append(Utils.GetPaddingString(ParentCompanyTelephoneNumber, 16, true));
            ret.Append(Utils.GetPaddingString(ParentCompanyVATNumber, 9, false));
            ret.Append(PreviousNameandAddress.Serialize());
            ret.Append(Utils.GetPaddingString(" ", 100, false));
            ret.Append(Utils.GetPaddingString(ProprietorPartnerDirectororOtherFlag, 1, false));
            ret.Append(Utils.GetPaddingString(SignatoryontheAccountFlag, 1, false));
            ret.Append(Utils.GetPaddingString(ShareholdersFlag, 1, false));
            ret.Append(Utils.GetPaddingString(CountryofRegistration, 50, false));
            ret.Append(Utils.GetPaddingString(DateofBirth, 8, false));
            ret.Append(Utils.GetPaddingString(ProprietorsDirectorsGuarantee, 1, false));
            ret.Append(Utils.GetPaddingString(ProprietorsDirectorsGuaranteeCancelledDischarged, 1, false));
            ret.Append(Utils.GetPaddingString(" ", 97, false));
            ret.Append(Utils.GetPaddingString(AccountType, 2, false));
            ret.Append(Utils.GetPaddingString(StartDateofAgreement, 8, false));
            ret.Append(Utils.GetPaddingString(CloseDateofAgreement, 8, false));
            ret.Append(Utils.GetPaddingString(MonthlyPayment, 6, false));
            ret.Append(Utils.GetPaddingString(RepaymentPeriod, 3, false));
            ret.Append(Utils.GetPaddingString(CurrentBalance, 10, false));
            ret.Append(Utils.GetPaddingString(CreditBalanceIndicator, 1, false));
            ret.Append(Utils.GetPaddingString(AccountStatus, 1, false));
            ret.Append(Utils.GetPaddingString(SpecialInstructionIndicator, 1, false));
            ret.Append(Utils.GetPaddingString(CreditLimit, 7, false));
            ret.Append(Utils.GetPaddingString(FlagSettings, 1, false));
            ret.Append(Utils.GetPaddingString(Debenture, 1, false));
            ret.Append(Utils.GetPaddingString(MortgageFlags, 1, false));
            ret.Append(Utils.GetPaddingString(AirtimeStatusFlag, 1, false));
            ret.Append(Utils.GetPaddingString(TransferredtoCollectionAccountFlag, 1, false));
            ret.Append(Utils.GetPaddingString(BalanceType, 1, false));
            ret.Append(Utils.GetPaddingString(CreditTurnover, 9, false));
            ret.Append(Utils.GetPaddingString(PrimaryAccountIndicator, 1, false));
            ret.Append(Utils.GetPaddingString(DefaultSatisfactionDate, 8, false));
            ret.Append(Utils.GetPaddingString(RejectionFlag, 1, false));
            ret.Append(Utils.GetPaddingString(BankerDetailsSortCode, 6, false));
            ret.Append(Utils.GetPaddingString(OriginalDefaultBalance, 7, false));
            ret.Append(Utils.GetPaddingString(PaymentFrequencyIndicator, 1, false));
            ret.Append(Utils.GetPaddingString(NumberofCreditCardsissued, 5, false));
            ret.Append(Utils.GetPaddingString(PaymentAmount, 6, false));
            ret.Append(Utils.GetPaddingString(PaymentCreditIndicator, 1, false));
            ret.Append(Utils.GetPaddingString(PreviousStatementBalance, 6, false));
            ret.Append(Utils.GetPaddingString(PreviousStatementBalanceIndicator, 1, false));
            ret.Append(Utils.GetPaddingString(NumberofCashAdvances, 2, false));
            ret.Append(Utils.GetPaddingString(ValueofCashAdvances, 6, false));
            ret.Append(Utils.GetPaddingString(PaymentCode, 1, false));
            ret.Append(Utils.GetPaddingString(PromotionActivityFlag, 1, false));
            ret.Append(Utils.GetPaddingString(PaymentType, 1, false));
            ret.Append(Utils.GetPaddingString(NewAccountNumber, 19, true));
            ret.Append(Utils.GetPaddingString(NewProprietorPartnerDirectorNumber, 4, false));

            return ret.ToString();
        }

        //-----------------------------------------------------------------------------------
        public void Deserialize(string data)
        {
            if (data.Length != 1364) throw new Exception("Invalid string length, must be 1364 characters");
            AccountNumber = Utils.ToString(data, 0, 19);
            ProprietorPartnerDirectorNumber = Utils.ToInt32(data, 19, 4);
            LimitedNonlimitedAndOtherFlag = Utils.ToString(data, 23, 1);
            NameAddressRegisteredOfficeTradingAddress.Deserialize(data.Substring(24, 175));
            AddressType = Utils.ToString(data, 199, 1);
            NameChange = Utils.ToString(data, 200, 1);
            CompanyRegisteredNumberBusinessNumber = Utils.ToString(data, 201, 8);
            SICCode = Utils.ToInt32(data, 209, 4);
            VATNumber = Utils.ToString(data, 213, 9);
            YearBusinessStarted = Utils.ToInt32(data, 222, 4);
            AdditionalTradingStyle = Utils.ToString(data, 226, 45);
            BusinessCompanyTelephoneNumber = Utils.ToString(data, 271, 16);
            BusinessCompanyWebsite = Utils.ToString(data, 287, 100);
            PointOfContactName = Utils.ToString(data, 387, 39);
            PointOfContactEmailAddress = Utils.ToString(data, 426, 100);
            PointOfContactTelephoneNumber = Utils.ToString(data, 526, 16);
            PointOfContactJobTitle = Utils.ToString(data, 542, 40);
            ParentCompanyNameAddress.Deserialize(data.Substring(582, 175));
            ParentCompanyRegisteredNumber = Utils.ToString(data, 757, 8);
            ParentCompanyTelephoneNumber = Utils.ToString(data, 765, 16);
            ParentCompanyVATNumber = Utils.ToString(data, 781, 9);
            PreviousNameandAddress.Deserialize(data.Substring(790, 175));
            ProprietorPartnerDirectororOtherFlag = Utils.ToString(data, 1065, 1);
            SignatoryontheAccountFlag = Utils.ToString(data, 1066, 1);
            ShareholdersFlag = Utils.ToString(data, 1067, 1);
            CountryofRegistration = Utils.ToString(data, 1068, 50);
            DateofBirth = Utils.ToDate(data, 1118, 8);
            ProprietorsDirectorsGuarantee = Utils.ToString(data, 1126, 1);
            ProprietorsDirectorsGuaranteeCancelledDischarged = Utils.ToString(data, 1127, 1);
            AccountType = Utils.ToInt32(data, 1225, 2);
            StartDateofAgreement = Utils.ToDate(data, 1227, 8);
            CloseDateofAgreement = Utils.ToDate(data, 1235, 8);
            MonthlyPayment = Utils.ToDecimal(data, 1243, 6);
            RepaymentPeriod = Utils.ToInt32(data, 1249, 3);
            CurrentBalance = Utils.ToDecimal(data, 1252, 10);
            CreditBalanceIndicator = Utils.ToString(data, 1262, 1);
            AccountStatus = Utils.ToString(data, 1263, 1);
            SpecialInstructionIndicator = Utils.ToString(data, 1264, 1);
            CreditLimit = Utils.ToDecimal(data, 1265, 7);
            FlagSettings = Utils.ToString(data, 1272, 1);
            Debenture = Utils.ToString(data, 1273, 1);
            MortgageFlags = Utils.ToString(data, 1274, 1);
            AirtimeStatusFlag = Utils.ToString(data, 1275, 1);
            TransferredtoCollectionAccountFlag = Utils.ToString(data, 1276, 1);
            BalanceType = Utils.ToString(data, 1277, 1);
            CreditTurnover = Utils.ToDecimal(data, 1278, 9);
            PrimaryAccountIndicator = Utils.ToString(data, 1287, 1);
            DefaultSatisfactionDate = Utils.ToDate(data, 1288, 8);
            RejectionFlag = Utils.ToString(data, 1296, 1);
            BankerDetailsSortCode = Utils.ToDecimal(data, 1297, 6);
            OriginalDefaultBalance = Utils.ToDecimal(data, 1303, 7);
            PaymentFrequencyIndicator = Utils.ToString(data, 1310, 1);
            NumberofCreditCardsissued = Utils.ToDecimal(data, 1311, 5);
            PaymentAmount = Utils.ToDecimal(data, 1316, 6);
            PaymentCreditIndicator = Utils.ToString(data, 1322, 1);
            PreviousStatementBalance = Utils.ToDecimal(data, 1323, 6);
            PreviousStatementBalanceIndicator = Utils.ToString(data, 1329, 1);
            NumberofCashAdvances = Utils.ToDecimal(data, 1330, 2);
            ValueofCashAdvances = Utils.ToDecimal(data, 1332, 6);
            PaymentCode = Utils.ToString(data, 1338, 1);
            PromotionActivityFlag = Utils.ToString(data, 1339, 1);
            PaymentType = Utils.ToString(data, 1340, 1);
            NewAccountNumber = Utils.ToString(data, 1341, 19);
            NewProprietorPartnerDirectorNumber = Utils.ToString(data, 1360, 4);
        }
    }
}
