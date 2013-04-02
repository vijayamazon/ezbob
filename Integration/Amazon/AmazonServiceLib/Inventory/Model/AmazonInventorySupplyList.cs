using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FBAInventoryServiceMWS.Service.Model;

namespace EzBob.AmazonServiceLib.Inventory.Model
{
	public class AmazonInventorySupplyList : IEnumerable<AmazonInventorySupply>
	{
		private readonly ConcurrentBag<AmazonInventorySupply> _List;


		public AmazonInventorySupplyList()
		{
			_List = new ConcurrentBag<AmazonInventorySupply>();
		}


		public AmazonInventorySupplyList(IEnumerable<AmazonInventorySupply> list)
		{
			_List = new ConcurrentBag<AmazonInventorySupply>(list);
		}


		public void Add( InventorySupply data )
		{
			_List.Add( data );
		}

		public IEnumerator<AmazonInventorySupply> GetEnumerator()
		{
			return _List.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
