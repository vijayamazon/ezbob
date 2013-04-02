using System;

namespace EZBob.DatabaseLib.Exceptions
{
	public class InvalidCustomerMarketPlaceException : Exception
	{
		public int CustomerMarketPlaceId { get; private set; }

		public InvalidCustomerMarketPlaceException( int customerMarketPlaceId )			
			:base(string.Format( "Invalid Customer Marketplace: id = {0}", customerMarketPlaceId ), null)
		{
			CustomerMarketPlaceId = customerMarketPlaceId;
		}
	}
}