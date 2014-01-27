namespace EzServiceConfiguration {
	public abstract class ConfigurationData : AConfigurationData {
		#region public

		#region configuration details

		public virtual int InstanceID { get; protected set; }
		public virtual int SleepTimeout { get; protected set; }
		public virtual int AdminPort { get; protected set; }
		public virtual int ClientPort { get; protected set; }

		public virtual string InstanceName { get; protected set; }

		#region property HostName

		public virtual string HostName {
			get { return m_sHostName; }
			protected set { m_sHostName = (value ?? string.Empty).Trim(); }
		} // HostName

		private string m_sHostName;

		#endregion property HostName

		#endregion configuration details

		#region property ClientEndpointAddress

		public virtual string ClientEndpointAddress { get {
				return "http://" + HostName + ":" + ClientPort;
		} } // ClientEndpointAddress

		#endregion property ClientEndpointAddress

		#region property AdminEndpointAddress

		public virtual string AdminEndpointAddress { get {
			return "net.tcp://" + HostName + ":" + AdminPort;
		} } // AdminEndpointAddress

		#endregion property AdminEndpointAddress

		#endregion public

		#region protected

		#region constructor

		protected ConfigurationData(string sInstanceName) {
			RequestedInstanceName = sInstanceName;
		} // constructor

		#endregion constructor

		protected virtual string RequestedInstanceName { get; private set; }

		#region property InvalidExceptionMessage

		protected override string InvalidExceptionMessage {
			get {
				return string.Format(
					"Invalid service configuration for service instance {0} has been loaded from DB.",
					RequestedInstanceName
				);
			} // get
		} // InvalidExceptionMessage

		#endregion property InvalidExceptionMessage

		#region method Adjust

		protected override void Adjust() {
			if (!string.IsNullOrWhiteSpace(InstanceName))
				InstanceName = RequestedInstanceName;
		} // Adjust

		#endregion method Adjust

		#region method IsValid

		protected override bool IsValid() {
			return
				!string.IsNullOrEmpty(HostName) &&
				IsPortValid(AdminPort) &&
				IsPortValid(ClientPort) &&
				(SleepTimeout > 100);
		} // IsValid

		#endregion method IsValid

		#region method IsPortValid

		protected virtual bool IsPortValid(int nPort) {
			return (1024 <= nPort) && (nPort <= 65535);
		} // IsPortValid

		#endregion method IsPortValid

		#endregion protected
	} // class ConfigurationData
} // namespace EzServiceConfiguration
