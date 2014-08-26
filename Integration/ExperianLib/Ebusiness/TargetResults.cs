using System.Collections.Generic;
using System.Xml.Linq;

namespace ExperianLib.Ebusiness
{
	using System.Linq;

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

				if (Targets.Any()) {
					foreach (var t in Targets) {
						t.BusName = string.IsNullOrEmpty(t.BusName)
							? string.Empty
							: System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.BusName.ToLower());
						t.AddrLine1 = string.IsNullOrEmpty(t.AddrLine1)
							? string.Empty
							: System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine1.ToLower());
						t.AddrLine2 = string.IsNullOrEmpty(t.AddrLine2)
							? string.Empty
							: System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine2.ToLower());
						t.AddrLine3 = string.IsNullOrEmpty(t.AddrLine3)
							? string.Empty
							: System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine3.ToLower());
						t.AddrLine4 = string.IsNullOrEmpty(t.AddrLine4)
							? string.Empty
							: System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(t.AddrLine4.ToLower());
					} // for each

					if (Targets.Count > 1)
						Targets.Add(new CompanyInfo { BusName = "Company not found", BusRefNum = "skip" });
				} // if
			}
			catch
			{
			} // try
		} // constructor
	} // class TargetResults
} // namespace
