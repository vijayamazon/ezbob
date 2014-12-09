using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperianLib.CaisFile
{
    public class BusinessHeader
    {
        public decimal SourceCode { get; set; }
        public DateTime DateOfCreation { get; set; }
        public string CompanyPortfolioName { get; set; }
        public string CreditCardBehaviouralSharingFlag { get; set; }

        //-----------------------------------------------------------------------------------
        public string Serialize()
        {
            var ret = new StringBuilder();

            ret.Append(Utils.GetPaddingString("HEADER", 20, false));
            ret.Append(Utils.GetPaddingString(SourceCode, 3, false));
            ret.Append(Utils.GetPaddingString(DateOfCreation, 8, false));
            ret.Append(Utils.GetPaddingString(CompanyPortfolioName, 30, true));
            ret.Append(Utils.GetPaddingString(" ", 20, false));
            ret.Append(Utils.GetPaddingString("CAISCOM2", 8, true));
            ret.Append(Utils.GetPaddingString(CreditCardBehaviouralSharingFlag, 1, false));
            ret.Append(Utils.GetPaddingString(" ", 1274, false));

            return ret.ToString();
        }

        //-----------------------------------------------------------------------------------
        public void Deserialize(string data)
        {
            if (data.Length != 1364) throw new Exception("Invalid string length, must be 1364 characters");

            SourceCode = Utils.ToDecimal(data, 20, 3);
            DateOfCreation = Utils.ToDate(data, 23, 8);
            CompanyPortfolioName = Utils.ToString(data, 31, 30);
            CreditCardBehaviouralSharingFlag = Utils.ToString(data, 89, 1);

        }
    }
}
