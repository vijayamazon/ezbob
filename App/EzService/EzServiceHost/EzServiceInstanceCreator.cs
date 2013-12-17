using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using EzService;

namespace EzServiceHost {
	#region class EzServiceInstanceCreator

	class EzServiceInstanceCreator : IInstanceProvider {
		public EzServiceInstanceCreator(EzServiceInstanceRuntimeData oData) {
			m_oDataHolder = oData;
		} // constructor

		public object GetInstance(InstanceContext instanceContext, Message message) {
			return new EzServiceImplementation(m_oDataHolder);
		} // GetInstance

		public object GetInstance(InstanceContext instanceContext) {
			return this.GetInstance(instanceContext, null);
		} // GetInstance

		public void ReleaseInstance(InstanceContext instanceContext, object instance) {
		} // ReleaseInstance

		private readonly EzServiceInstanceRuntimeData m_oDataHolder;
	} // class EzServiceInstanceCreator

	#endregion class EzServiceInstanceCreator
} // namespace EzServiceHost
