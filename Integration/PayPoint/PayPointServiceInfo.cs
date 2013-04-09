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
            get { return new Guid("{FC8F2710-AEDA-481D-86FF-539DD1FB76E0}"); }
        }

        public string Description
        {
            get { return "PayPoint description"; }
        }

    }
}