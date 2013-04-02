using System;
using PayPal.Platform.SDK;

namespace EzBob.PayPalServiceLib
{
	public class PayPalException : Exception
	{
		private readonly FaultDetailFaultMessageError[] _rrorDetails;

		public PayPalException( FaultDetailFaultMessageError[] errorDetails )
		{
			_rrorDetails = errorDetails;
		}

		public FaultDetailFaultMessageError[] ErrorDetails
		{
            get { return _rrorDetails; }
		}
	}
}