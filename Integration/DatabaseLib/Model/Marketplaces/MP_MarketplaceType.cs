using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database {
    
    public class MP_MarketplaceType 
	{
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual System.Guid InternalId { get; set; }
        public virtual string Description { get; set; }
		public virtual bool Active { get; set; } // alexbo Apr 4 2013
    }
}
