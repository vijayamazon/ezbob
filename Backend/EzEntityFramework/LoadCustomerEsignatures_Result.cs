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
    
    public partial class LoadCustomerEsignatures_Result
    {
        public int CustomerID { get; set; }
        public long EsignatureID { get; set; }
        public System.DateTime SendDate { get; set; }
        public int EsignTemplateID { get; set; }
        public string DocumentName { get; set; }
        public int SignatureStatusID { get; set; }
        public string SignatureStatus { get; set; }
        public Nullable<bool> HasDocument { get; set; }
        public long EsignerID { get; set; }
        public Nullable<int> DirectorID { get; set; }
        public Nullable<int> ExperianDirectorID { get; set; }
        public string SignerEmail { get; set; }
        public string SignerFirstName { get; set; }
        public string SignerLastName { get; set; }
        public int SignerStatusID { get; set; }
        public string SignerStatus { get; set; }
        public Nullable<System.DateTime> SignDate { get; set; }
    }
}
