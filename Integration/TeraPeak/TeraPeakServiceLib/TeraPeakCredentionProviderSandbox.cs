using EzBob.TeraPeakServiceLib.Requests.ResearchResult;

namespace EzBob.TeraPeakServiceLib
{
	public class TeraPeakCredentionProviderSandbox : ITeraPeakCredentionProvider
	{
		public TeraPeakRequesterCredentials RequesterCredentials
		{
			get
			{
				return new TeraPeakRequesterCredentials
				       	{
				       		Token = "bc00857c449858c17e83f6c1cf7659",
				       		UserToken = "369d4fa5b86b4a32ca86374b44e564",
				       		DeveloperName = "user_testebay_testebay"
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