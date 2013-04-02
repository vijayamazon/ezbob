using EzBob.TeraPeakServiceLib;
using EzBob.TeraPeakServiceLib.Requests.ResearchResult;
using Scorto.Configuration;

namespace EzBob.PayPalServiceLib
{
	public class TeraPeakCredentionProviderEnvConfig : ConfigurationRoot, ITeraPeakConnectionProvider, ITeraPeakCredentionProvider
	{
		public TeraPeakRequesterCredentials RequesterCredentials
		{
			get
			{
				return IsNewVersionOfCredentials? null : new TeraPeakRequesterCredentials
															{
																Token = Token,
																UserToken = UserToken,
																DeveloperName = DeveloperName
															};
			}
		}

		private string DeveloperName
		{
			get { return GetValue<string>( "DeveloperName" ); }
		}

		private string UserToken
		{
			get { return GetValue<string>( "UserToken" ); }
		}

		private string Token
		{
			get { return GetValue<string>( "Token" ); }

		}
		public string Url
		{
			get { return GetValue<string>( "Url" ); }
		}

		public string ApiKey
		{
			get { return GetValueWithDefault<string>( "ApiKey", "" ); }
		}

		public bool IsNewVersionOfCredentials
		{
			get { return !string.IsNullOrEmpty( ApiKey ); }
		}
	}
}