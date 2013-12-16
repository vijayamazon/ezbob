namespace EzBob.Backend.Strategies
{
	using log4net;

	public class CaisGenerator
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(CaisGenerator));
		private readonly StrategiesMailer mailer = new StrategiesMailer();
		
		
		private object caisGenerationLock = new object();
		private int caisGenerationTriggerer = -1;
		public void CAISGenerate(int underwriterId)
		{
			lock (caisGenerationLock)
			{
				if (caisGenerationTriggerer != -1)
				{
					log.WarnFormat("A CAIS generation is already in progress. Triggered by Underwriter:{0}", caisGenerationTriggerer);
					return;
				}
				caisGenerationTriggerer = underwriterId;
			}
			
			// TODO: complete implementation - CAIS_NO_Upload

			lock (caisGenerationLock)
			{
				caisGenerationTriggerer = -1;
			}
		}

		public void CAISUpdate(int caisId)
		{
			// TODO: complete implementation - CAIS_NO_Upload
		}
	}
}
