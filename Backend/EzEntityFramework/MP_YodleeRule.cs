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
    
    public partial class MP_YodleeRule
    {
        public MP_YodleeRule()
        {
            this.MP_YodleeGroupRuleMap = new HashSet<MP_YodleeGroupRuleMap>();
        }
    
        public int Id { get; set; }
        public string RuleDescription { get; set; }
    
        public virtual ICollection<MP_YodleeGroupRuleMap> MP_YodleeGroupRuleMap { get; set; }
    }
}
