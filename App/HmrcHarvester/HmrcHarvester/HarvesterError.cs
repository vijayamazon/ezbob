using System.Net;

namespace Ezbob.HmrcHarvester {
	#region struct HarvesterError

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

	#endregion struct HarvesterError
} // namespace Ezbob.HmrcHarvester
