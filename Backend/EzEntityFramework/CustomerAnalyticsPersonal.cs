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
    
    public partial class CustomerAnalyticsPersonal
    {
        public int CustomerID { get; set; }
        public System.DateTime AnalyticsDate { get; set; }
        public bool IsActive { get; set; }
        public int Score { get; set; }
        public int IndebtednessIndex { get; set; }
        public int NumOfAccounts { get; set; }
        public int NumOfDefaults { get; set; }
        public int NumOfLastDefaults { get; set; }
        public long CustomerAnalyticsPersonalID { get; set; }
        public byte[] TimestampCounter { get; set; }
    
        public virtual Customer Customer { get; set; }
    }
}
