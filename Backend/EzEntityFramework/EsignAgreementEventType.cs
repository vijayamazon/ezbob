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
    
    public partial class EsignAgreementEventType
    {
        public EsignAgreementEventType()
        {
            this.EsignatureHistories = new HashSet<EsignatureHistory>();
        }
    
        public int EventTypeID { get; set; }
        public string EventType { get; set; }
        public byte[] TimestampCounter { get; set; }
    
        public virtual ICollection<EsignatureHistory> EsignatureHistories { get; set; }
    }
}
