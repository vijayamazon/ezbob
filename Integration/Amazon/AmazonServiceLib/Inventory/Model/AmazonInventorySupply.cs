using FBAInventoryServiceMWS.Service.Model;

namespace EzBob.AmazonServiceLib.Inventory.Model
{
	public class AmazonInventorySupply
	{
		public static implicit operator AmazonInventorySupply( InventorySupply data )
		{
			var rez = new AmazonInventorySupply();

			if ( data.IsSetASIN() )
			{
				rez.ASIN = data.ASIN;
			}

			if ( data.IsSetCondition() )
			{
				rez.Condition = data.Condition;
			}

			if ( data.IsSetEarliestAvailability() )
			{
				rez.EarliestAvailability = data.EarliestAvailability;
			}

			if ( data.IsSetFNSKU() )
			{
				rez.FNSKU = data.FNSKU;
			}

			if ( data.IsSetInStockSupplyQuantity() )
			{
				rez.InStockSupplyQuantity = (int)data.InStockSupplyQuantity;
			}

			if ( data.IsSetSellerSKU() )
			{
				rez.SellerSKU = data.SellerSKU;
			}

			if ( data.IsSetSupplyDetail() )
			{
				rez.SupplyDetail = data.SupplyDetail;
			}

			if ( data.IsSetTotalSupplyQuantity() )
			{
				rez.TotalSupplyQuantity = (int)data.TotalSupplyQuantity;
			}

			return rez;
		}
		
		public string ASIN { get; set; }

		public string Condition { get; set; }

		public AmazonTimePoint EarliestAvailability { get; set; }

		public string FNSKU { get; set; }

		public int InStockSupplyQuantity { get; set; }

		public string SellerSKU { get; set; }

		public AmazonInventorySupplyDetailList SupplyDetail { get; set; }

		public int TotalSupplyQuantity { get; set; }

	}
}