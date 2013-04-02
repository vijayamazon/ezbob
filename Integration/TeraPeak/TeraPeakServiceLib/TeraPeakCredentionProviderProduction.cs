using EzBob.TeraPeakServiceLib.Requests.ResearchResult;

namespace EzBob.TeraPeakServiceLib
{
	public class TeraPeakCredentionProviderProduction : ITeraPeakCredentionProvider
	{
		public TeraPeakRequesterCredentials RequesterCredentials
		{
			get
			{
				return new TeraPeakRequesterCredentials
				       	{
				       		Token = "bf69db0c2ad55c207ec9c01f793cf0",
				       		UserToken = "5d4f3089986df6cced1e367dda87ee",
				       		DeveloperName = "alex_syrotyuk_alex_syrotyuk"
				       	};
			}
		}

		public string ApiKey
		{
			get { throw new System.NotImplementedException(); }
		}

		public bool IsNewVersionOfCredentials
		{
			get { return false; }
		}
	}
}