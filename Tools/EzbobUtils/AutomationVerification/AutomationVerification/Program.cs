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
			Console.WriteLine(aj.IsAutoRejected(4998));
		}
	}
}
