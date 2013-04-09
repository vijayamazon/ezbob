using System;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Model.Database;

namespace EZBob.DatabaseLib.Exceptions
{
	public class NoMarketPlaceForCustomerException : Exception
	{
		public NoMarketPlaceForCustomerException(Customer customer, IDatabaseMarketplace marketplace)
			:base(string.Format("Market place \"{0}\" for customer \"{1}\" does not exist", marketplace.DisplayName, customer.Name))
		{
		}
	}
}