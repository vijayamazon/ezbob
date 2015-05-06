using EzBob.Web.Models.Repository;
using StructureMap;

namespace EzBob.Web.Models.Strings
{
	public static class DbStrings
	{
		private static IDbStringRepository Strings { get { return ObjectFactory.GetInstance<IDbStringRepository>(); } }

		/// <summary>
		/// Fetches string like This email adress already exists in our system.  Please try to log-in or request new password.
		/// </summary>
		public static string EmailAddressAlreadyExists { get { return Strings.GetByKey("EmailAddressAlreadyExists"); } }
		/// <summary>
		/// Fetches string like This email is already registered. For more information contact support: 
		/// </summary>
		public static string EmailAddressAlreadyRegisteredInOtherOrigin { get { return Strings.GetByKey("EmailAddressAlreadyRegisteredInOtherOrigin"); } }
		/// <summary>
		/// Fetches string like Maximum answer length is 199 characters.
		/// </summary>
		public static string MaximumAnswerLengthExceeded { get { return Strings.GetByKey("MaximumAnswerLengthExceeded"); } }
		/// <summary>
		/// Fetches string like Invalid code.
		/// </summary>
		public static string InvalidMobileCode { get { return Strings.GetByKey("InvalidMobileCode"); } }
		/// <summary>
		/// Fetches string like Failed to create user.
		/// </summary>
		public static string UserCreationFailed { get { return Strings.GetByKey("UserCreationFailed"); } }
		/// <summary>
		/// Fetches string like Customer id is not provided
		/// </summary>
		public static string CustomeIdNotProvided { get { return Strings.GetByKey("CustomeIdNotProvided"); } }
		/// <summary>
		/// Fetches string like This is not a valid email adress
		/// </summary>
		public static string NotValidEmailAddress { get { return Strings.GetByKey("NotValidEmailAddress"); } }
		/// <summary>
		/// Fetches string like Please enter a password that is 6 digits long.
		/// </summary>
		public static string PasswordPolicyCheck { get { return "Please enter a password that is 6 characters or more."; } }
		/// <summary>
		/// Fetches string like please re-enter password, doesn't match
		/// </summary>
		public static string PasswordDoesNotMatch { get { return Strings.GetByKey("PasswordDoesNotMatch"); } }
		/// <summary>
		/// Fetches string like Your store already exists in our system under a different user
		/// </summary>
		public static string StoreAlreadyExistsInDb { get { return Strings.GetByKey("StoreAlreadyExistsInDb"); } }
		/// <summary>
		/// Fetches string like Congratulations.  Your store was added successfully.
		/// </summary>
		public static string StoreAdded { get { return Strings.GetByKey("StoreAdded"); } }
		/// <summary>
		/// Fetches string like This PayPal account already exists in our database under a different user.
		/// </summary>
		public static string AccountAlreadyExixtsInDb { get { return Strings.GetByKey("AccountAlreadyExixtsInDb"); } }
		/// <summary>
		/// Fetches string like You've already added this PayPal account
		/// </summary>
		public static string PayPalAddedByYou { get { return Strings.GetByKey("PayPalAddedByYou"); } }
		/// <summary>
		/// Fetches string like You've already added this store. If you have another store please add it now. 
		/// </summary>
		public static string StoreAddedByYou { get { return Strings.GetByKey("StoreAddedByYou"); } }
		/// <summary>
		/// Fetches string like You've already added this account. If you have another account please add it now. 
		/// </summary>
		public static string AccountAddedByYou { get { return Strings.GetByKey("AccountAddedByYou"); } }
		/// <summary>
		/// Fetches string like You are now opening a FREE, SECURE, and PERSONAL account and minutes away from receiving monies.
		/// </summary>
		public static string QuickSignUpTopNotification { get { return Strings.GetByKey("QuickSignUpTopNotification"); } }
		/// <summary>
		/// Fetches string like You are under no obligation, there is no cost to this service.  EZBOB uses state of the art security : all your data is kept secure!!!
		/// </summary>
		public static string QuickSignUpSecurityStatement { get { return Strings.GetByKey("QuickSignUpSecurityStatement"); } }
		/// <summary>
		/// Fetches string like Need Help?  Try live chat, email, or call ####
		/// </summary>
		public static string QuickSignUpNeedHelp { get { return Strings.GetByKey("QuickSignUpNeedHelp"); } }
		/// <summary>
		/// Fetches string like <ul><li>What is EZBOB?</li><li>How long is the Process?</li><li>When will I receive my money?</li></ul>
		/// </summary>
		public static string QuickSignUpCommonQuestions { get { return Strings.GetByKey("QuickSignUpCommonQuestions"); } }
		/// <summary>
		/// Fetches string like As part of the appilcation process, you are opening your own, private, EZBOB account.   The entire applicatin process takes no more than 10 minutes to complete. 
		/// </summary>
		public static string QuickSignUpNotifications { get { return Strings.GetByKey("QuickSignUpNotifications"); } }
		/// <summary>
		/// Fetches string like Security and Privacy are  paramount at EZBOB. Your information is not shared with any 3rd party.
		/// </summary>
		public static string StoreInfoSecurityStatement { get { return Strings.GetByKey("StoreInfoSecurityStatement"); } }
		/// <summary>
		/// Fetches string like <ul><li>Does EZBOB have access to my store?</li><li>Benefits of adding a store?</li><li>What information does EZBOB collect?</li></ul>
		/// </summary>
		public static string StoreInfoCommonQuestions { get { return Strings.GetByKey("StoreInfoCommonQuestions"); } }
		/// <summary>
		/// Fetches string like We have created a simple visual tutorial to guide you through the process.
		/// </summary>
		public static string AmazonTutorial { get { return Strings.GetByKey("AmazonTutorial"); } }
		/// <summary>
		/// Fetches string like If I add another store how much more money can I get?  Does EZBOB have access to my store?
		/// </summary>
		public static string AmazonCommonQuestions { get { return Strings.GetByKey("AmazonCommonQuestions"); } }
		/// <summary>
		/// Fetches string like EZBOB never sees your store passwords.  We have read only access to your details.
		/// </summary>
		public static string AmazonSecurityStatement { get { return Strings.GetByKey("AmazonSecurityStatement"); } }
		/// <summary>
		/// Fetches string like EZBOB does not have access to your passwords and cannot alter your store data in anyway.  EZBOB analyzes our online store data in order to make you a financing offer.
		/// </summary>
		public static string StoreNotification { get { return Strings.GetByKey("StoreNotification"); } }
		/// <summary>
		/// Fetches string like We have created a simple visual tutorial to guide you through the process.
		/// </summary>
		public static string EbayTutorial { get { return Strings.GetByKey("EbayTutorial"); } }
		/// <summary>
		/// Fetches string like Congratulations. Your payment account was addded successfully.
		/// </summary>
		public static string PaymentAccountAdded { get { return Strings.GetByKey("PaymentAccountAdded"); } }
		/// <summary>
		/// Fetches string like EZBOB finances based on your online business.  Add your best store first.
		/// </summary>
		public static string StoreInfoTopNotification { get { return Strings.GetByKey("StoreInfoTopNotification"); } }
		/// <summary>
		/// Fetches string like Add more stores get more money!
		/// </summary>
		public static string StoreInfoAddMore { get { return Strings.GetByKey("StoreInfoAddMore"); } }
		/// <summary>
		/// Fetches string like Now we need your payment details so we can send you money!
		/// </summary>
		public static string NoPayPalOrBankAccount { get { return Strings.GetByKey("NoPayPalOrBankAccount"); } }
		/// <summary>
		/// Fetches string like We need your Bank Account details so we can send you money!
		/// </summary>
		public static string NoBankAccount { get { return Strings.GetByKey("NoBankAccount"); } }
		/// <summary>
		/// Fetches string like The registration process is almost completed.
		/// </summary>
		public static string AccountsAdded { get { return Strings.GetByKey("AccountsAdded"); } }
		/// <summary>
		/// Fetches string like Since you are an Ebay seller we require your PayPal details.
		/// </summary>
		public static string NoPayPalAccount { get { return Strings.GetByKey("NoPayPalAccount"); } }
		/// <summary>
		/// Fetches string like Since you are an ebay seller, you must fill out your PayPal details.
		/// </summary>
		public static string PayPalTopNotification { get { return Strings.GetByKey("PayPalTopNotification"); } }
		/// <summary>
		/// Fetches string like We have created a simple visual tutorial to guide you through the process.
		/// </summary>
		public static string PayPalTutorial { get { return Strings.GetByKey("PayPalTutorial"); } }
		/// <summary>
		/// Fetches string like What happens if I don't have / fill out my PayPal details?  What does EZBOB do with my PayPal account details?  Do I get more money if I add my Paypal
		/// </summary>
		public static string PayPalQuestions { get { return Strings.GetByKey("PayPalQuestions"); } }

