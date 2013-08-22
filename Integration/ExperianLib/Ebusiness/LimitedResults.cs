using System;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ExperianLib.Ebusiness
{
    public class LimitedResults:BusinessReturnData
    {
        public decimal BureauScore { get; set; }
        public decimal ExistingBusinessLoans { get; set; }

        public LimitedResults(string inputXml) : base(inputXml)
        {
            
        }

        public LimitedResults(Exception exception) : base(exception)
        {
        }

        protected override void Parse(XElement root)
        {
            var node = root.XPathSelectElement("./REQUEST/DL76/RISKSCORE");
            if (node == null)
            {
                Error += "There is no RISKSCORE in the experian response! ";
            }
            else BureauScore = Convert.ToDecimal(node.Value);

            node = root.XPathSelectElement("./REQUEST/DL95/NUMACTIVEACCS");
            if (node != null)
            {
                ExistingBusinessLoans = Convert.ToDecimal(node.Value);
            }
        }
    }
}
