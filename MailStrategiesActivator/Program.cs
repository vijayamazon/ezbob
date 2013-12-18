using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailStrategiesActivator
{
	using EzBob.Backend.Strategies.MailStrategies;

	class Program
	{
		
		static void Main(string[] args)
		{
			string strategyName = "Greeting"; // extract from args

			switch (strategyName)
			{
				case "Greeting":
					int customerId = int.Parse("123"); // args1
					string confirmEmailAddress = "args2";
					var g = new Greeting(customerId, confirmEmailAddress);
					g.Execute();
					break;
				default:
					Console.WriteLine(string.Format("Unsupported strategy name:{0}", strategyName));
					break;
			}
		}
	}
}
