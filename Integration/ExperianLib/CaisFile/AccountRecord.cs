using System;
using System.Globalization;
using System.Text;

namespace ExperianLib.CaisFile
{
    public class NameAndAddressData
    {
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Postcode { get; set; }

        public string Serialize()
        {
            var ret = new StringBuilder();
            ret.Append(Utils.GetPaddingString(Name, 39, true));
            ret.Append(Utils.GetPaddingString(AddressLine1, 32, true));
            ret.Append(Utils.GetPaddingString(AddressLine2, 32, true));
            ret.Append(Utils.GetPaddingString(AddressLine3, 32, true));
            ret.Append(Utils.GetPaddingString(AddressLine4, 32, true));
            ret.Append(Utils.GetPaddingString(Postcode, 8, true));
            return ret.ToString();
        }

        public void Deserialize(string data)
        {
            if (data.Length != 175) throw new Exception("Invalid string length, must be 175 characters");

            Name = Utils.ToString(data, 0, 39);
            AddressLine1 = Utils.ToString(data, 39, 32);
            AddressLine2 = Utils.ToString(data, 71, 32);
            AddressLine3 = Utils.ToString(data, 103, 32);
            AddressLine4 = Utils.ToString(data, 135, 32);
            Postcode = Utils.ToString(data, 167, 8);
        }
    }

    public class AccountRecord
    {
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public int MonthlyPayment { get; set; }
        public int RepaymentPeriod { get; set; }
        public int CurrentBalance { get; set; }
        public string CreditBalanceIndicator { get; set; }
        public string AccountStatus { get; set; }
        public string SpecialInstructionIndicator { get; set; }
        public decimal PaymentAmount { get; set; }
        public string CreditPaymentIndicator { get; set; }
        public int PreviousStatementBalance { get; set; }
        public string PreviousStatementBalanceIndicator { get; set; }
        public int NumberCashAdvances { get; set; }
        public int ValueCashAdvances { get; set; }
        public string PaymentCode { get; set; }
        public string PromotionActivityFlag { get; set; }
        public string TransientAssociationFlag { get; set; }
        public string AirtimeFlag { get; set; }
        public string FlagSettings { get; set; }
        public NameAndAddressData NameAndAddress { get; set; }
        public int CreditLimit { get; set; }
        public DateTime? DateBirth { get; set; }
        public string TransferredCollectionAccountFlag { get; set; }
        public string BalanceType { get; set; }
        public decimal CreditTurnover { get; set; }
        public string PrimaryAccountIndicator { get; set; }
        public DateTime? DefaultSatisfactionDate { get; set; }
        public string TransactionFlag { get; set; }
        public int OriginalDefaultBalance { get; set; }
        public string PaymentFrequency { get; set; }
        public string NewAccountNumber { get; set; }

