namespace ServiceClientProxy {
	using System;
	using System.ServiceModel;
	using EzServiceReference;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ServiceClient {

		public EzServiceClient Instance {
			get {
				lock (ms_oInstanceLock) {
					if (ReferenceEquals(ms_oServiceClient, null) || (ms_oServiceClient.State != CommunicationState.Opened && ms_oServiceClient.State != CommunicationState.Created)) {
						if (ms_oServiceClient != null)
							ms_oLog.Debug("ServiceClient State: {0}.", ms_oServiceClient.State);
						else
							ms_oLog.Debug("ServiceClient is null, creating new.");

						try {
							var cfg = new EzServiceConfigurationLoader.DefaultConfiguration(
								Environment.MachineName,
								DbConnectionGenerator.Get(),
								ms_oLog
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
							ms_oLog.Debug(e, "Failed to connect to EzService.");

							// TODO: save to DB failed request to run it later...

							throw; // TODO: remove this after the previous TODO is implemented
						} // try
					}// if
				} //lock

				return ms_oServiceClient;
			} // get
		} // ServiceClient

		private static EzServiceClient ms_oServiceClient;
		private static readonly object ms_oInstanceLock = new Object();

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(ServiceClient));
	} // class ServiceClient
} // namespace
