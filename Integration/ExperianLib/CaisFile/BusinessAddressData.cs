using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperianLib.CaisFile
{
    public class BusinessAddressData
    {
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string PostCode { get; set; }

        //-----------------------------------------------------------------------------------
        public string Serialize()
        {
            var ret = new StringBuilder();

            ret.Append(Utils.GetPaddingString(Name, 39, true));
            ret.Append(Utils.GetPaddingString(AddressLine1, 32, true));
            ret.Append(Utils.GetPaddingString(AddressLine2, 32, true));
            ret.Append(Utils.GetPaddingString(AddressLine3, 32, true));
            ret.Append(Utils.GetPaddingString(AddressLine4, 32, true));
            ret.Append(Utils.GetPaddingString(PostCode, 8, true));

            return ret.ToString();
        }

        //-----------------------------------------------------------------------------------
        public void Deserialize(string data)
        {
            if (data.Length != 175) throw new Exception("Invalid string length, must be 174 characters");

            Name = Utils.ToString(data, 0, 39);
            AddressLine1 = Utils.ToString(data, 39, 32);
            AddressLine2 = Utils.ToString(data, 71, 32);
            AddressLine3 = Utils.ToString(data, 103, 32);
            AddressLine4 = Utils.ToString(data, 135, 32);
            PostCode = Utils.ToString(data, 167, 8);
        }
    }
}
