using System;
using System.Diagnostics;
using System.ServiceModel;

namespace EzService {
	[ServiceBehavior(
		InstanceContextMode = InstanceContextMode.PerCall,
		IncludeExceptionDetailInFaults = true
	)]
	public class EzServiceImplementation : IEzService, IDisposable {
		#region public

		public EzServiceImplementation(EzServiceInstanceRuntimeData oData) {
			m_oData = oData;
		} // constructor

		#region Service exposed methods

		public string GetData(int value) {
			try {
				throw new Exception("Ex " + value);
				return string.Format("You entered: {0} {1}", value, (this.m_oData == null ? "null" : "not null"));
			}
			catch (Exception e) {
				throw new FaultException(e.Message);
			}
		} // GetData

		public CompositeType GetDataUsingDataContract(CompositeType composite) {
			try {
				if (composite == null) {
					throw new ArgumentNullException("composite");
				}

				composite.StringValue += " " + (this.m_oData == null ? "null" : "not null") + " ";

				if (composite.BoolValue) {
					composite.StringValue += "Suffix";
				}

				return composite;
			}
			catch (Exception e) {
				throw new FaultException(e.Message);
			}
		} // GetDataUsingDataContract

		#region method Shutdown

		public string Shutdown() {
			try {
				var x = DateTime.Now.ToString() + " data is " + (this.m_oData == null ? "null" : "not null");

				if ((m_oData != null) && (m_oData.Host != null))
					m_oData.Host.Shutdown();

				return x;
			}
			catch (Exception e) {
				throw new FaultException(e.Message);
			}
		} // Shutdown

		#endregion method Shutdown

		#endregion Service exposed methods

		#region method IDisposable.Dispose

		public void Dispose() {
			// TODO: empty so far
		} // Dispose

		#endregion method IDisposable.Dispose

		private readonly EzServiceInstanceRuntimeData m_oData;

		#endregion public
	} // class EzServiceImplementation
} // namespace EzService
