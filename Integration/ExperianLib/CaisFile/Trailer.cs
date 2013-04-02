using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ExperianLib.CaisFile
{
    public class Trailer
    {
        private const string Magic = "99999999999999999999";
        public int TotalRecords { get; set; }

        public string Serialize()
        {
            var ret = new StringBuilder();
            ret.Append(Magic);
            ret.Append(Utils.GetPaddingString(TotalRecords, 8));
            ret.Append(Utils.GetPaddingString(String.Empty, 502));
            return ret.ToString();
        }

        //-----------------------------------------------------------------------------------
        public void Deserialize(string data)
        {
            if (data.Length != 530) throw new Exception("Invalid string length, must be 530 characters");
            TotalRecords = Utils.ToInt32(data, 20, 8);
        }
    }
}
