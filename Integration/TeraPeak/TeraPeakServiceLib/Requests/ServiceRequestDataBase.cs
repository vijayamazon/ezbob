using EzBob.TeraPeakServiceLib.Requests.ResearchResult;

namespace EzBob.TeraPeakServiceLib.Requests
{
	public abstract class ServiceRequestDataBase
	{
		protected ServiceRequestDataBase( TeraPeakRequesterCredentials requesterCredentials )
		{
			RequesterCredentials = requesterCredentials;
			Version = 2;
		}

		protected ServiceRequestDataBase()
		{
			Version = 2;
		}

		public int Version { get; set; }

		public TeraPeakRequesterCredentials RequesterCredentials { get; set; }
	}
}
