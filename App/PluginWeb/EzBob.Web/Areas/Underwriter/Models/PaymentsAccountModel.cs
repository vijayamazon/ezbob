using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Models;
using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class PaymentsAccountModel
    {
        public List<PayPalModel> PayPalAccounts
        {
            get { return _payPalAccounts; }
        }

        public List<BankAccountModel> BankAccounts
        {
            get { return _bankAccounts; }
        }

        public List<PayPointCardModel> PayPointCards
        {
            get { return _paypointCards; }
        }

        public BankAccountModel CurrentBankAccount { get; set; }

        private readonly List<PayPalModel> _payPalAccounts = new List<PayPalModel>();
        private readonly List<BankAccountModel> _bankAccounts = new List<BankAccountModel>();
        private readonly List<PayPointCardModel> _paypointCards = new List<PayPointCardModel>();
    }

    public class PaymentAccountModel 
    {
        public string PaymentAccount { get; set; }
        public string AccountId { get; set; }
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal TurnOver { get; set; }
    }

    public class BankAccountModel
    {
        public string AccountId { get; set; }

        public string Type { get; set; }
        
        public int Id { get; set; }

        public string Bank { get; set; }

        public string BankBIC { get; set; }

        public string Branch { get; set; }

        public string BranchBIC { get; set; }

        public string ContactAddressLine1 { get; set; }

        public string ContactAddressLine2 { get; set; }

        public string ContactPostTown { get; set; }

        public string ContactPostcode { get; set; }

        public string ContactPhone { get; set; }

        public string ContactFax { get; set; }

        public bool FasterPaymentsSupported { get; set; }

        public bool CHAPSSupported { get; set; }

        public string SortCode { get; set; }

        public string IBAN { get; set; }

        public bool IsDirectDebitCapable { get; set; }

        public string StatusInformation { get; set; }

        public string BankAccount { get; set; }

        public string BWAResult { get; set; }

        public static BankAccountModel FromCard(CardInfo card)
        {
            if (card == null) return null;
            return new BankAccountModel()
            {
                    Bank = card.Bank,
                    BankBIC = card.BankBIC,
                    Id = card.Id,
                    IBAN = card.IBAN,
                    Branch = card.Branch,
                    BranchBIC = card.BranchBIC,
                    SortCode = card.SortCode,
                    CHAPSSupported = card.CHAPSSupported,
                    ContactAddressLine1 = card.ContactAddressLine1,
                    ContactAddressLine2 = card.ContactAddressLine2,
                    ContactFax = card.ContactFax,
                    ContactPhone = card.ContactPhone,
                    ContactPostTown = card.ContactPostTown,
                    ContactPostcode = card.ContactPostcode,
                    FasterPaymentsSupported = card.FasterPaymentsSupported,
                    IsDirectDebitCapable = card.IsDirectDebitCapable,
                    StatusInformation = card.StatusInformation,
                    BankAccount = card.BankAccount,
                    Type = card.Type.ToString(),
                    BWAResult = card.BWAResult
                };
        }
    }
}
