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
    
    public partial class AmlResult
    {
        public int Id { get; set; }
        public string LookupKey { get; set; }
        public Nullable<int> ServiceLogId { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public string AuthenticationDecision { get; set; }
        public Nullable<int> AuthenticationIndex { get; set; }
        public string AuthIndexText { get; set; }
        public Nullable<decimal> NumPrimDataItems { get; set; }
        public Nullable<decimal> NumPrimDataSources { get; set; }
        public Nullable<decimal> NumSecDataItems { get; set; }
        public string StartDateOldestPrim { get; set; }
        public string StartDateOldestSec { get; set; }
        public string Error { get; set; }
        public Nullable<bool> IsActive { get; set; }
    }
}
