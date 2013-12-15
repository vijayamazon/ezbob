namespace EzBob.Backend.Strategies
{
	using Models;
	using log4net;

	public class FirstOfMonthStatusNotifier
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(FirstOfMonthStatusNotifier));

		public void Execute()
		{
			new FirstOfMonthStatusStrategyHelper().SendFirstOfMonthStatusMail();
		}
	}
}
