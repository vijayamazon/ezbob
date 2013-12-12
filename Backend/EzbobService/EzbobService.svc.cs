namespace EzBob.Backend.EzbobService
{
	using Strategies.MailStrategies;
	using log4net;

	public class EzbobService : IEzbobService
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EzbobService));

		public void Greeting(int custumerId, string confirmEmailAddress)
		{
			new Greeting(custumerId, confirmEmailAddress).Execute();
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
