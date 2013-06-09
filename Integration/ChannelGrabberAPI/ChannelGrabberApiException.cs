using System;

namespace Integration.ChannelGrabberAPI {
	#region class ChannelGrabberApiException

	public class ChannelGrabberApiException : Exception {
		#region public

		#region constructor

		public ChannelGrabberApiException(string sMsg) : base(sMsg) {} // constructor

		#endregion constructor

		#endregion public
	} // class ChannelGrabberApiException

	#endregion class ChannelGrabberApiException

	#region class ConnectionFailChannelGrabberApiException

	public class ConnectionFailChannelGrabberApiException : ChannelGrabberApiException {
		#region public

		#region constructor

		public ConnectionFailChannelGrabberApiException(string sMsg) : base(sMsg) {} // constructor

		#endregion constructor

		#endregion public
	} // class ConnectionFailChannelGrabberApiException

	#endregion class ConnectionFailChannelGrabberApiException
} // namespace Integration.ChannelGrabberAPI
