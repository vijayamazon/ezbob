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
    
    public partial class MP_TeraPeakOrder
    {
        public MP_TeraPeakOrder()
        {
            this.MP_TeraPeakOrderItem = new HashSet<MP_TeraPeakOrderItem>();
        }
    
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> LastOrderItemEndDate { get; set; }
        public Nullable<int> CustomerMarketPlaceUpdatingHistoryRecordId { get; set; }
    
        public virtual MP_CustomerMarketPlace MP_CustomerMarketPlace { get; set; }
        public virtual MP_CustomerMarketPlaceUpdatingHistory MP_CustomerMarketPlaceUpdatingHistory { get; set; }
        public virtual ICollection<MP_TeraPeakOrderItem> MP_TeraPeakOrderItem { get; set; }
    }
}