		/// <summary>
		/// Fetches string for marketing Box in side bar according to user's step
		/// </summary>
		public static string MarketingDefault { get { return Strings.GetByKey("MarketingDefault"); } }

		public static string MarketingWizardStep1 { get { return Strings.GetByKey("MarketingWizardStep1"); } }
		public static string MarketingWizardStep2 { get { return Strings.GetByKey("MarketingWizardStep2"); } }
		public static string MarketingWizardStep3 { get { return Strings.GetByKey("MarketingWizardStep3"); } }
		public static string MarketingWizardStep35 { get { return Strings.GetByKey("MarketingWizardStep35"); } }
		public static string MarketingWizardStep4 { get { return Strings.GetByKey("MarketingWizardStep4"); } }

		public static string MarketingDashboard { get { return Strings.GetByKey("MarketingDashboard"); } }
		public static string MarketingDashboardMakePayment { get { return Strings.GetByKey("MarketingDashboardMakePayment"); } }
		public static string MarketingDashboardGetCash { get { return Strings.GetByKey("MarketingDashboardGetCash"); } }
		public static string MarketingDashboardLoanDetails { get { return Strings.GetByKey("MarketingDashboardLoanDetails"); } }
		public static string MarketingDashboardAccountActivity { get { return Strings.GetByKey("MarketingDashboardAccountActivity"); } }
		public static string MarketingDashboardYourDetails { get { return Strings.GetByKey("MarketingDashboardYourDetails"); } }
		public static string MarketingDashboardYourStores { get { return Strings.GetByKey("MarketingDashboardYourStores"); } }
		public static string MarketingDashboardPaymentAccounts { get { return Strings.GetByKey("MarketingDashboardPaymentAccounts"); } }
		public static string MarketingDashboardPayEarly { get { return Strings.GetByKey("MarketingDashboardPayEarly"); } }
		public static string MarketingDashboardSettings { get { return Strings.GetByKey("MarketingDashboardSettings"); } }
	};

}