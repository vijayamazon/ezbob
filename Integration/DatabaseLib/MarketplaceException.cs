using System;
using EZBob.DatabaseLib.DatabaseWrapper;

namespace EZBob.DatabaseLib
{
	public class MarketplaceException : Exception
	{
		public IDatabaseCustomerMarketPlace DatabaseCustomerMarketPlace { get; private set; }

		public MarketplaceException(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, Exception ex)
			:this(string.Empty, ex)
		{
			DatabaseCustomerMarketPlace = databaseCustomerMarketPlace;
		}

		protected MarketplaceException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public override string Message
		{
			get
			{
				return string.Format("{0}: {1}", DatabaseCustomerMarketPlace.Marketplace.DisplayName, InnerException.Message);
			}
		}

		public int MarketplaceId
		{
			get { return DatabaseCustomerMarketPlace.Id; }
		}

		public string MessageWithCallStack
		{
			get { return string.Format("{0}\n{1}", Message, InnerException.StackTrace); }
		}
	}
}