        public string Serialize()
        {
            var ret = new StringBuilder();

            ret.Append(Utils.GetPaddingString(AccountNumber, 20, true));
            ret.Append(Utils.GetPaddingString(AccountType, 2));
            ret.Append(Utils.GetPaddingString(StartDate));
            ret.Append(Utils.GetPaddingString(CloseDate));
            ret.Append(Utils.GetPaddingString(MonthlyPayment, 6));
            ret.Append(Utils.GetPaddingString(RepaymentPeriod, 3));
            ret.Append(Utils.GetPaddingString(CurrentBalance, 7));
            ret.Append(Utils.GetPaddingString(CreditBalanceIndicator, 1));
            ret.Append(Utils.GetPaddingString(AccountStatus, 1));
            ret.Append(Utils.GetPaddingString(SpecialInstructionIndicator, 1));
            ret.Append(Utils.GetPaddingString(String.Empty, 150));
            ret.Append(Utils.GetPaddingString(PaymentAmount, 6));
            ret.Append(Utils.GetPaddingString(CreditBalanceIndicator, 1));
            ret.Append(Utils.GetPaddingString(PreviousStatementBalance, 6));
            ret.Append(Utils.GetPaddingString(PreviousStatementBalanceIndicator, 1));
            ret.Append(Utils.GetPaddingString(NumberCashAdvances, 2));
            ret.Append(Utils.GetPaddingString(ValueCashAdvances, 6));
            ret.Append(Utils.GetPaddingString(PaymentCode, 1));
            ret.Append(Utils.GetPaddingString(PromotionActivityFlag, 1));
            ret.Append(Utils.GetPaddingString(String.Empty, 31));
            ret.Append(Utils.GetPaddingString(TransientAssociationFlag, 1));
            ret.Append(Utils.GetPaddingString(AirtimeFlag, 1));
            ret.Append(Utils.GetPaddingString(FlagSettings, 1));
            ret.Append(NameAndAddress.Serialize());
            ret.Append(Utils.GetPaddingString(CreditLimit, 7));
            ret.Append(Utils.GetPaddingString(DateBirth));
            ret.Append(Utils.GetPaddingString(String.Empty, 1));
            ret.Append(Utils.GetPaddingString(TransferredCollectionAccountFlag, 1));
            ret.Append(Utils.GetPaddingString(BalanceType, 1));
            ret.Append(Utils.GetPaddingString(CreditTurnover, 9));
            ret.Append(Utils.GetPaddingString(PrimaryAccountIndicator, 1));
            ret.Append(Utils.GetPaddingString(DefaultSatisfactionDate));
            ret.Append(Utils.GetPaddingString(TransactionFlag, 1));
            ret.Append(Utils.GetPaddingString(String.Empty, 25));
            ret.Append(Utils.GetPaddingString(OriginalDefaultBalance, 7));
            ret.Append(Utils.GetPaddingString(PaymentFrequency, 1));
            ret.Append(Utils.GetPaddingString(NewAccountNumber, 20, true));
           
            return ret.ToString();
        }

        //-----------------------------------------------------------------------------------
        public void Deserialize(string data)
        {
            if (data.Length != 530) throw new Exception("Invalid string length, must be 530 characters");

            AccountNumber = Utils.ToString(data, 0, 20);
            AccountType = Utils.ToString(data, 20, 2);
            StartDate = Utils.ToDate(data, 22);
            CloseDate = Utils.ToDate(data, 30);
            MonthlyPayment = Utils.ToInt32(data, 38, 6);
            RepaymentPeriod = Utils.ToInt32(data, 44, 3);
            CurrentBalance = Utils.ToInt32(data, 47, 7);
            CreditBalanceIndicator = Utils.ToString(data, 54, 1);
            AccountStatus = Utils.ToString(data, 55, 1);
            SpecialInstructionIndicator = Utils.ToString(data, 56, 1);
            PaymentAmount = Utils.ToDecimal(data, 207, 6);
            CreditPaymentIndicator = Utils.ToString(data, 213, 1);
            PreviousStatementBalance = Utils.ToInt32(data, 214, 6);
            PreviousStatementBalanceIndicator = Utils.ToString(data, 220, 1);
            NumberCashAdvances = Utils.ToInt32(data, 221, 2);
            ValueCashAdvances = Utils.ToInt32(data, 223, 6);
            PaymentCode = Utils.ToString(data, 229, 1);
            PromotionActivityFlag = Utils.ToString(data, 230, 1);
            TransientAssociationFlag = Utils.ToString(data, 262, 1);
            AirtimeFlag = Utils.ToString(data, 263, 1);
            FlagSettings = Utils.ToString(data, 264, 1);
            NameAndAddress = new NameAndAddressData();
            NameAndAddress.Deserialize(Utils.ReabStructure(data, 265, 175));
            CreditLimit = Utils.ToInt32(data, 440, 7);
            DateBirth = Utils.ToDate(data, 447);
            TransferredCollectionAccountFlag = Utils.ToString(data, 456, 1);
            BalanceType = Utils.ToString(data, 457, 1);
            CreditTurnover = Utils.ToDecimal(data, 458, 9);
            PrimaryAccountIndicator = Utils.ToString(data, 467, 1);
            DefaultSatisfactionDate = Utils.ToDate(data, 468);
            TransactionFlag = Utils.ToString(data, 476, 1);
            OriginalDefaultBalance = Utils.ToInt32(data, 502, 7);
            PaymentFrequency = Utils.ToString(data, 509, 1);
            NewAccountNumber = Utils.ToString(data, 510, 20);
        }
    }
}
