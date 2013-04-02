using System;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_CustomerMarketplaceUpdatingActionLog
	{
		public MP_CustomerMarketplaceUpdatingActionLog()
		{
			RequestsCounter = new HashedSet<MP_CustomerMarketplaceUpdatingCounter>();
		}

		public virtual long Id { get; set; }
		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
		public virtual DateTime? UpdatingStart { get; set; }
		public virtual DateTime? UpdatingEnd { get; set; }
		public virtual string Error { get; set; }
		public virtual string ActionName { get; set; }
		public virtual string ControlValueName { get; set; }
		public virtual string ControlValue { get; set; }

		public virtual ISet<MP_CustomerMarketplaceUpdatingCounter> RequestsCounter { get; set; }

		public virtual DatabaseElapsedTimeInfo ElapsedTime { get; set; }
	}
}