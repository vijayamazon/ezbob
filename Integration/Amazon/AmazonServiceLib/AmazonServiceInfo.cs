using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EzBob.CommonLib;

namespace EzBob.AmazonServiceLib
{
	public class AmazonServiceInfo : IMarketplaceServiceInfo
	{
		public string DisplayName
		{
			get { return "Amazon"; }
		}

		public Guid InternalId
		{
			get { return new Guid( "{A4920125-411F-4BB9-A52D-27E8A00D0A3B}" ); }
		}

		public string Description
		{
			get { return "amazon"; }
		}

		public bool IsPaymentAccount
		{
			get { return false; }
		}

	}
}
