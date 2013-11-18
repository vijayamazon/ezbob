using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ExperianLib.Ebusiness
{
	using System.Collections.Generic;

	public abstract class BusinessReturnData
    {
        public bool IsError
        {
            get { return !string.IsNullOrEmpty(Error); }
        }
        public string Error { get; set; }
		public DateTime? LastCheckDate { get; protected set; }
		public bool IsDataExpired { get; set; }
        public string OutputXml { get; private set; }

		public decimal BureauScore { get; set; }
		public SortedSet<string> Owners { get; protected set; }

		protected BusinessReturnData()
        {
			Owners = new SortedSet<string>();
        }

		protected BusinessReturnData(Exception ex)
        {
			Owners = new SortedSet<string>();
            Error = ex.Message;
        }

        //-----------------------------------------------------------------------------------
		protected BusinessReturnData(string outputXml, DateTime lastCheckDate)
		{
			LastCheckDate = lastCheckDate;
            OutputXml = outputXml;
			Owners = new SortedSet<string>();
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
