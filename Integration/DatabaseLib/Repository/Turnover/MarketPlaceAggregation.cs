using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZBob.DatabaseLib.Repository.Turnover {
	using EZBob.DatabaseLib.Model.Database;

	public class MarketPlaceAggregation {

		public virtual System.DateTime TheMonth { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual MP_CustomerMarketplaceUpdatingHistory CustomerMarketPlaceUpdatingHistory { get; set; }
	}
}
