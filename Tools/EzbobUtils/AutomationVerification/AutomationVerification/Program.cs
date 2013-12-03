using System;


namespace AutomationVerification
{
	using AutomationCalculator;

	class Program
	{
		static void Main(string[] args)
		{
			var aj = new AutoRejectionCalculator();
			string reason;
			bool isAutoRehected = aj.IsAutoRejected(14166, out reason); 
			Console.WriteLine("is rejected: {0} reason: {1}", isAutoRehected, reason);
		}
	}
}
