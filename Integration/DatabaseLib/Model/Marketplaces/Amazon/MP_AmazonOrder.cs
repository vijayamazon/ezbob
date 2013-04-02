using System;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_AmazonOrder
	{
		public MP_AmazonOrder()
		{
			OrderItems = new HashedSet<MP_AmazonOrderItem>();
			OrderItems2 = new HashedSet<MP_AmazonOrderItem2>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual ISet<MP_AmazonOrderItem> OrderItems { get; set; }
		public virtual ISet<MP_AmazonOrderItem2> OrderItems2 { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}
}