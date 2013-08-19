using System;
using System.Linq;
using System.ServiceModel;
using EZBob.DatabaseLib.Model.Database;
using PostcodeAnywhere.pca2;
using log4net;

namespace PostcodeAnywhere
{
    public class SortCodeChecker : ISortCodeChecker
    {
        private readonly IPostcodeAnywhereConfig _config;
        private readonly ILog log = LogManager.GetLogger("PostcodeAnywhere.SortCodeChecker");

        public SortCodeChecker(IPostcodeAnywhereConfig config)
        {
            _config = config;
        }

        public CardInfo Check(Customer customer, string accountNumber, string sortcode, string bankAccountType)
        {
            var card = new CardInfo() { BankAccount = accountNumber, SortCode = sortcode, Customer = customer, Type = (BankAccountType)Enum.Parse(typeof(BankAccountType), bankAccountType) };

            if (customer.BankAccountValidationInvalidAttempts < Math.Max(_config.MaxBankAccountValidationAttempts , 1))
            {
                try
                {
                    Check(card);
                }
                catch (Exception )
                {
                    customer.BankAccountValidationInvalidAttempts++;
                    throw;
                }                
            }

            customer.CurrentCard = card;

            return card;
        }

        public void Check(CardInfo card)
        {

            if (string.IsNullOrEmpty(card.SortCode))
            {
                throw new ArgumentException("Sort Code");
            }

            if (string.IsNullOrEmpty(card.BankAccount))
            {
                throw new ArgumentException("Account Number");
            }

            BankAccountValidation_Interactive_Validate_v2_00_ArrayOfResults result;
            try
            {
                var myBinding = new BasicHttpBinding();
                var myEndpoint = new EndpointAddress("https://services.postcodeanywhere.co.uk/BankAccountValidation/Interactive/Validate/v2.00/soapnew.ws");

                myBinding.Security.Mode = BasicHttpSecurityMode.Transport;

                var c = new PostcodeAnywhere_SoapClient(myBinding, myEndpoint);

                result = c.BankAccountValidation_Interactive_Validate_v2_00(_config.Key, card.BankAccount, card.SortCode);
            }
            catch (Exception e)
            {
                log.Error(e);
                return;
            }

            if (!result.Any())
            {
                throw new SortCodeNotFoundException();
            }

            var first = result.First();

            if (!first.IsCorrect)
            {
                if (first.StatusInformation == "UnknownSortCode")
                {
                    throw new UnknownSortCodeException();
                }
                if (first.StatusInformation == "InvalidAccountNumber")
                {
                    throw new InvalidAccountNumberException();
                }
                throw new NotValidSortCodeException();
            }

            card.BankBIC = first.BankBIC;
            card.Bank = first.Bank;
            card.Branch = first.Branch;
            card.BranchBIC = first.BranchBIC;
            card.CHAPSSupported = first.CHAPSSupported;
            card.ContactAddressLine1 = first.ContactAddressLine1;
            card.ContactAddressLine2 = first.ContactAddressLine2;
            card.ContactFax = first.ContactFax;
            card.ContactPhone = first.ContactPhone;
            card.ContactPostTown = first.ContactPostTown;
            card.ContactPostcode = first.ContactPostcode;
            card.FasterPaymentsSupported = first.FasterPaymentsSupported;
            card.IBAN = first.IBAN;
            card.IsDirectDebitCapable = first.IsDirectDebitCapable;
            card.StatusInformation = first.StatusInformation;
        }
    }
}
