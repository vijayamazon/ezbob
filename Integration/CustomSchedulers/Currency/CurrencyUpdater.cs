namespace CustomSchedulers.Currency
{
	using System;
	using Scorto.Service.Scheduler;
	using log4net;

	public class CurrencyUpdater : ScheduledExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CurrencyUpdater));

        public override void ExecuteIteration()
        {
            try
            {
				CurrencyUpdateController.Run();   
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex);
            }

        }
    }
}
