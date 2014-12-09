namespace EzServiceConfigurationLoader {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DefaultConfiguration : EzServiceConfigurationLoader.Configuration {

		public DefaultConfiguration(string sInstanceName, AConnection oDB, ASafeLog oLog) : base(sInstanceName, oDB, oLog) {
		} // constructor

		protected override string SpName {
			get { return "EzServiceGetDefaultInstance"; } // get
		} // SpName

		protected override string ArgName {
			get { return "@Argument"; } // get
		} // ArgName

	} // class DefaultConfiguration
} // namespace EzServiceConfigurationLoader
