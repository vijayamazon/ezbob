using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ExperianLib.Ebusiness
{
    public abstract class BusinessReturnData
    {
        public bool IsError
        {
            get { return !string.IsNullOrEmpty(Error); }
        }
        public string Error { get; set; }
        public string OutputXml { get; private set; }

        public BusinessReturnData()
        {
        }

        public BusinessReturnData(Exception ex)
        {
            Error = ex.Message;
        }

        //-----------------------------------------------------------------------------------
        public BusinessReturnData(string outputXml)
        {
            OutputXml = outputXml;

            try
            {
                var root = XDocument.Load(new StringReader(outputXml)).Root;
                var errors = root.XPathSelectElements("./REQUEST/ERR1/MESSAGE");
                foreach (var el in errors)
                {
                    Error += el.Value + Environment.NewLine;
                }
                Parse(root);
            }
            catch
            {
                Error = "Invalid xml returned from e-series: " + outputXml;
            }
        }

        protected abstract void Parse(XElement root);

    }
}
