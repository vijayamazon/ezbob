using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperianLib.Dictionaries
{
    public class PaymentFrequencyDictionary
    {
        protected static Dictionary<string, string> PaymentFrequencyIndicators;

        protected static void Initialize()
        {
            if (PaymentFrequencyIndicators == null)
                PaymentFrequencyIndicators = new Dictionary<string, string>();
            PaymentFrequencyIndicators.Clear();

            PaymentFrequencyIndicators.Add("W", "Weekly");
            PaymentFrequencyIndicators.Add("F", "Fortnightly");
            PaymentFrequencyIndicators.Add("M", "Monthly");
            PaymentFrequencyIndicators.Add("Q", "Quarterly");
            PaymentFrequencyIndicators.Add("A", "Annually");
            PaymentFrequencyIndicators.Add("P", "Periodically");

        }

        public static string GetPaymentFrequency(string paymentFrequencyIndicator)
        {
            if (PaymentFrequencyIndicators == null)
            {
                PaymentFrequencyIndicators = new Dictionary<string, string>();
                Initialize();
            }
            return PaymentFrequencyIndicators.ContainsKey(paymentFrequencyIndicator) 
                    ? PaymentFrequencyIndicators[paymentFrequencyIndicator] 
                    : (paymentFrequencyIndicator ?? string.Empty);
        }
    }
}
