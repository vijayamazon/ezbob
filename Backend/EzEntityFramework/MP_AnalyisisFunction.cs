//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EzEntityFramework
{
    using System;
    using System.Collections.Generic;
    
    public partial class MP_AnalyisisFunction
    {
        public MP_AnalyisisFunction()
        {
            this.MP_AnalyisisFunctionValues = new HashSet<MP_AnalyisisFunctionValues>();
        }
    
        public int Id { get; set; }
        public int MarketPlaceId { get; set; }
        public int ValueTypeId { get; set; }
        public string Name { get; set; }
        public System.Guid InternalId { get; set; }
        public string Description { get; set; }
    
        public virtual MP_MarketplaceType MP_MarketplaceType { get; set; }
        public virtual MP_ValueType MP_ValueType { get; set; }
        public virtual ICollection<MP_AnalyisisFunctionValues> MP_AnalyisisFunctionValues { get; set; }
    }
}
