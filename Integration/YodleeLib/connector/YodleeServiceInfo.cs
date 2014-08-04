namespace YodleeLib.connector
{
	using System;
	using EzBob.CommonLib;

    public class YodleeServiceInfo : IMarketplaceServiceInfo
    {
        public string DisplayName
        {
            get { return "Yodlee"; }
        }

        public Guid InternalId
        {
            get { return new Guid("{107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF}"); }
        }

        public string Description
        {
            get { return "Bank"; }
        }

		public bool IsPaymentAccount
		{
			get { return true; }
		}

    }
}