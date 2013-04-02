using System;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.ApplicationCreator
{
    public interface IAppCreator
    {
        void AfterSignup(User user, string address);
        void CashTransfered(User user, string firstName, decimal? cashAmount, decimal setUpFee);
        void ThreeInvalidAttempts(User user, string firstName, string password);
        void PasswordChanged(User user, string firstName, string password);
        void PasswordRestored(User user, string emailTo, string firstName,  string password);
        void EbayAdded(Customer user, int umi);
        void PayPalAdded(Customer user, int umi);
        void Evaluate(User user, bool isUnderwriterForced = false);
        void EvaluateWithIdHubCustomAddress(User user, int checkType, string houseNumber, string houseName, string street,
                                       string district, string town, string county, string postcode, string bankAccount, string sortCode);
        void AmazonAdded(Customer user, int umi);
        void GetCashFailed(User user, string firstName);
        void PayEarly(User user,  DateTime date, decimal? amount, string firstName );
        void PayPointNameValidationFailed(string cardHodlerName, User user, Customer customer);
        void ApprovedUser(User user, Customer customer, decimal? loanAmount);
        void RejectUser(User user, string email, int userId, string firstName);
        void MoreAMLInformation(User user, string email, int userId, string firstName);
        void MoreAMLandBWAInformation(User user, string email, int userId, string firstName);
        void MoreBWAInformation(User user, string email, int userId, string firstName);
        void SendEmailVerification(User user, Customer customer, string address);
        void PayPointAddedByUnderwriter(User user, Customer customer, string cardno);
        void UpdateAllMarketplaces(Customer customer);
        void FeeAdded(Customer customer, decimal? feeAmount);
        void EmailRolloverAdded(Customer customer, decimal amount, DateTime expireDate);
        void RenewEbayToken(Customer customer, string marketplaceName, string url);
        void Escalated(Customer customer);
        void CAISNOUpload(User customer);
    }
}