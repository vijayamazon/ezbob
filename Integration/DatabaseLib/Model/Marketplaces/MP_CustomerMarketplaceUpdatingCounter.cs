using System;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_CustomerMarketplaceUpdatingCounter
	{
		public virtual long Id { get; set; }
		public virtual MP_CustomerMarketplaceUpdatingActionLog Action { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual string Method { get; set; }
		public virtual string Details { get; set; }
	}
}