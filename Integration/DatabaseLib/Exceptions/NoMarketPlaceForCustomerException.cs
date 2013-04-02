using System;
using EZBob.DatabaseLib.DatabaseWrapper;

namespace EZBob.DatabaseLib.Exceptions
{
	public class NoMarketPlaceForCustomerException : Exception
	{
		public NoMarketPlaceForCustomerException(IDatabaseCustomer customer, IDatabaseMarketplace marketplace)
			:base(string.Format("Market place \"{0}\" for customer \"{1}\" not exists", marketplace.DisplayName, customer.Name))
		{
		}
	}
}