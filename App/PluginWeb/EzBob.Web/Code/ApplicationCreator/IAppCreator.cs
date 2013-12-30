namespace EzBob.Web.Code.ApplicationCreator
{
	using System;
	using ApplicationMng.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EzServiceReference;

	public interface IAppCreator
    {
        void AfterSignup(User user, string address);
		void GenerateMobileCode(string mobilePhone);
		bool ValidateMobileCode(string mobilePhone, string mobileCode);
		WizardConfigsActionResult GetWizardConfigs();
        void CashTransfered(User user, string firstName, decimal cashAmount, decimal setUpFee, int loanId);
        void ThreeInvalidAttempts(User user, string firstName, string password);
        void PasswordChanged(User user, string firstName, string password);
        void PasswordRestored(User user, string emailTo, string firstName,  string password);
        void CustomerMarketPlaceAdded(Customer user, int umi);
        void Evaluate(int underwriterId, User user, NewCreditLineOption runNewCreditLine, int avoidAutomaticDescison, bool isUnderwriterForced = false);
        void EvaluateWithIdHubCustomAddress(int underwriterId, User user, int checkType, string houseNumber, string houseName, string street,
                                       string district, string town, string county, string postcode, string bankAccount, string sortCode, int avoidAutomaticDescison);
        void GetCashFailed(User user, string firstName);
        void TransferCashFailed(User user, string firstName);
        void PayEarly(User user,  DateTime date, decimal? amount, string firstName, string refNum );
        void PayPointNameValidationFailed(string cardHodlerName, User user, Customer customer);
        void ApprovedUser(User user, Customer customer, decimal? loanAmount);
        void RejectUser(User user, string email, int userId, string firstName);
        void MoreAMLInformation(User user, string email, int userId, string firstName);
        void MoreAMLandBWAInformation(User user, string email, int userId, string firstName);
        void MoreBWAInformation(User user, string email, int userId, string firstName);
        void SendEmailVerification(User user, Customer customer, string address);
        void PayPointAddedByUnderwriter(User user, Customer customer, string cardno);
        void UpdateAllMarketplaces(Customer customer);
        void EmailRolloverAdded(Customer customer, decimal amount, DateTime expireDate);
        void RenewEbayToken(Customer customer, string marketplaceName, string url);
        void Escalated(Customer customer);
        void CAISGenerate(User user);
        void CAISUpdate(User user, int caisId);
        void EmailUnderReview(User user, string firstName, string email);
	    void FraudChecker(User user);
        void RequestCashWithoutTakenLoan(Customer customer, string dashboard);
        void LoanFullyPaid(Loan loan);
    }
}