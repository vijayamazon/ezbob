using System;
using EzBob.CommonLib;

namespace EKM
{
    public class EkmServiceInfo : IMarketplaceServiceInfo
    {
        public string DisplayName
        {
            get { return "EKM"; }
        }

        public Guid InternalId
        {
            get { return new Guid("{57ABA690-EDBA-4D95-89CF-13A34B40E2F1}"); }
        }

        public string Description
        {
            get { return "ekmPowershop"; }
        }

	    public bool IsPaymentAccount
	    {
			get { return false; }
	    }

    }
}