using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FBAInventoryServiceMWS.Service.Model;

namespace EzBob.AmazonServiceLib.Inventory.Model
{
	public class AmazonInventorySupplyDetailList : IEnumerable<AmazonInventorySupplyDetail>
	{
		public static implicit operator AmazonInventorySupplyDetailList( InventorySupplyDetailList data )
		{
			var rez = new AmazonInventorySupplyDetailList();

			data.member.AsParallel().ForAll( rez.Add );

			return rez;
		}

		private readonly ConcurrentBag<AmazonInventorySupplyDetail> _List = new ConcurrentBag<AmazonInventorySupplyDetail>();

		private void Add( InventorySupplyDetail data )
		{
			_List.Add( data );
		}

		public IEnumerator<AmazonInventorySupplyDetail> GetEnumerator()
		{
			return _List.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}