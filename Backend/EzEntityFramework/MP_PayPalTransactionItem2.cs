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
    
    public partial class MP_PayPalTransactionItem2
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CurrencyId { get; set; }
        public Nullable<double> FeeAmount { get; set; }
        public Nullable<double> GrossAmount { get; set; }
        public Nullable<double> NetAmount { get; set; }
        public string TimeZone { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string PayPalTransactionId { get; set; }
    
        public virtual MP_Currency MP_Currency { get; set; }
        public virtual MP_PayPalTransaction MP_PayPalTransaction { get; set; }
    }
}
