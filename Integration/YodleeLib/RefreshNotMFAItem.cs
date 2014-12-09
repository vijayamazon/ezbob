namespace YodleeLib
{
	using System;
	using System.Threading;
	using ConfigManager;
	using log4net;

	/// <summary>
	/// Summary description for RefreshItem.
	/// </summary>
	public class RefreshNotMFAItem : ApplicationSuper
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(RefreshNotMFAItem));

		protected DataServiceService dataService;
		protected RefreshService refresh;
		protected double REFRESH_TIME_OUT = 3; // in minutes
		protected int SLEEP_TIME = 10000; // in millis seconds
		protected static int MFA_QUEUE_WAIT_TIME_MILLIS = 20000;

		/**
		 * Indicates a high refresh_priority, which is normally used
		 * while refreshing a single item on demand.
		 */
		public static int REFRESH_PRIORITY_HIGH = 1;
		public static int GATHERER_ERRORS_STATUS_OK = 0;
		/**
		 * Indicates a low refresh_priority, which is normally used
		 * while refreshing many items together.
		 */
		public static int REFRESH_PRIORITY_LOW = 2;

		/**
		 * Indicates the stop_refresh reason as "refresh timedout".
		 */
		public static int STOP_REFRESH_REASON_TIMEDOUT = 100;

		/**
		 * Indicates the stop_refresh reason as "refresh aborted by the user".
		 */
		public static int STOP_REFRESH_REASON_USER_ABORTED = 101;

		public RefreshNotMFAItem()
		{
			string soapServer = CurrentValues.Instance.YodleeSoapServer;
			dataService = new DataServiceService
				{
					Url = soapServer + "/" + "DataService"
				};
			refresh = new RefreshService
				{
					Url = soapServer + "/" + "RefreshService"
				};
		}

		/**
		 * Refreshes the specified item (not MFA)
		 */
		public void RefreshItem(UserContext userContext, long? itemId, bool forceRefresh = false)
		{
			var refreshParameters = new RefreshParameters
				{
					refreshMode = RefreshMode.NORMAL_REFRESH_MODE,
					refreshModeSpecified = true,
					refreshPriority = REFRESH_PRIORITY_HIGH,
					refreshPrioritySpecified = true,
					forceRefresh = forceRefresh
				};

			bool startRefresh7ReturnSpecified;
			RefreshStatus? status;
			refresh.startRefresh7(userContext,itemId, itemIdSpecified: true, refreshParameters: refreshParameters, startRefresh7Return: out status, startRefresh7ReturnSpecified: out startRefresh7ReturnSpecified);
			if (status.ToString().Equals(RefreshStatus.SUCCESS_START_REFRESH.ToString()))
			{
				Log.Debug("\tStarted refresh");
			}
			else 
			{
				throw new RefreshYodleeException(status.HasValue ? status.Value : RefreshStatus.STATUS_UNKNOWN);
			}
		}

		public bool PollRefreshStatus(UserContext userContext, long itemId)
		{

			var d1 = DateTime.Now;
			var d2 = d1.AddMinutes(REFRESH_TIME_OUT);

			long?[] itemIds = { itemId };
			while (d2 > d1)
			{
				RefreshInfo[] refreshInfo = refresh.getRefreshInfo1(userContext, itemIds);

				int refreshStatusCode = refreshInfo[0].statusCode;
				if (refreshStatusCode == GATHERER_ERRORS_STATUS_OK)
				{
					return true;
				}
				Log.Debug(string.Format("RefreshStatusCode: {0}", refreshStatusCode));

				// Sleep
				Thread.Sleep(SLEEP_TIME);
				d1 = DateTime.Now;
			}

			// Timeout the refresh request
			refresh.stopRefresh(userContext, itemId, true, STOP_REFRESH_REASON_TIMEDOUT, true);
			throw new TimeoutException("The refresh has timed out");
		}
	}
}
