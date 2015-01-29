using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZBob.DatabaseLib.Repository.Turnover {
	public class FilteredAggregationResult {

		public virtual DateTime TheMonth { get; set; }
		public virtual decimal Turnover { get; set; }
		public virtual int MpId { get; set; }

	}
}
