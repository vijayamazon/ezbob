using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using EzService;

namespace EzServiceHost {
	public class Program : IHost {
		#region public

		#region method Shutdown

		public void Shutdown() {
			lock (ms_oLock) {
				ms_bStop = true;
			} // lock
		} // Handle

		#endregion method Shutdown

		#endregion public

		#region private

		#region method Main

		private static void Main(string[] args) {
			try {
				var oData = new EzServiceInstanceRuntimeData {
					Host = new Program()
				};

				// TODO: init log and store it into oData

				// TODO: init DB connection and store it into oData

				int nListeningPort = 7081; // TODO: configurable

				string sBaseAddress = "net.tcp://localhost:" + nListeningPort;

				var oHost = new EzServiceHost(oData, typeof (EzServiceImplementation), new Uri(sBaseAddress));

				SetMetadataEndpoit(oHost);

				Binding tcpBinding = new NetTcpBinding();
				oHost.AddServiceEndpoint(typeof (IEzService), tcpBinding, sBaseAddress);

				oHost.Open();

				MainLoop(1000); // TODO: configurable

				oHost.Close();

				// TODO: close db connection

				// TODO: close log
			}
			catch (Exception e) {
				Console.WriteLine("Exception caught " + e.Message);
			}
		} // Main

		#endregion method Main

		#region method SetMetadataEndpoint

		private static void SetMetadataEndpoit(EzServiceHost oHost) {
			ServiceMetadataBehavior metadataBehavior = oHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

			if (metadataBehavior == null) {
				metadataBehavior = new ServiceMetadataBehavior();
				oHost.Description.Behaviors.Add(metadataBehavior);
			} // if

			Binding binding = MetadataExchangeBindings.CreateMexTcpBinding();
			oHost.AddServiceEndpoint(typeof(IMetadataExchange), binding, "MEX");
		} // SetMetadataEndpoint

		#endregion method SetMetadataEndpoint

		#region method MainLoop

		private static void MainLoop(int nSleeptime) {
			bool bStop = false;

			do {
				Thread.Sleep(nSleeptime);

				lock (ms_oLock) {
					bStop = ms_bStop;
				} // lock
			} while (!bStop);
		} // MainLoop

		#endregion method MainLoop

		private static bool ms_bStop = false;
		private static readonly object ms_oLock = new object();

		#endregion private
	} // class Program
} // namespace EzServiceHost
