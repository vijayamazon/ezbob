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
    
    public partial class ExperianLtdDL68
    {
        public long ExperianLtdDL68ID { get; set; }
        public long ExperianLtdID { get; set; }
        public string SubsidiaryRegisteredNumber { get; set; }
        public string SubsidiaryStatus { get; set; }
        public string SubsidiaryLegalStatus { get; set; }
        public string SubsidiaryName { get; set; }
        public byte[] TimestampCounter { get; set; }
    
        public virtual ExperianLtd ExperianLtd { get; set; }
    }
}
