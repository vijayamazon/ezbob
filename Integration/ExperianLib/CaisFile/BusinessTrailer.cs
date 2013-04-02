using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperianLib.CaisFile
{
    public class BusinessTrailer
    {
        public int TotalRecords { get; set; }

        //-----------------------------------------------------------------------------------
        public string Serialize()
        {
            var ret = new StringBuilder();
            ret.Append(Utils.GetPaddingString("99999999999999999999", 20, false));
            ret.Append(Utils.GetPaddingString(TotalRecords, 8, false));
            ret.Append(Utils.GetPaddingString(" ", 1336, false));

            return ret.ToString();
        }

        //-----------------------------------------------------------------------------------
        public void Deserialize(string data)
        {
            if (data.Length != 1364) throw new Exception("Invalid string length, must be 1364 characters");
            TotalRecords = Utils.ToInt32(data, 20, 8);

        }
    }
}
