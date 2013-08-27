namespace EzAutoResponder
{
	class Const
	{
		public static string GetConfigsSpName = "GetPacnetAgentConfigs";
		public static string MailSearchCondition = "UNSEEN";
		public static string GetLastAutoresponderDateSpName = "GetLastAutoresponderDate";
		public static string InsertAutoresponderLogSpName = "InsertAutoresponderLog";
		public static string EmailSpParam = "Email";
		public static string NameSpParam = "Name";
		public static int HourAfter = 19;
		public static int HourBefore = 6; //Sending autoresponse in the range of 19:00-06:00
		public static int ThreeDays = -3; //sending autoresponse once in three days
		public static string MandrillAutoResponseTemplate = "AutoresponderTest";

	}
}
