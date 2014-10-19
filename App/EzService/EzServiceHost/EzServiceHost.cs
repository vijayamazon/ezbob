namespace EzServiceHost {
	using System;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.ServiceModel.Discovery;
	using EzService;
	using EzServiceConfigurationLoader;
	using EzService.EzServiceImplementation;

	public class EzServiceHost : ServiceHost {
		public EzServiceHost(Configuration oCfg, EzServiceInstanceRuntimeData oData) : base(
			typeof(EzServiceImplementation),
			new Uri(oCfg.AdminEndpointAddress),
			new Uri(oCfg.ClientEndpointAddress)
		) {
			m_oCfg = oCfg;
			m_oData = oData;

			var oTcpBinding = new NetTcpBinding {
				MaxReceivedMessageSize = 2147483647,
				MaxBufferSize = 2147483647,
			};

			AddServiceEndpoint(typeof(IEzServiceAdmin), oTcpBinding, m_oCfg.AdminEndpointAddress);

			// To enable HTTP binding on custom port: open cmd.exe as administrator and
			//     netsh http add urlacl url=http://+:7082/ user=ALEXBO-PC\alexbo
			// where 7082 is your customer port and ALEXBO-PC\alexbo is the user
			// who runs the instance of the host.
			// To remove permission:
			//     netsh http add urlacl url=http://+:7082/
			// Mind the backslash at the end of the URL.

			// TODO: restore with HTTPS

			AddServiceEndpoint(typeof(IEzService), oTcpBinding, m_oCfg.AdminEndpointAddress);

			//var oHttpBinding = new WSHttpBinding();
			//oHttpBinding.Security.Mode = SecurityMode.Transport;
			//oHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;

			//AddServiceEndpoint(typeof(IEzService), oHttpBinding, m_oCfg.ClientEndpointAddress);
		} // constructor

		#region protected

		protected override void OnOpening() {
			Description.Behaviors.Add(new EzInstanceProviderBehaviour(m_oData));
			Expose();
			base.OnOpening();
		} // OnOpening

		#endregion protected

		#region private

		#region method Expose

		private void Expose() {
			Description.Behaviors.Add(new ServiceDiscoveryBehavior());

			Description.Behaviors.Add(new ServiceMetadataBehavior());

			foreach (Uri baseAddress in BaseAddresses) {
				Binding binding = null;

				if (baseAddress.Scheme == "net.tcp")
					binding = MetadataExchangeBindings.CreateMexTcpBinding();
				else if (baseAddress.Scheme == "net.pipe")
					binding = MetadataExchangeBindings.CreateMexNamedPipeBinding();
				else if (baseAddress.Scheme == "http")
					binding = MetadataExchangeBindings.CreateMexHttpBinding();

				if (binding != null)
					AddServiceEndpoint(typeof(IMetadataExchange), binding, "MEX");
			} // for each base address

			AddServiceEndpoint(new UdpDiscoveryEndpoint());
		} // Expose

		#endregion method Expose
 
		private readonly EzServiceInstanceRuntimeData m_oData;
		private readonly Configuration m_oCfg;

		#endregion private
	} // class EzServiceHost
} // namespace EzServiceHost
