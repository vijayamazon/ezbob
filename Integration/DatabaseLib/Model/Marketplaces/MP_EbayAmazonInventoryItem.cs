using EZBob.DatabaseLib.Common;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayAmazonInventoryItem
	{
		public MP_EbayAmazonInventoryItem() { }

		public virtual int Id { get; set; }
		public virtual MP_EbayAmazonInventory Inventory { get; set; }
		public virtual int BidCount { get; set; }
		public virtual string Sku { get; set; }
		public virtual int Quantity { get; set; }
		public virtual string ItemId { get; set; }		
		public virtual AmountInfo Amount { get; set; }
	}
}