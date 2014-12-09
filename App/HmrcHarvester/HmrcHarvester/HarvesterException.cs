using System;

namespace Ezbob.HmrcHarvester {

	/// <summary>
	/// Exception thrown from the Harvester.
	/// </summary>
	class HarvesterException : Integration.ChannelGrabberAPI.ApiException {

		/// <summary>
		/// Creates an exception object with given message.
		/// </summary>
		/// <param name="sMsg">Error message.</param>
		public HarvesterException(string sMsg) : base(sMsg) {} // constructor

		/// <summary>
		/// Creates an exception object with given message and inner exception.
		/// </summary>
		/// <param name="sMsg">Error message.</param>
		/// <param name="oInner">Inner exception object.</param>
		public HarvesterException(string sMsg, Exception oInner) : base(sMsg, oInner) {} // constructor

	} // class HarvesterException

	/// <summary>
	/// Exception thrown from the Harvester that is passed to customer as is.
	/// </summary>
	class ClientHarvesterException : HarvesterException {

		/// <summary>
		/// Creates an exception object with given message.
		/// </summary>
		/// <param name="sMsg">Error message.</param>
		public ClientHarvesterException(string sMsg) : base(sMsg) {} // constructor

		/// <summary>
		/// Creates an exception object with given message and inner exception.
		/// </summary>
		/// <param name="sMsg">Error message.</param>
		/// <param name="oInner">Inner exception object.</param>
		public ClientHarvesterException(string sMsg, Exception oInner) : base(sMsg, oInner) {} // constructor

	} // class ClientHarvesterException

} // namespace Ezbob.HmrcHarvester
