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
    
    public partial class MP_RtiTaxMonthRecords
    {
        public MP_RtiTaxMonthRecords()
        {
            this.MP_RtiTaxMonthEntries = new HashSet<MP_RtiTaxMonthEntries>();
        }
    
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CustomerMarketPlaceUpdatingHistoryRecordId { get; set; }
        public int SourceID { get; set; }
    
        public virtual MP_CustomerMarketPlace MP_CustomerMarketPlace { get; set; }
        public virtual ICollection<MP_RtiTaxMonthEntries> MP_RtiTaxMonthEntries { get; set; }
        public virtual VatReturnRecordSource VatReturnRecordSource { get; set; }
    }
}
