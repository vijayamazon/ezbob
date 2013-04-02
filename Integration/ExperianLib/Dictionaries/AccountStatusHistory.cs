using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperianLib.Dictionaries
{
    public class AccountStatusDictionary
    {
        protected static Dictionary<string, string> AccountStatuses;
        protected static Dictionary<string, string> AccountDetailedStatuses;

        protected static void Initialize()
        {
            if (AccountStatuses == null)
                AccountStatuses = new Dictionary<string, string>();
            AccountStatuses.Clear();

            if (AccountDetailedStatuses == null)
                AccountDetailedStatuses = new Dictionary<string, string>();
            AccountDetailedStatuses.Clear();

            AccountStatuses.Add("0", "OK");
            AccountStatuses.Add("1", "30");
            AccountStatuses.Add("2", "60");
            AccountStatuses.Add("3", "90");
            AccountStatuses.Add("4", "120");
            AccountStatuses.Add("5", "150");
            AccountStatuses.Add("6", "180");
            AccountStatuses.Add("8", "Def");
            AccountStatuses.Add("9", "Bad");
            AccountStatuses.Add("S", "Slow");
            AccountStatuses.Add("U", "U");
            AccountStatuses.Add("D", "Dorm");
            AccountStatuses.Add("?", "?");

            AccountDetailedStatuses.Add("0", "0 days");
            AccountDetailedStatuses.Add("1", "30 days");
            AccountDetailedStatuses.Add("2", "60 days");
            AccountDetailedStatuses.Add("3", "90 days");
            AccountDetailedStatuses.Add("4", "120 days");
            AccountDetailedStatuses.Add("5", "150 days");
            AccountDetailedStatuses.Add("6", "180 days");
            AccountDetailedStatuses.Add("8", "Default");
            AccountDetailedStatuses.Add("9", "Bad Debt");
            AccountDetailedStatuses.Add("S", "Slow Payer");
            AccountDetailedStatuses.Add("U", "Unclassified");
            AccountDetailedStatuses.Add("D", "Dormant");
            AccountDetailedStatuses.Add("?", "Unknown");
        
        }

        public static string GetAccountStatusString(string accStatusIndicator)
        {
            if (AccountStatuses == null)
            {
                AccountStatuses = new Dictionary<string, string>();
                Initialize();
            }
            return AccountStatuses.ContainsKey(accStatusIndicator)
                    ? AccountStatuses[accStatusIndicator]
                    : (accStatusIndicator ?? string.Empty);
        }

        public static string GetDetailedAccountStatusString(string accStatusIndicator)
        {
            if (AccountDetailedStatuses == null)
            {
                AccountDetailedStatuses = new Dictionary<string, string>();
                Initialize();
            }
            return AccountDetailedStatuses.ContainsKey(accStatusIndicator)
                    ? AccountDetailedStatuses[accStatusIndicator]
                    : (accStatusIndicator ?? string.Empty);
        }
    }
}
