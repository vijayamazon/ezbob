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
    
    public partial class QuickOffer
    {
        public QuickOffer()
        {
            this.CashRequests = new HashSet<CashRequest>();
            this.Customers = new HashSet<Customer>();
        }
    
        public int QuickOfferID { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime CreationDate { get; set; }
        public System.DateTime ExpirationDate { get; set; }
        public int Aml { get; set; }
        public int BusinessScore { get; set; }
        public System.DateTime IncorporationDate { get; set; }
        public decimal TangibleEquity { get; set; }
        public decimal TotalCurrentAssets { get; set; }
        public byte[] TimerstampCounter { get; set; }
        public Nullable<int> ImmediateTerm { get; set; }
        public Nullable<decimal> ImmediateInterestRate { get; set; }
        public Nullable<decimal> ImmediateSetupFee { get; set; }
        public Nullable<decimal> PotentialAmount { get; set; }
        public Nullable<int> PotentialTerm { get; set; }
        public Nullable<decimal> PotentialInterestRate { get; set; }
        public Nullable<decimal> PotentialSetupFee { get; set; }
    
        public virtual ICollection<CashRequest> CashRequests { get; set; }
        public virtual ICollection<Customer> Customers { get; set; }
    }
}
