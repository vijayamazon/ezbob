using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationVerification
{
	class Program
	{
		static void Main(string[] args)
		{
			var aj = new AutoRejection.AutoRejectionCalculator();
			string reason;
			bool isAutoRehected = aj.IsAutoRejected(14166, out reason); 
			Console.WriteLine("is rejected: {0} reason: {1}", isAutoRehected, reason);
		}
	}
}
