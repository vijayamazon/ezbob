using System;
using System.Security.Policy;
using EzBob.eBayServiceLib.Common;

namespace EzBob.eBayServiceLib.TradingServiceCore
{
	public interface IServiceSignInUrlFactory
	{
		Url Create( ServiceEndPointType endPointType );
	}

	public class ServiceSignInUrlFactory : IServiceSignInUrlFactory
	{
		public Url Create( ServiceEndPointType endPointType )
		{
			switch ( endPointType )
			{
				case ServiceEndPointType.Sandbox:
					return new Url( "https://signin.sandbox.ebay.com/ws/eBayISAPI.dll?SignIn" );

				case ServiceEndPointType.Production:
					return new Url( "https://signin.ebay.com/ws/eBayISAPI.dll?SignIn" );

				default:
					throw new NotImplementedException();
			}
		}
	}

}