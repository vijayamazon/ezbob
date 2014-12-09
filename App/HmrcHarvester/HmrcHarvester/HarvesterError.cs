using System.Net;

namespace Ezbob.HmrcHarvester {

	/// <summary>
	/// Harvester error.
	/// </summary>
	public struct HarvesterError {
		/// <summary>
		/// Error code.
		/// </summary>
		public HttpStatusCode Code;

		/// <summary>
		/// Error message.
		/// </summary>
		public string Message;
	} // HarvesterError

} // namespace Ezbob.HmrcHarvester
