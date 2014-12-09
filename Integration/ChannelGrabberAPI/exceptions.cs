using System;

namespace Integration.ChannelGrabberAPI {

	public class ApiException : Exception {

		public ApiException(string sMsg) : base(sMsg, null) {} // constructor

		public ApiException(string sMsg, Exception e) : base(sMsg, e) {} // constructor

	} // class ApiException

	public class ConnectionFailException : ApiException {

		public ConnectionFailException(string sMsg) : base(sMsg, null) {} // constructor

		public ConnectionFailException(string sMsg, Exception e) : base(sMsg, e) {} // constructor

	} // class ConnectionFailException

} // namespace Integration.ChannelGrabberAPI
