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
    
    public partial class MP_ServiceLog
    {
        public MP_ServiceLog()
        {
            this.ExperianConsumerDatas = new HashSet<ExperianConsumerData>();
            this.ExperianDefaultAccounts = new HashSet<ExperianDefaultAccount>();
            this.ExperianLtds = new HashSet<ExperianLtd>();
            this.MP_ExperianHistory = new HashSet<MP_ExperianHistory>();
        }
    
        public long Id { get; set; }
        public string ServiceType { get; set; }
        public Nullable<System.DateTime> InsertDate { get; set; }
        public string RequestData { get; set; }
        public string ResponseData { get; set; }
        public Nullable<long> CustomerId { get; set; }
        public Nullable<int> DirectorId { get; set; }
        public string CompanyRefNum { get; set; }
        public Nullable<int> CompanyID { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string Postcode { get; set; }
    
        public virtual ICollection<ExperianConsumerData> ExperianConsumerDatas { get; set; }
        public virtual ICollection<ExperianDefaultAccount> ExperianDefaultAccounts { get; set; }
        public virtual ICollection<ExperianLtd> ExperianLtds { get; set; }
        public virtual ICollection<MP_ExperianHistory> MP_ExperianHistory { get; set; }
    }
}
