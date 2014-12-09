using System;
using System.Web.Services.Protocols;
using System.Threading;
using System.Collections;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Summary description for RefreshItem.
	/// </summary>
	public class RefreshAll : ApplicationSuper
	{		
        DataServiceService dataService;
		RefreshService	refresh;
		double REFRESH_TIME_OUT = 3; // in minutes
		int SLEEP_TIME = 10000; // in millis seconds

        public static int STATUS_OK = 0 ;

		/**
		 * Indicates a high refresh_priority, which is normally used
		 * while refreshing a single item on demand.
		 */
		public static int REFRESH_PRIORITY_HIGH  = 1;

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

		public RefreshAll()
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
		public void refreshAll(UserContext userContext)
		{
			System.Console.WriteLine("Attempting to start refresh");
			try
			{
				Entry[] refreshStatus = refresh.startRefresh2(userContext,REFRESH_PRIORITY_HIGH, true );
                for (int i = 0; i < refreshStatus.Length; i++)
                {
                    Entry status = refreshStatus[i];
                    long itemId = (long)status.key;
                    RefreshItem ri = new RefreshItem();
                    String mfaTypeId = ri.getMFATypeId(userContext, itemId);
                    if ( mfaTypeId != null ) {                	   
        			    MFARefreshInfo mfaInfo = refresh.getMFAResponse(userContext, itemId, true);
        			    MFA mfa = new MFA();
        			    long errorCode = mfa.processMFA(userContext, mfaInfo, itemId);
                        if (errorCode == 0) {
                            System.Console.WriteLine("MFA Account has been added successfully");
                        } else {
                            System.Console.WriteLine("Error while adding this account with an error code " + errorCode);
                        }
                    }
                    RefreshStatus refStatus = (RefreshStatus)status.value;
                    if (refStatus.Equals(RefreshStatus.SUCCESS_START_REFRESH))
					{
						System.Console.WriteLine("\tStarted refresh for " + itemId);
					} 
					else if(refStatus.Equals(RefreshStatus.REFRESH_ALREADY_IN_PROGRESS))
					{
						System.Console.WriteLine("\tThe refresh is already in progress for " + itemId);
					}
					else if (refStatus.Equals(RefreshStatus.ALREADY_REFRESHED_RECENTLY))
					{
						System.Console.WriteLine("\tItem " + itemId + " has been refreshed very recently. Please try again later.");
					}
					else if (refStatus.Equals(RefreshStatus.ITEM_CANNOT_BE_REFRESHED))
					{
						System.Console.WriteLine("The refresh on item " + itemId + " is not permitted.");
					}
					else 
					{
						System.Console.WriteLine("Unbale to refresh the item " + itemId + "\nRefreshStatus:" + status);
					}
				}

			}
			catch(SoapException se)
			{
				System.Console.WriteLine("The given item is invalid\nException:\n" + se.ToString() );
			}
		}

		public bool pollRefreshStatus(UserContext userContext)
		{
			DateTime d1 = DateTime.Now;
			DateTime d2 = d1.AddMinutes(REFRESH_TIME_OUT);
			Hashtable ht = new Hashtable();

			while(d2 > d1)
			{
				System.Console.Write("\n\tChecking the refresh status of items:");
				RefreshInfo[] refreshInfo = refresh.getRefreshInfo(userContext);

				// initilize hashmap
				for(int i=0; i<refreshInfo.Length; i++)
				{	
					//hm.Add(refreshInfo[i].itemId, false);
					ht.Remove(refreshInfo[i].itemId);
					ht.Add(refreshInfo[i].itemId, false);
					System.Console.Write(refreshInfo[i].itemId + " ");
				}

				for(int i=0; i<refreshInfo.Length; i++)
				{
					if(refreshInfo[i].refreshRequestTime == 0 )
					{
						long itemId = refreshInfo[i].itemId;
						int refreshStatusCode = refreshInfo[i].statusCode;
						if( refreshStatusCode == STATUS_OK )
						{
							//hm.Add(itemId, true);
							ht.Remove(itemId);
							ht.Add(itemId, true);

							System.Console.WriteLine("\n\tThe refresh of " + itemId + " has completed successfully.");
							if(doneRefreshing(ht))
							{
								return true;
							}
						}
						else
						{
							//hm.Add(itemId, true);
							ht.Remove(itemId);
							ht.Add(itemId, true);
							System.Console.WriteLine("\n\tThe refresh of " + itemId + " did not succed. Error code: " + refreshStatusCode );
							if(doneRefreshing(ht))
							{
								return true;
							}
						}

					}
					// Sleep
					System.Console.Write(".");
					Thread.Sleep(SLEEP_TIME);
					d1 = DateTime.Now;
					// System.Console.WriteLine("d1 = " + d1.ToLongTimeString()  );
				}
			}

			// Timeout the refresh request
			refresh.stopRefresh2(userContext,STOP_REFRESH_REASON_TIMEDOUT, true); 
			System.Console.WriteLine("\tThe refresh has timed out");
			return false;
		}

		public static bool doneRefreshing(Hashtable ht)
		{
			IDictionaryEnumerator en = ht.GetEnumerator();

			while (en.MoveNext())
			{
				//System.Console.WriteLine(en.Key + " : " + en.Value);
				if(!(Boolean)en.Value)
				{
					return false;
				}
			}
			return true;	
		}

	}
}
