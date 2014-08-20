namespace EzMailChimpCampaigner
{
	public static class Constants
	{
		public enum CampaignsType
		{
			OnlyRegisteredEmail,
			OnlyRegisteredStore,
			DidntTakeLoan,
		}

		public enum ConditionType
		{
			DayAfter,
			//ThreeDays,
			Week,
			TwoWeeks,
			Month
		}

		public const string GetFirstStepCustomersSp = "GetFirstStepCustomers";
		public const string GetSecondStepCustomersSp = "GetSecondStepCustomers";
		public const string GetLastStepCustomersSp = "GetLastStepCustomers";

		public const string FromEmail = "customercare@ezbob.com";
		public const string FromEmailName = "EZBOB";

		public const string EzbobCustomersListId = "0371913807";

		public const string FirstStepCustomersDayAfterSubject = "Jumpstart your business get up to £40K for one year";
		//public const string FirstStepCustomersThreeDaysSubject = "You are less than 10 minutes away from cash";
		public const string FirstStepCustomersWeekSubject = "Ezbob offers £100 rebate";

		public const string SecondStepCustomersDayAfterSubject = "Your business pre-qualifies for £*|LOANOFFER|* in funding";
		public const string SecondStepCustomersWeekSubject = "Complete your application and get £100 rebate from Ezbob";
		public const string SecondStepCustomersTwoWeeksSubject = "Reminder: you are pre-qualified for £*|LOANOFFER|* up to 12 months";
		public const string SecondStepCustomersMonthSubject = "Take advantage: EZBOB pre-qualified you for £*|LOANOFFER|*";

		public const string LastStepCustomersDayAfterSubject = "*|FNAME|* £*|LOANOFFER|* in funding is waiting for you!";
		public const string LastStepCustomersWeekSubject = "*|FNAME|* £*|LOANOFFER|* is just a click away";
		public const string LastStepCustomersTwoWeeksSubject = "*|FNAME|* Join the thousands of E-retailers that have benefited";
		public const string LastStepCustomersMonthSubject = "Jumpstart your business with *|LOANOFFER|* from EZBOB";

		public const string FirstStepCustomersTitle = "EZBOB Registered Email";
		public const string SecondStepCustomersTitle = "EZBOB Registered Store";
		public const string LastStepCustomersTitle = "EZBOB Didnt Take Offer";

		public const int FirstStepCustomersDayAfterTemplateId = 90669;
		//public const int SecondStepCustomersDayAfterTemplateId = 66273;
		public const int LastStepCustomersDayAfterTemplateId = 90665;

		//public const int FirstStepCustomersThreeDaysTemplateId = 39813;
		//public const int SecondStepCustomersThreeDaysTemplateId = 44237;
		//public const int LastStepCustomersThreeDaysTemplateId = 48169;

		//public const int FirstStepCustomersWeekTemplateId = 77577;
		//public const int SecondStepCustomersWeekTemplateId = 66269;
		public const int LastStepCustomersWeekTemplateId = 90661;

		//public const int SecondStepCustomersTwoWeeksTemplateId = 66285;
		public const int LastStepCustomersTwoWeeksTemplateId = 90657;

		//public const int SecondStepCustomersMonthTemplateId = 66289;
		public const int LastStepCustomersMonthTemplateId = 90649;

		public const string SignUpProcessGroupId = "8209";
		public const string SignUpProcessGroupName = "SignUpProcess";
		public const string EmailField = "EMAIL";
		public const string EmailTypeField = "EMAIL_TYPE";
		public const string FirstNameField = "FNAME";
		public const string LoanOfferField = "LOANOFFER";
		public const string DayAfterConditionField = "DAYAFTER";
		public const string ThreeDaysConditionField = "THREEDAYS";
		public const string WeekConditionField = "WEEK";
		public const string TwoWeeksConditionField = "TWOWEEKS";
		public const string MonthConditionField = "MONTH";

		public const string GoogleAnalyticsKie = "google";
		public const string GoogleAnalyticsValue = "UA-32583191";

		public static string[] Conditions = { DayAfterConditionField, WeekConditionField, TwoWeeksConditionField, MonthConditionField };
	}
}
