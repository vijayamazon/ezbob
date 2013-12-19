namespace StrategiesActivator
{
	using System;
	using EzBob.Backend.Strategies.MailStrategies;

	public class StrategiesActivator
	{
		private readonly string[] args;

		public StrategiesActivator(string[] args)
		{
			this.args = args;
		}

		public void Execute()
		{
			string strategyName = args[0];
			switch (strategyName)
			{
				case "Greeting":
					ActivateGreeting();
					break;
				default:
					Console.WriteLine("Strategy {0} is not supported", strategyName);
					break;
			}
		}

		private void ActivateGreeting()
		{
			int customerId;
			if (args.Length != 3 || !int.TryParse(args[1], out customerId))
			{
				Console.WriteLine("Usage: StrategiesActivator.exe Greeting <CustomerId> <ConfirmEmailAddress>");
				return;
			}
			new Greeting(customerId, args[2]).Execute();
		}
	}
}
