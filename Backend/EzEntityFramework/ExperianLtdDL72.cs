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
    
    public partial class ExperianLtdDL72
    {
        public long ExperianLtdDL72ID { get; set; }
        public long ExperianLtdID { get; set; }
        public string ForeignAddressFlag { get; set; }
        public string IsCompany { get; set; }
        public string Number { get; set; }
        public Nullable<int> LengthOfDirectorship { get; set; }
        public Nullable<int> DirectorsAgeYears { get; set; }
        public Nullable<int> NumberOfConvictions { get; set; }
        public string Prefix { get; set; }
        public string FirstName { get; set; }
        public string MidName1 { get; set; }
        public string MidName2 { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Qualifications { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string CompanyNumber { get; set; }
        public string ShareInfo { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
        public string HouseName { get; set; }
        public string HouseNumber { get; set; }
        public string Street { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public byte[] TimestampCounter { get; set; }
    
        public virtual ExperianLtd ExperianLtd { get; set; }
    }
}
