namespace Sage
{
	using System;
	using EzBob.CommonLib;

	public class SageServiceInfo : IMarketplaceServiceInfo
	{
		public string DisplayName
		{
			get { return "Sage"; }
		}

		public Guid InternalId
		{
			get { return new Guid("{4966BB57-0146-4E3D-AA24-F092D90B7923}"); }
		}

		public string Description
		{
			get { return "SageOne"; }
		}

		public bool IsPaymentAccount
		{
			get { return true; }
		}
	}
}