namespace EZBob.DatabaseLib.Model.Marketplaces.Amazon
{
	using System;
	using Iesi.Collections.Generic;
	using Database;

	public class MP_AmazonOrder
	{
		public MP_AmazonOrder()
		{
			OrderItems = new HashedSet<MP_AmazonOrderItem>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual ISet<MP_AmazonOrderItem> OrderItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}
}