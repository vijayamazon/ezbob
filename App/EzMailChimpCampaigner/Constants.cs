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
            ThreeDays,
            Week
        }

        public const string GetFirstStepCustomersSp = "GetFirstStepCustomers '{0}','{1}'";
        public const string GetSecondStepCustomersSp = "GetSecondStepCustomers '{0}','{1}'";
        public const string GetLastStepCustomersSp = "GetLastStepCustomers '{0}','{1}'";

        public const string FromEmail = "customercare@ezbob.com";
        public const string FromEmailName = "EZBOB";

        public const string EzbobCustomersListId = "0371913807";

        public const string FirstStepCustomersDayAfterSubject = "Fund your business with EZBOB";
        public const string FirstStepCustomersThreeDaysSubject = "You are less than 10 minutes away from cash";
        public const string FirstStepCustomersWeekSubject = "Get your business financed today";

        public const string SecondStepCustomersDayAfterSubject = "EZBOB has a £*|LOANOFFER|* loan to offer you";
        public const string SecondStepCustomersThreeDaysSubject = "You're pre-qualified for approximately £*|LOANOFFER|*";
        public const string SecondStepCustomersWeekSubject = "Your business is eligible for a £*|LOANOFFER|* loan";

        public const string LastStepCustomersDayAfterSubject = "You were recently approved for a £*|LOANOFFER|* loan!";
        public const string LastStepCustomersThreeDaysSubject = "EZBOB has £*|LOANOFFER|* available for you today";
        public const string LastStepCustomersWeekSubject = "EZBOB helps your business to grow with a £*|LOANOFFER|* loan";

        public const string FirstStepCustomersTitle = "EZBOB Registered Email";
        public const string SecondStepCustomersTitle = "EZBOB Registered Store";
        public const string LastStepCustomersTitle = "EZBOB Didnt Take Offer";

        public const int FirstStepCustomersDayAfterTemplateId = 44149;
        public const int SecondStepCustomersDayAfterTemplateId = 39833;
        public const int LastStepCustomersDayAfterTemplateId = 39861;

        public const int FirstStepCustomersThreeDaysTemplateId = 39813;
        public const int SecondStepCustomersThreeDaysTemplateId = 44237;
        public const int LastStepCustomersThreeDaysTemplateId = 48169;

        public const int FirstStepCustomersWeekTemplateId = 44165;
        public const int SecondStepCustomersWeekTemplateId = 44245;
        public const int LastStepCustomersWeekTemplateId = 48149;

        public const string SignUpProcessGroupId = "8209";
        public const string SignUpProcessGroupName = "SignUpProcess";
        public const string EmailField = "EMAIL";
        public const string EmailTypeField = "EMAIL_TYPE";
        public const string FirstNameField = "FNAME";
        public const string LoanOfferField = "LOANOFFER";
        public const string DayAfterConditionField = "DAYAFTER";
        public const string ThreeDaysConditionField = "THREEDAYS";
        public const string WeekConditionField = "WEEK";

        public const string GoogleAnalyticsKie = "google";
        public const string GoogleAnalyticsValue = "UA-32583191";

        public static string[] Conditions = { DayAfterConditionField, ThreeDaysConditionField, WeekConditionField };
    }
}
