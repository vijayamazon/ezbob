namespace AutomationCalculator
{
	using CommonLib;

	public class AutoReRejectionCalculator
    {
		public bool IsAutoReRejected(int customerId, out string reason)
		{
			var dbHelper = new DbHelper();
			var experianScore = dbHelper.GetExperianScore(customerId);
			var mps = dbHelper.GetCustomerMarketPlaces(customerId);
			var anualTurnover = AnalysisFunctionsHelper.GetTurnoverForPeriod(mps, TimePeriodEnum.Year);

			
			reason = "None of the rules";
			return false;
		}
    }
}
