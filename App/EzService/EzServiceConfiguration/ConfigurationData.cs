namespace EzServiceConfiguration {
	public abstract class ConfigurationData : AConfigurationData {

		public virtual int InstanceID { get; protected set; }
		public virtual int SleepTimeout { get; protected set; }
		public virtual int AdminPort { get; protected set; }
		public virtual int ClientPort { get; protected set; }
		public virtual int ClientTimeoutSeconds { get; protected set; }

		public virtual string InstanceName { get; protected set; }

		public virtual string HostName {
			get { return m_sHostName; }
			protected set { m_sHostName = (value ?? string.Empty).Trim(); }
		} // HostName

		private string m_sHostName;

		public virtual string ClientEndpointAddress { get {
				return "http://" + HostName + ":" + ClientPort;
		} } // ClientEndpointAddress

		public virtual string AdminEndpointAddress { get {
			return "net.tcp://" + HostName + ":" + AdminPort;
		} } // AdminEndpointAddress

		protected ConfigurationData(string sInstanceName) {
			RequestedInstanceName = sInstanceName;
		} // constructor

		protected virtual string RequestedInstanceName { get; private set; }

		protected override string InvalidExceptionMessage {
			get {
				return string.Format(
					"Invalid service configuration for service instance {0} has been loaded from DB.",
					RequestedInstanceName
				);
			} // get
		} // InvalidExceptionMessage

		protected override void Adjust() {
			if (!string.IsNullOrWhiteSpace(InstanceName))
				InstanceName = RequestedInstanceName;
		} // Adjust

		protected override bool IsValid() {
			return
				!string.IsNullOrEmpty(HostName) &&
				IsPortValid(AdminPort) &&
				IsPortValid(ClientPort) &&
				(SleepTimeout > 100);
		} // IsValid

		protected virtual bool IsPortValid(int nPort) {
			return (1024 <= nPort) && (nPort <= 65535);
		} // IsPortValid

	} // class ConfigurationData
} // namespace EzServiceConfiguration
