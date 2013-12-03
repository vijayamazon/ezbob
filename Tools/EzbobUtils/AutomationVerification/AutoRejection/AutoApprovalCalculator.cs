namespace AutomationCalculator
{
	using CommonLib;

	public class AutoApprovalCalculator
    {
		public bool IsAutoApproved(int customerId, out string reason)
		{
			var dbHelper = new DbHelper();
			var experianScore = dbHelper.GetExperianScore(customerId);
			var mps = dbHelper.GetCustomerMarketPlaces(customerId);
			var anualTurnover = AnalysisFunctionsHelper.GetTurnoverForPeriod(mps, TimePeriodEnum.Year);
			
			reason = "";
			return false;
		}
    }
}
