using System.Collections.Generic;
using System.Linq;

namespace PaymentServices.PayPoint
{
    public class PayPointStatusTranslator
    {
        public static List<string> TranslateStatusCode(string code)
        {
            var result = new List<string>();
            if (code == "A")
            {
                result.Add("Transaction authorised by bank");
                return result;
            }

            if(code == "N")
            {
                result.Add("Transaction not authorised");
                return result;
            }
            if(code == "C")
            {
                result.Add("Communication problem. Trying again later may well work");
                return result;
            }
            if(code == "F")
            {
                result.Add("The PayPoint.net system has detected a fraud condition and rejected the transaction");
                return result;
            }

            if (code[0] != 'P' || code.Length < 3)
            {
                return result;
            }

            var subcodes = code.Substring(2);

            var dict = new Dictionary<char, string>()
                           {
                               {'A', "Pre-bank checks. Amount not supplied or invalid"},
                               {'X', "Pre-bank checks. Not all mandatory parameters supplied"},
                               {'P', "Pre-bank checks. Same payment presented twice"},
                               {'S', "Pre-bank checks. Start date invalid"},
                               {'E', "Pre-bank checks. Expiry date invalid"},
                               {'I', "Pre-bank checks. Issue number invalid"},
                               {'C', "Pre-bank checks. Card number fails LUHN check (the card number is wrong)"},
                               {'T', "Pre-bank checks. Card type invalid - i.e. does not match card number prefix"},
                               {'N', "Pre-bank checks. Customer name not supplied"},
                               {'M', "Pre-bank checks. Merchant does not exist or not registered yet"},
                               {'B', "Pre-bank checks. Merchant account for card type does not exist"},
                               {'D', "Pre-bank checks. Merchant account for this currency does not exist"}, 
                               {'V', "Pre-bank checks. CV2 security code mandatory and not supplied / invalid"},
                               {'R', "Pre-bank checks. Transaction timed out awaiting a virtual circuit. Merchant may not have enough virtual circuits for the volume of business."},
                               {'#', "Pre-bank checks. No MD5 hash / token key set up against account"}
                           };

            result.AddRange(subcodes.Select(c => dict[c]));
            return result;
        }
    }
}