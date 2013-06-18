﻿using System;

namespace Integration.ChannelGrabberAPI {
	#region class ApiException

	public class ApiException : Exception {
		#region public

		#region constructor

		public ApiException(string sMsg) : base(sMsg) {} // constructor

		#endregion constructor

		#endregion public
	} // class ApiException

	#endregion class ApiException

	#region class ConnectionFailException

	public class ConnectionFailException : ApiException {
		#region public

		#region constructor

		public ConnectionFailException(string sMsg) : base(sMsg) {} // constructor

		#endregion constructor

		#endregion public
	} // class ConnectionFailException

	#endregion class ConnectionFailException
} // namespace Integration.ChannelGrabberAPI
