using System;
using System.Linq;
using EzBob.PayPalServiceLib.com.paypal.service;

namespace EzBob.PayPalServiceLib
{

	public class ServiceResponceException : Exception
	{
		private readonly AbstractResponseType _Response;

		public ServiceResponceException( AbstractResponseType response )
		{
			_Response = response;
		}

		public override string Message
		{
			get
			{
				string rez = string.Empty;
				if ( _Response.Errors.Length == 1 )
				{
					rez = string.Format( "{0} ({1}: {2})", _Response.Errors[0].LongMessage, _Response.Errors[0].ErrorCode, _Response.Errors[0].ShortMessage );
				}
				else
				{
					var errs = _Response.Errors.Select( ( t, i ) => string.Format( "{0}.{1} ({2}: {3})", i + 1, t.LongMessage, t.ErrorCode, t.ShortMessage ) ).ToList();

					rez = string.Join( "\n", errs );
				}

				return string.Format( "Error on Service: {0}\n\t{1}", new PayPalServiceInfo().DisplayName, rez );
			}
		}
	}
}
