using System;

namespace Integration.ChannelGrabberConfig {
	#region class InvalidCredentialsException

	public class InvalidCredentialsException : Exception {
		#region public

		#region constructor

		public InvalidCredentialsException(string sMsg) : base(sMsg) {} // constructor

		#endregion constructor

		#endregion public
	} // class InvalidCredentialsException

	#endregion class InvalidCredentialsException
} // namespace Integration.ChannelGrabberConfig
