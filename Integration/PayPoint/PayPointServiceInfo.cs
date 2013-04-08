﻿using System;
using EzBob.CommonLib;

namespace PayPoint
{
    public class PayPointServiceInfo : IMarketplaceServiceInfo
    {
        public string DisplayName
        {
            get { return "PayPoint"; }
        }

        public Guid InternalId
        {
            get { return new Guid("{57ABA690-EDBA-4D95-89CF-13A34B40E2F6}"); }
        }

        public string Description
        {
            get { return "PayPoint description"; }
        }

    }
}