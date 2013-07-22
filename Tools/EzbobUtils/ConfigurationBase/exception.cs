using System;

namespace ConfigurationBase {
	#region class ConfigurationBaseException

	class ConfigurationBaseException : Exception {
		#region public

		#region constructor

		public ConfigurationBaseException(string sMsg) : base(sMsg) {
		} // constructor

		public ConfigurationBaseException(string sMsg, Exception oInnerException) : base(sMsg, oInnerException) {
		} // constructor

		#endregion constructor

		#endregion public
	} // class ConfigurationBaseException

	#endregion class ConfigurationBaseException

	#region class ParseConfigurationBaseException

	class ParseConfigurationBaseException : Exception {
		#region public

		#region constructor

		public ParseConfigurationBaseException(string msg, string key, string value) : base(
			string.Format("Error parsing configuration '{0}' with value '{1}': {2}", key, value, msg)
		) {
		} // constructor

		#endregion constructor

		#endregion public
	} // class ParseConfigurationBaseException

	#endregion class ParseConfigurationBaseException
} // namespace ConfigurationBase
