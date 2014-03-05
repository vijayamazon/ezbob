namespace EzBob.Web.Code
{
	using System;
	using System.ServiceModel;
	using EzServiceReference;
	using Ezbob.Logger;
	using log4net;

	public class ServiceClient
	{
		#region property ServiceClient
		private static readonly ILog oLog = LogManager.GetLogger(typeof(ServiceClient));

		public static EzServiceClient Instance
		{
			
			get
			{
				lock (syncRoot)
				{
					if (ReferenceEquals(ms_oServiceClient, null) || (ms_oServiceClient.State != CommunicationState.Opened && ms_oServiceClient.State != CommunicationState.Created)  )
					{
						if (ms_oServiceClient != null)
						{
							oLog.DebugFormat("ServiceClient State: {0}", ms_oServiceClient.State);
						}
						else
						{
							oLog.DebugFormat("ServiceClient is null creating new");
						}


						try
						{
							var cfg = new EzServiceConfigurationLoader.DefaultConfiguration(
								System.Environment.MachineName,
								DbConnectionGenerator.Get(),
								new SafeILog(oLog)
								);

							cfg.Init();

							var oTcpBinding = new NetTcpBinding
								{
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
						catch (Exception e)
						{
							oLog.Debug("Failed to connect to EzService", e);

							// TODO: save to DB failed request to run it later...

							throw; // TODO: remove this after the previous TODO is implemented
						} // try
					}// if
				} //lock

				return ms_oServiceClient;
			} // get
		} // ServiceClient

		private static volatile EzServiceClient ms_oServiceClient;
		private static object syncRoot = new Object();

		#endregion property ServiceClient
	} // class ServiceClient
} // namespace
