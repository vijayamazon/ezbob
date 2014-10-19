namespace EzServiceHost {
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Dispatcher;
	using EzService;
	using EzService.EzServiceImplementation;

	class EzServiceInstanceCreator : IInstanceProvider {
		public EzServiceInstanceCreator(EzServiceInstanceRuntimeData oData) {
			m_oData = oData;
		} // constructor

		public object GetInstance(InstanceContext instanceContext, Message message) {
			return new EzServiceImplementation(m_oData);
		} // GetInstance

		public object GetInstance(InstanceContext instanceContext) {
			return this.GetInstance(instanceContext, null);
		} // GetInstance

		public void ReleaseInstance(InstanceContext instanceContext, object instance) {
		} // ReleaseInstance

		private readonly EzServiceInstanceRuntimeData m_oData;
	} // class EzServiceInstanceCreator
} // namespace EzServiceHost
