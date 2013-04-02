using System;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_TeraPeakOrder
	{
		public MP_TeraPeakOrder()
		{
			OrderItems = new HashedSet<MP_TeraPeakOrderItem>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual DateTime? LastOrderItemEndDate { get; set; }
		public virtual ISet<MP_TeraPeakOrderItem> OrderItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}
}