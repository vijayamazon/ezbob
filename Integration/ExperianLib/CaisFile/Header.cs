using System;
using System.Globalization;
using System.Text;

namespace ExperianLib.CaisFile
{
    public class Header
    {
        private const string HeaderTitle = "HEADER";
        private const string FileFormat = "CAIS2007";

        public int SourceCodeNumber { get; set; }
        public DateTime DateCreation { get; set; }
        public string CompanyPortfolioName { get; set; }
        public int OverdraftReportingCutOff { get; set; }
        public bool IsCardsBehaviouralSharing { get; set; }

        //-----------------------------------------------------------------------------------
        public string Serialize()
        {
            var ret = new StringBuilder();
            ret.Append(Utils.GetPaddingString(HeaderTitle, 20));
            ret.Append(Utils.GetPaddingString(SourceCodeNumber, 3));
            ret.Append(Utils.GetPaddingString(DateCreation));
            ret.Append(Utils.GetPaddingString(CompanyPortfolioName, 30, true));
            ret.Append(Utils.GetPaddingString(String.Empty, 20));
            ret.Append(Utils.GetPaddingString(FileFormat, 8));
            ret.Append(Utils.GetPaddingString(OverdraftReportingCutOff, 6));
            ret.Append(IsCardsBehaviouralSharing ? "Y" : " ");
            ret.Append(Utils.GetPaddingString(String.Empty, 434));
            return ret.ToString();
        }

        //-----------------------------------------------------------------------------------
        public void Deserialize(string data)
        {
            if (data.Length != 530) throw new Exception("Invalid string length, must be 530 characters");
            if (data.Substring(0, 20).Trim() != HeaderTitle) throw new Exception("Invalid header");
            if (data.Substring(81, 8).Trim() != FileFormat) throw new Exception("Invalid file format");

            SourceCodeNumber = Utils.ToInt32(data, 20, 3);
            DateCreation = Utils.ToDate(data, 23);
            CompanyPortfolioName = Utils.ToString(data, 31, 30);
            OverdraftReportingCutOff = Utils.ToInt32(data, 89, 6);
            IsCardsBehaviouralSharing = Utils.ToString(data, 95, 1) == "Y";
        }
    }
}
