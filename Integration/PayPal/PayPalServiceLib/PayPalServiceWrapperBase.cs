using EzBob.PayPalDbLib.Models;
using EzBob.PayPalServiceLib.Common;
using PayPal.Platform.SDK;
using StructureMap;
using com.paypal.sdk.core;

namespace EzBob.PayPalServiceLib
{
	public abstract class PayPalServiceWrapperBase
	{
		private readonly BaseAPIProfile _Profile;
		protected ServiceUrlsInfo ConnectionInfo { get; private set; }

		protected PayPalServiceWrapperBase(IPayPalConfig config, PayPalServiceType serviceType)
		{
			var factory = ObjectFactory.GetInstance<IServiceEndPointFactory>();
			ConnectionInfo = factory.Create( serviceType, config.ServiceType );
			_Profile = ProfileProvider.CreateProfile( config );
		}

		protected BaseAPIProfile GetProfile( PayPalRermissionsGranted securityData = null, string scriptName = null )
		{
			if ( securityData != null )
			{
				string accessToken = securityData.AccessToken;
				string tokenSecret = securityData.TokenSecret;

				string scriptURI = string.Format( @"{0}{1}{2}", ConnectionInfo.ServiceEndPoint, Endpoint, scriptName );
				var headers = OauthSignature.getAuthHeader( _Profile.APIUsername, _Profile.APIPassword, accessToken,
				                                            tokenSecret, OauthSignature.HTTPMethod.POST, scriptURI, null );

				var timestamp = headers["TimeStamp"] as string;
				var signature = headers["Signature"] as string;

				_Profile.Oauth_Signature = signature;
				_Profile.Oauth_Timestamp = timestamp;
				_Profile.Oauth_Token = accessToken;
			}

			return _Profile;
		}

		protected abstract string Endpoint { get; }
	}
}