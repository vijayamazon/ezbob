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
    
    public partial class Security_Session
    {
        public int UserId { get; set; }
        public byte State { get; set; }
        public string SessionId { get; set; }
        public System.DateTime CreationDate { get; set; }
        public System.DateTime LastAccessTime { get; set; }
        public string HostAddress { get; set; }
    
        public virtual Security_User Security_User { get; set; }
    }
}
