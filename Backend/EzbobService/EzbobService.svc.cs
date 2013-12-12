namespace EzBob.Backend.EzbobService
{
	using log4net;
	using Strategies;

	public class EzbobService : IEzbobService
	{
		private readonly Strategies strategies = new Strategies();
		private static readonly ILog log = LogManager.GetLogger(typeof(EzbobService));

		public void Greeting(int custumerId, string confirmEmailAddress)
		{
			strategies.Greeting(custumerId, confirmEmailAddress);
		}

		/* Examples
		public string GetData(int value)
		{
			return string.Format("You entered: {0}", value);
		}

		public CompositeType GetDataUsingDataContract(CompositeType composite)
		{
			if (composite == null)
			{
				throw new ArgumentNullException("composite");
			}
			if (composite.BoolValue)
			{
				composite.StringValue += "Suffix";
			}
			return composite;
		}
		 */
	}
}
