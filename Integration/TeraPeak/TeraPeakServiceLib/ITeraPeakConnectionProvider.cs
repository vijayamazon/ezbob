using System.Security.Policy;

namespace EzBob.TeraPeakServiceLib
{
	public interface ITeraPeakConnectionProvider
	{
		string Url { get; }
	}
}