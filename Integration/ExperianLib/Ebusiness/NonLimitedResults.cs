using System;
using System.Xml.Linq;
using System.Xml.XPath;
using ExperianLib.CaisFile;

namespace ExperianLib.Ebusiness
{
    public class NonLimitedResults:BusinessReturnData
    {
        public decimal BureauScore { get; set; }
        public bool CompanyNotFoundOnBureau { get; set; }



        public NonLimitedResults(string inputXml) : base(inputXml)
        {
            CompanyNotFoundOnBureau = IsError;
        }

        public NonLimitedResults(Exception ex) : base(ex)
        {
        }

        protected override void Parse(XElement root)
        {
            var creditrisk = root.XPathSelectElement("./REQUEST/DN40/RISKSCORE");
            if (creditrisk == null)
            {
                Error += "Can`t read RISKSCORE section from response!";
                return;
            }
            BureauScore = Convert.ToDecimal(creditrisk.Value);
        }
    }
}
