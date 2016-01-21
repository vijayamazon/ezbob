namespace SalesForceRestApiTestClient {
	using System;

	class Program {
		private static void Main() {
			var service = new SalesForceService();
			service.CreateBrokerAccount().Wait();
			service.GetAccountByID(null, null).Wait();
			Console.WriteLine("End");
		}
	}
}
