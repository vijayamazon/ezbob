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
    
    public partial class EzServiceCronjobLog
    {
        public EzServiceCronjobLog()
        {
            this.EzServiceCronjobLogArguments = new HashSet<EzServiceCronjobLogArgument>();
        }
    
        public long EntryID { get; set; }
        public long JobID { get; set; }
        public int ActionNameID { get; set; }
        public System.DateTime EntryTime { get; set; }
        public int ActionStatusID { get; set; }
        public byte[] TimestampCounter { get; set; }
    
        public virtual EzServiceActionName EzServiceActionName { get; set; }
        public virtual EzServiceActionStatu EzServiceActionStatu { get; set; }
        public virtual ICollection<EzServiceCronjobLogArgument> EzServiceCronjobLogArguments { get; set; }
        public virtual EzServiceCrontab EzServiceCrontab { get; set; }
    }
}
