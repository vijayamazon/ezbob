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
    
    public partial class MP_TeraPeakCategoryStatistics
    {
        public int Id { get; set; }
        public int Listings { get; set; }
        public int Successful { get; set; }
        public int ItemsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal SuccessRate { get; set; }
        public int OrderItemId { get; set; }
        public int CategoryId { get; set; }
    
        public virtual MP_TeraPeakCategory MP_TeraPeakCategory { get; set; }
    }
}
