using System;
using System.ServiceModel;
using EzService;

namespace EzServiceHost {
	#region class EzServiceHost

	class EzServiceHost : ServiceHost {
		public EzServiceHost(EzServiceInstanceRuntimeData oData, Type oServiceType, params Uri[] aryBaseAddresses) : base(oServiceType, aryBaseAddresses) {
			m_oData = oData;
		} // constructor

		protected override void OnOpening() {
			Description.Behaviors.Add(new EzInstanceProviderBehaviour(m_oData));
			base.OnOpening();
		} // OnOpening

		private readonly EzServiceInstanceRuntimeData m_oData;
	} // class EzServiceHost

	#endregion class EzServiceHost
} // namespace EzServiceHost
