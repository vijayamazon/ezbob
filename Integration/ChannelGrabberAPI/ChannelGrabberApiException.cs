using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Integration.ChannelGrabberAPI {
	#region class ChannelGrabberApiException

	public class ChannelGrabberApiException : Exception {
		public ChannelGrabberApiException(string sMsg) : base(sMsg) {} // constructor
	} // class ChannelGrabberApiException

	#endregion class ChannelGrabberApiException
} // namespace Integration.ChannelGrabberAPI
