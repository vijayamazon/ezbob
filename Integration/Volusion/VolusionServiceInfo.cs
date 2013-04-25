using System;
using EzBob.CommonLib;

namespace Integration.Volusion {
    public class VolusionServiceInfo : IMarketplaceServiceInfo {
        public string DisplayName { get { return "Volusion"; } }
        public string Description { get { return "Volusion"; } }
        public Guid   InternalId  { get { return new Guid("{afca0e18-05e3-400f-8af4-b1bcae09375c}"); } }
    } // class VolusionServiceInfo
} // namespace