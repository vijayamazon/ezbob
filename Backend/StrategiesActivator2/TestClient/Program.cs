namespace BiztalkTestClient
{
	using StasEzService;

	class Program
	{
		private static void Main(string[] args)
		{
			var c = new EzServiceClient();
			var check = c.FraudChecker(25, FraudMode.FullCheck);
			xmlHelper.SerializeObject(check, "FraudChecker");
		}
	}
}
