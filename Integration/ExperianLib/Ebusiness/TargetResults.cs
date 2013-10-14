using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;

namespace ExperianLib.Ebusiness {
	public class TargetResults {
		public List<CompanyInfo> Targets { get; private set; }
		public string OutStr { get; private set; }

		public enum LegalStatus {
			DontCare,
			Limited,
			NonLimited
		} // enum LegalStatus

		public TargetResults(string targetData, LegalStatus nFilter) {
			OutStr = targetData;

			Targets = new List<CompanyInfo>();

			string sLegalStatus = nFilter.ToString().Substring(0, 1);

			try {
				foreach (var business in XElement.Parse(targetData).Element("REQUEST").Elements("DT11")) {
					var bi = XSerializer.Deserialize<CompanyInfo>(business);

					if ((nFilter == LegalStatus.DontCare) || (sLegalStatus == bi.LegalStatus))
						Targets.Add(bi);
				} // for each business
			}
			catch {
			} // try
		} // constructor
	} // class TargetResults
} // namespace
