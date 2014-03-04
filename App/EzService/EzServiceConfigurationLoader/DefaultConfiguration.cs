namespace EzServiceConfigurationLoader {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DefaultConfiguration : EzServiceConfigurationLoader.Configuration {
		#region public

		#region constructor

		public DefaultConfiguration(string sInstanceName, AConnection oDB, ASafeLog oLog) : base(sInstanceName, oDB, oLog) {
		} // constructor

		#endregion constructor

		#endregion public

		#region protected

		#region property SpName

		protected override string SpName {
			get { return "EzServiceGetDefaultInstance"; } // get
		} // SpName

		#endregion property SpName

		#region property ArgName

		protected override string ArgName {
			get { return "@Argument"; } // get
		} // ArgName

		#endregion property SpName

		#endregion protected
	} // class DefaultConfiguration
} // namespace EzServiceConfigurationLoader
