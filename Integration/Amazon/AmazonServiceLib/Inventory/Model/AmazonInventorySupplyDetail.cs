using FBAInventoryServiceMWS.Service.Model;

namespace EzBob.AmazonServiceLib.Inventory.Model
{
	public class AmazonInventorySupplyDetail
	{
		public static implicit operator AmazonInventorySupplyDetail( InventorySupplyDetail data )
		{
			var rez = new AmazonInventorySupplyDetail();

			if ( data.IsSetEarliestAvailableToPick() )
			{
				rez.EarliestAvailableToPick = data.EarliestAvailableToPick;
			}

			if ( data.IsSetLatestAvailableToPick() )
			{
				rez.LatestAvailableToPick = data.LatestAvailableToPick;
			}

			if ( data.IsSetQuantity() )
			{
				rez.Quantity = (int) data.Quantity;
			}

			if ( data.IsSetSupplyType() )
			{
				rez.SupplyType = data.SupplyType;
			}

			return rez;
		}

		public string SupplyType { get; set; }

		public int Quantity { get; set; }

		public AmazonTimePoint LatestAvailableToPick { get; set; }

		public AmazonTimePoint EarliestAvailableToPick { get; set; }
	}
}