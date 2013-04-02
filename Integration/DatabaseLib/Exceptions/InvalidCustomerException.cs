using System;

namespace EZBob.DatabaseLib.Exceptions
{
	public class InvalidCustomerException : Exception
	{
		public int? CustomerId { get; private set; }

		public InvalidCustomerException(int customerId)
			: this( string.Format( "Customer id. #{0} was not found", customerId ) )
		{
			CustomerId = customerId;
		}

		public InvalidCustomerException(string message): 
			base(message, null)
	    {
	        
	    }		
	}
}