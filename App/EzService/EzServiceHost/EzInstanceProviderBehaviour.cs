namespace EzServiceHost {
	using System.Collections.ObjectModel;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.ServiceModel.Dispatcher;
	using EzService;

	class EzInstanceProviderBehaviour : IServiceBehavior {
		public EzInstanceProviderBehaviour(EzServiceInstanceRuntimeData oData) {
			m_oData = oData;
		} // constructor

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {
		} // Validate

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) {
		} // AddBindingParameters

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {
			foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
				foreach (EndpointDispatcher ed in cd.Endpoints)
					if (!ed.IsSystemEndpoint)
						ed.DispatchRuntime.InstanceProvider = new EzServiceInstanceCreator(m_oData);
		} // ApplyDispatchBehavior

		private readonly EzServiceInstanceRuntimeData m_oData;
	} // class EzInstanceProviderBehaviour
} // namespace EzServiceHost
