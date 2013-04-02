using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EzBob.CommonLib.ReceivedDataListLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.Inventory
{
	public class DatabaseInventoryList : ReceivedDataListBase<DatabaseInventoryItem>
	{
		public DatabaseInventoryList(DateTime submittedDate) 
			: base(submittedDate)
		{			
		}

		public DatabaseInventoryList(DateTime submittedDate, IEnumerable<DatabaseInventoryItem> collection) 
			: base(submittedDate, collection)
		{			
		}

		public bool? UseAFN { get; set; }
	}

	public class DatabaseInventoryItem
	{
		public int BidCount { get; set; }
		public string Sku { get; set; }
		
		public int Quantity { get; set; }
		public string ItemId { get; set; }		

		public AmountInfo Amount { get; set; }
	}
}
