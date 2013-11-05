namespace FreeAgent
{
	using System;
	using EzBob.CommonLib;

	public class FreeAgentServiceInfo : IMarketplaceServiceInfo
    {
        public string DisplayName
        {
			get { return "FreeAgent"; }
        }

        public Guid InternalId
        {
            get { return new Guid("{737691E8-5C77-48EF-B01B-7348E24094B6}"); }
        }

        public string Description
        {
			get { return "FreeAgent"; }
        }

		public bool IsPaymentAccount
		{
			get { return true; }
		}
    }
}