using System;
using System.ServiceModel;
using System.Threading;
using TestEzClient.EzService;

namespace TestEzClient {
	class Program {
		static void Main(string[] args) {
			EzServiceClient proxy = null;

			var aryActions = new Action<EzServiceClient>[] {
				TestGetData, TestGetDataUsingDataContract, TestGetData, TestGetDataUsingDataContract
			};

			foreach (Action<EzServiceClient> oAction in aryActions)
				proxy = TestOneAction(proxy, oAction);

			Console.WriteLine("Strike Enter to quit or 'exit' to shutdown the service...");
			string sResponse = Console.ReadLine();

			if (sResponse == "exit") {
				TestOneAction(proxy, TestShutdown);
				Console.WriteLine("Strike Enter when ready...");
				Console.ReadLine();
			} // if
		} // Main

		private static EzServiceClient TestOneAction(EzServiceClient proxy, Action<EzServiceClient> oAction) {
			if (proxy == null) {
				Console.WriteLine("Creating a proxy object...");

				proxy = new EzServiceClient();

				Console.WriteLine("Connecting a proxy object to service...");
				try {
					proxy.Open();
				}
				catch (Exception e) {
					Console.WriteLine("Failed to connect to the service: exception of type {1} caught: {0}", e.Message, e.GetType());
					return null;
				} // try
			} // if

			if (proxy.InnerChannel.State != CommunicationState.Opened) {
				Console.WriteLine("Cannot call the service: its state is {0}", proxy.InnerChannel.State);
				return null;
			} // if

			try {
				oAction(proxy);
			}
			catch (Exception e) {
				Console.WriteLine("GetData: exception of type {1} caught: {0}", e.Message, e.GetType());
			} // try

			Console.WriteLine("After the call service state is {0}", proxy.InnerChannel.State);

			if (proxy.InnerChannel.State != CommunicationState.Opened)
				return null;

			return proxy;
		} // TestOneAction

		private static void TestGetData(EzServiceClient proxy) {
			Console.WriteLine("GetData(12)...");
			string s = proxy.GetData(12);
			Console.WriteLine("GetData(12) = {0}", s);
		} // TestGetData

		private static void TestGetDataUsingDataContract(EzServiceClient proxy) {
			Console.WriteLine("GetDataUsingDataContract({ true, 'str' })...");
			CompositeType ct = proxy.GetDataUsingDataContract(new CompositeType {BoolValue = true, StringValue = "str"});
			Console.WriteLine("GetDataUsingDataContract({{ true, 'str' }}) = {{ {0}, {1} }}", ct.BoolValue, ct.StringValue);
		} // TestGetDataUsingDataContract

		private static void TestShutdown(EzServiceClient proxy) {
			Console.WriteLine("Shutdown()...");
			string s = proxy.Shutdown();
			Console.WriteLine("Shutdown() = {0}", s);
		} // TestShutdown
	} // class Program
} // namespace
