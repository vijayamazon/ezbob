using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ExperianLib.Ebusiness
{
    public class TargetResults
    {
        public List<CompanyInfo> Targets { get; private set; }
        public string OutStr { get; private set; }

        public TargetResults(string targetData)
        {
            OutStr = targetData;
            Targets = new List<CompanyInfo>();
            try
            {
                foreach (var business in XElement.Parse(targetData).Element("REQUEST").Elements("DT11"))
                {
                    var bi = XSerializer.Deserialize<CompanyInfo>(business);
                    Targets.Add(bi);
                }
            }
            catch
            {
            }
        }
    }
}
