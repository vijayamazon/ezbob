namespace EzBob.CommonLib.TrapForThrottlingLogic
{
	internal class TrapForThrottlingSettings
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="requestQuota">Requests Quota</param>
		/// <param name="restoreRateInSeconds">Restore Rate( seconds)</param>
		/// <param name="limitAccessPercentOfRequestQuota"></param>
		public TrapForThrottlingSettings(int requestQuota, int restoreRateInSeconds, int limitAccessPercentOfRequestQuota)
		{
			RequestQuota = requestQuota;
			RestoreRateInSeconds = restoreRateInSeconds;
			LimitAccessPercentOfRequestQuota = limitAccessPercentOfRequestQuota > 100 ? 100 : limitAccessPercentOfRequestQuota < 0 ? 0 : limitAccessPercentOfRequestQuota;
		}

		public  int RequestQuota { get; private set; }
		public int RestoreRateInSeconds { get; private set; }
		public int LimitAccessPercentOfRequestQuota { get; set; }
	}
}