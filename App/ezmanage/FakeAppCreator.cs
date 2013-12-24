using System;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.Web.ApplicationCreator;

namespace ezmanage
{
    public class FakeAppCreator : IAppCreator
    {
        public void AfterSignup(User user, string address)
        {
            
        }

        public void CashTransfered(User user, string firstName, decimal cashAmount, decimal setUpFee, int loanId)
        {
            
        }

        public void ThreeInvalidAttempts(User user, string firstName, string password)
        {
            
        }

        public void PasswordChanged(User user, string firstName, string password)
        {
            
        }

        public void PasswordRestored(User user, string emailTo, string firstName, string password)
        {
            
        }

        public void CustomerMarketPlaceAdded(Customer user, int umi)
        {
            
        }

        public void Evaluate(User user, NewCreditLineOption runNewCreditLine, int avoidAutomaticDescison, bool isUnderwriterForced = false)
        {
            
        }

        public void EvaluateWithIdHubCustomAddress(User user, int checkType, string houseNumber, string houseName, string street,
                                                   string district, string town, string county, string postcode, string bankAccount,
                                                   string sortCode, int avoidAutomaticDescison)
        {
            
        }

        public void GetCashFailed(User user, string firstName)
        {
            
        }

        public void TransferCashFailed(User user, string firstName)
        {
            
        }

        public void PayEarly(User user, DateTime date, decimal? amount, string firstName, string refNumber)
        {
            
        }

        public void PayPointNameValidationFailed(string cardHodlerName, User user, Customer customer)
        {
            
        }

        public void ApprovedUser(User user, Customer customer, decimal? loanAmount)
        {
            
        }

        public void RejectUser(User user, string email, int userId, string firstName)
        {
            
        }

        public void MoreAMLInformation(User user, string email, int userId, string firstName)
        {
            
        }

        public void MoreAMLandBWAInformation(User user, string email, int userId, string firstName)
        {
            
        }

        public void MoreBWAInformation(User user, string email, int userId, string firstName)
        {
            
        }

        public void SendEmailVerification(User user, Customer customer, string address)
        {
            
        }

        public void PayPointAddedByUnderwriter(User user, Customer customer, string cardno)
        {
            
        }

        public void UpdateAllMarketplaces(Customer customer)
        {
            
        }

        public void EmailRolloverAdded(Customer customer, decimal amount, DateTime expireDate)
        {
            
        }

        public void RenewEbayToken(Customer customer, string marketplaceName, string url)
        {
            
        }

        public void Escalated(Customer customer)
        {
            
        }

        public void CAISGenerate(User user)
        {
            
        }

        public void CAISUpdate(User user, int caisId)
        {
            
        }

        public void EmailUnderReview(User user, string firstName, string email)
        {
        }

        public void RequestCashWithoutTakenLoan(Customer customer, string dashboard)
        {
        }

        public void LoanFullyPaid(Loan loan)
        {
        }


		public void FraudChecker(User user)
		{
			
		}
	}
}