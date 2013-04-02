using System;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayAmazonInventory
	{
		public MP_EbayAmazonInventory()
		{
			InventoryItems = new HashedSet<MP_EbayAmazonInventoryItem>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual bool? AmazonUseAFN { get; set; }
		public virtual ISet<MP_EbayAmazonInventoryItem> InventoryItems { get; set; }
		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }

	}
}
