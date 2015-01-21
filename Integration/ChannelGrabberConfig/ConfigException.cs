using System;

namespace Integration.ChannelGrabberConfig {

	public class ConfigException : Exception {
		public bool IsWarn { get; set; }
		public ConfigException(string sMsg, bool isWarn = false) : base(sMsg) {
			IsWarn = isWarn;
		} // constructor

	} // class ConfigException

} // namespace Integration.ChannelGrabberConfig
