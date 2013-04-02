using EzBob.PayPalServiceLib;
using Scorto.Configuration;

namespace EzBob.Configuration
{
	public class TeraPeakConfigRoot : ConfigurationRoot
	{
		public TeraPeakCredentionProviderEnvConfig TeraPeakCredentionProvider
		{
			get { return GetConfiguration<TeraPeakCredentionProviderEnvConfig>( "TeraPeakConfig" ); }
		}
	}
}