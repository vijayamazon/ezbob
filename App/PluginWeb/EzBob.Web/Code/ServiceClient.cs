namespace EzBob.Web.Code {
	using System;
	using System.ServiceModel;
	using EzServiceReference;
	using Ezbob.Logger;
	using log4net;

	public static class ServiceClient {
		#region property ServiceClient

		public static EzServiceClient Instance {
			get {
				if (ReferenceEquals(ms_oServiceClient, null) || (ms_oServiceClient.State != CommunicationState.Opened)) {
					ILog oLog = LogManager.GetLogger(typeof (ServiceClient));

					try {
						var cfg = new EzServiceConfigurationLoader.Configuration(
							System.Environment.MachineName,
							DbConnectionGenerator.Get(),
							new SafeILog(oLog)
							);

						cfg.Init();

						var oTcpBinding = new NetTcpBinding {
							MaxBufferPoolSize = 524288,
							MaxBufferSize = 65536000,
							MaxReceivedMessageSize = 65536000
						};

						ms_oServiceClient = new EzServiceClient(
							oTcpBinding, // TODO: HTTPS...
							new EndpointAddress(cfg.AdminEndpointAddress) // TODO: when HTTPS is ready make it ClientAdminEndpoint
							);

						ms_oServiceClient.InnerChannel.OperationTimeout = TimeSpan.FromSeconds(cfg.ClientTimeoutSeconds);
					}
					catch (Exception e) {
						oLog.Debug("Failed to connect to EzService", e);

						// TODO: save to DB failed request to run it later...

						throw; // TODO: remove this after the previous TODO is implemented
					} // try
				} // if

				return ms_oServiceClient;
			} // get
		} // ServiceClient

		private static EzServiceClient ms_oServiceClient;

		#endregion property ServiceClient
	} // class ServiceClient
} // namespace
