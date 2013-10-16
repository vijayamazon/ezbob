using System.Collections.Generic;
using System.Xml.Linq;

namespace ExperianLib.Ebusiness
{
	public class TargetResults
	{
		public List<CompanyInfo> Targets { get; private set; }
		public string OutStr { get; private set; }

		public enum LegalStatus
		{
			DontCare,
			Limited,
			NonLimited
		} // enum LegalStatus

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
				} // for each business
			}
			catch
			{
			} // try
		} // constructor
	} // class TargetResults
} // namespace
