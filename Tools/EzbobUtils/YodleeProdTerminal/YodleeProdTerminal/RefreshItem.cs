using System;
using System.Web.Services.Protocols;
using System.Threading;
using com.yodlee.sampleapps.util;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Summary description for RefreshItem.
	/// </summary>
	public class RefreshItem : ApplicationSuper
	{
        protected DataServiceService dataService;
        protected RefreshService refresh;
        protected double REFRESH_TIME_OUT = 3; // in minutes
        protected int SLEEP_TIME = 10000; // in millis seconds
        protected static int MFA_QUEUE_WAIT_TIME_MILLIS = 20000;

		/**
		 * Indicates a high refresh_priority, which is normally used
		 * while refreshing a single item on demand.
		 */
		public static int REFRESH_PRIORITY_HIGH  = 1;
        public static int GATHERER_ERRORS_STATUS_OK = 0;
		/**
		 * Indicates a low refresh_priority, which is normally used
		 * while refreshing many items together.
		 */
		public static int REFRESH_PRIORITY_LOW   = 2;

		/**
		 * Indicates the stop_refresh reason as "refresh timedout".
		 */
		public static int STOP_REFRESH_REASON_TIMEDOUT      = 100;

		/**
		 * Indicates the stop_refresh reason as "refresh aborted by the user".
		 */
		public static int STOP_REFRESH_REASON_USER_ABORTED   = 101;

		public RefreshItem()
		{
			dataService = new DataServiceService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
			refresh = new RefreshService();
            refresh.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "RefreshService";
		}

        /**
         * Refreshes the specified item
         * 
         */
        public void refreshItem(UserContext userContext, long? itemId, bool isMFA)
        {
            System.Console.WriteLine("Attempting to start refresh");
         //   bool startRefresh4ReturnSpecified = true;
            try
            {
                int REFRESH_PRIORITY_HIGH = 1;
                RefreshParameters refreshParameters = new RefreshParameters();
                RefreshMode refreshMode;
                if (isMFA)
                    refreshMode = RefreshMode.MFA_REFRESH_MODE;
                else
                    refreshMode = RefreshMode.NORMAL_REFRESH_MODE;

                refreshParameters.refreshMode = refreshMode;
                refreshParameters.refreshModeSpecified = true;
                refreshParameters.refreshPriority = REFRESH_PRIORITY_HIGH;
                refreshParameters.refreshPrioritySpecified = true;
                refreshParameters.forceRefresh = false;

                bool itemIdspecified = true;
                bool startRefresh7ReturnSpecified;
                RefreshStatus? status;
                refresh.startRefresh7(userContext, itemId, itemIdspecified, refreshParameters, out status, out startRefresh7ReturnSpecified);
                if (status.ToString().Equals(RefreshStatus.SUCCESS_START_REFRESH.ToString()))
                {
                    System.Console.WriteLine("\tStarted refresh");
                }
                else if (status.ToString().Equals(RefreshStatus.REFRESH_ALREADY_IN_PROGRESS.ToString()))
                {
                    System.Console.WriteLine("\tThe refresh is already in progress");
                }
                else if (status.ToString().Equals(RefreshStatus.ALREADY_REFRESHED_RECENTLY.ToString()))
                {
                    throw new Exception("The item has been refreshed very recently. Please try again later.");
                }
                else if (status.ToString().Equals(RefreshStatus.ITEM_CANNOT_BE_REFRESHED.ToString()))
                {
                    throw new Exception("The refresh on this item is not permitted.");
                }
                else
                {
                    throw new Exception("Unbale to refresh the item\nRefreshStatus:" + status);
                }
                if (isMFA)
                {
                    //We might need delay here to give few seconds for the message to be posted since this is a console based. 
                    //In an application, it might not be required. 
                    Thread.Sleep(MFA_QUEUE_WAIT_TIME_MILLIS);
                    MFARefreshInfo mfaInfo = refresh.getMFAResponse(userContext, itemId, true);
                    MFA mfa = new MFA();
                    long errorCode = mfa.processMFA(userContext, mfaInfo, itemId);
                    if (errorCode == 0) {
                        System.Console.WriteLine("MFA Account has been added successfully");
                    } else if (errorCode > 0) {                        
                        System.Console.WriteLine("Error while adding this account with an error code " + errorCode);
                    } else {
                        System.Console.WriteLine("Error while adding this account");
                    }
                }
            }
            catch (SoapException se)
            {
                System.Console.WriteLine("The given item is invalid\nException:\n" + se.ToString());
            }
        }

		public bool pollRefreshStatus(UserContext userContext, long itemId)
		{

			DateTime d1 = DateTime.Now;
			DateTime d2 = d1.AddMinutes(REFRESH_TIME_OUT);

			// System.Console.WriteLine("d1 = " + d1.ToLongTimeString() );
			// System.Console.WriteLine("d2 = " + d2.ToLongTimeString() );

			long?[] itemIds = {itemId}; 
			while(d2 > d1)
			{
				System.Console.WriteLine("\tChecking the refresh status...");
				RefreshInfo[] refreshInfo = refresh.getRefreshInfo1(userContext, itemIds);
				//if(refreshInfo[0].refreshRequestTime == 0 )
				//{
					int refreshStatusCode = refreshInfo[0].statusCode;
                    if (refreshStatusCode == GATHERER_ERRORS_STATUS_OK)
					{
						System.Console.WriteLine("\tThe refresh has completed successfully.");
						return true;
					}
					else
					{
						System.Console.WriteLine("\tThe refresh did not succed. Error code: " + refreshStatusCode );
						return false;
					}

				//}
				// Sleep
				Thread.Sleep(SLEEP_TIME);
				d1 = DateTime.Now;
				// System.Console.WriteLine("d1 = " + d1.ToLongTimeString()  );
			}

			// Timeout the refresh request
			refresh.stopRefresh(userContext,itemId,true,STOP_REFRESH_REASON_TIMEDOUT,true); 
			System.Console.WriteLine("\tThe refresh has timed out");
			return false;
		}

        /**
         * This method is to find the MFAType of the content service given 
         * the itemId and Usercontext      
         * @param userContext
         * @param itemId
         * @return
         */
        public String getMFATypeId(UserContext userContext, long itemId)
        {
            ItemSummary itemSummary = dataService.getItemSummaryForItem(userContext, itemId, true);
            long contentserviceId = itemSummary.contentServiceInfo.contentServiceId;
            ContentServiceHelper csh = new ContentServiceHelper(userContext);
            return csh.getMfATypeId(contentserviceId);
        }

	}
}
