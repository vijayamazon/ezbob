using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EzBob.AmazonServiceLib.MarketWebService.Model
{
	public class AmazonInventoryData : ReceivedDataListBase<InventoryItem>
	{
		public AmazonInventoryData(DateTime submittedDate) : base(submittedDate)
		{
		}

		private AmazonInventoryData(DateTime submittedDate, IEnumerable<InventoryItem> collection) 
			: base(submittedDate, collection)
		{
		}

		public bool? UseAFN { get; set; }

		/*public override ReceivedDataListBase<InventoryItem> Create(DateTime submittedDate, IEnumerable<InventoryItem> collection)
		{
			return new AmazonInventoryData( submittedDate, collection );
		}*/
	}

	public class InventoryItem
	{
		public string SKU { get; set; }
		public double Price { get; set; }
		public int Quantity { get; set; }
		public string ItemID { get; set; }
		public AmountInfo Amount { get; set; }
	}

}
