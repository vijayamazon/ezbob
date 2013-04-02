using EzBob.TeraPeakServiceLib.Requests.ResearchResult;

namespace EzBob.TeraPeakServiceLib
{
	public interface ITeraPeakCredentionProvider: ITeraPeakConnectionProviderOld
	{
		string ApiKey { get; }
		bool IsNewVersionOfCredentials { get; }
	}

	public interface ITeraPeakConnectionProviderOld
	{
		TeraPeakRequesterCredentials RequesterCredentials { get; }
	}
}