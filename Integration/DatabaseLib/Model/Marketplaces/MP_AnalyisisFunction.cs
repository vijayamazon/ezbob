using System;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database {
    
    public class MP_AnalyisisFunction 
	{
        public MP_AnalyisisFunction() 
		{
			AnalyisisFunctionValues = new HashedSet<MP_AnalyisisFunctionValue>();
        }
        public virtual int Id { get; set; }
        public virtual MP_MarketplaceType Marketplace { get; set; }
        public virtual MP_ValueType ValueType { get; set; }
        public virtual ISet<MP_AnalyisisFunctionValue> AnalyisisFunctionValues { get; set; }
        public virtual string Name { get; set; }
        public virtual Guid InternalId { get; set; }
        public virtual string Description { get; set; }
    }
}
