using System;

namespace Ezbob.HmrcHarvester {
	#region class HarvesterException

	/// <summary>
	/// Exception thrown from the Harvester.
	/// </summary>
	class HarvesterException : Integration.ChannelGrabberAPI.ApiException {
		#region public

		#region constructor

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

		#endregion constructor

		#endregion public
	} // class HarvesterException

	#endregion class HarvesterException
} // namespace Ezbob.HmrcHarvester
