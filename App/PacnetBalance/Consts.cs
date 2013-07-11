using System.Collections.Generic;

namespace PacnetBalance {
	internal class Consts {
		public static string InsertPacNetBalanceSpName = "InsertPacNetBalance";
		public static string InsertPacNetBalanceSpParam1 = "@Date";
		public static string InsertPacNetBalanceSpParam2 = "@Amount";
		public static string InsertPacNetBalanceSpParam3 = "@Fees";
		public static string InsertPacNetBalanceSpParam4 = "@CurrentBalance";
		public static string InsertPacNetBalanceSpParam5 = "@IsCredit";
		public static string InsertPacNetBalanceSpDateFormat = "yyyy-MM-dd";

		public static string GetConfigsSpName = "GetPacnetAgentConfigs";

		public static string MailSearchCondition = "UNSEEN";
		public static List<string> AttachmentContentTypes = new List<string>(new[] { "application/pdf", "application/x-pdf" });
	} // class Consts
} // namespace
