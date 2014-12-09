using System;

namespace ConfigurationBase {

	class ConfigurationBaseException : Exception {

		public ConfigurationBaseException(string sMsg) : base(sMsg) {
		} // constructor

		public ConfigurationBaseException(string sMsg, Exception oInnerException) : base(sMsg, oInnerException) {
		} // constructor

	} // class ConfigurationBaseException

	class ParseConfigurationBaseException : Exception {

		public ParseConfigurationBaseException(string msg, string key, string value) : base(
			string.Format("Error parsing configuration '{0}' with value '{1}': {2}", key, value, msg)
		) {
		} // constructor

	} // class ParseConfigurationBaseException

} // namespace ConfigurationBase
