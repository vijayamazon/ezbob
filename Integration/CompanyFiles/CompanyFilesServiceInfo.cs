using System;
using EzBob.CommonLib;

namespace CompanyFiles
{
	public class CompanyFilesServiceInfo : IMarketplaceServiceInfo
	{
		public string DisplayName
		{
			get { return "CompanyFiles"; }
		}

		public Guid InternalId
		{
			get { return new Guid("{1C077670-6D6C-4CE9-BEBC-C1F9A9723908}"); }
		}

		public string Description
		{
			get { return "Financial Documents"; }
		}

		public bool IsPaymentAccount
		{
			get { return false; }
		}

	}
}