using System;

namespace com.yodlee.sampleapps
{
    public class SessionlessCall : ApplicationSuper
    {
        /** Navigation Counter. **/
        private static int optionCount = 1;
        /** Navigation Menu Choice. * */
        private static int NAV_SESSIONLESS_VIEW_ITEMS = optionCount++;
        /** Navigation Menu Choice. **/
        private static int NAV_QUIT = 0;

        DataServiceService dataService;

        public SessionlessCall()
		{
			dataService = new DataServiceService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
		}

        public void doSessionlessCallMenu()
        {
            bool loop = true;
            int choice = 0;
            while (loop)
            {
                try
                {
                    System.Console.WriteLine("Sessionless call Menu");
                    System.Console.WriteLine("********************");
                    System.Console.WriteLine(NAV_SESSIONLESS_VIEW_ITEMS + ". Sessionless View Items (non-SSO)");
                    System.Console.WriteLine(NAV_QUIT + ". Exit Sub-menu");
                    System.Console.WriteLine("********************");
                    System.Console.Write("Enter Choice : ");
                    choice = int.Parse(System.Console.ReadLine());

                    if (choice == NAV_SESSIONLESS_VIEW_ITEMS)
                    {
                        sessionlessViewItems();
                    }
                    else if (choice == NAV_QUIT)
                        loop = false;
                    else
                        System.Console.WriteLine("Invalid Entry!");
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.StackTrace);
                }
            }
        }
        public
    void sessionlessViewItems()
        {
            System.Console.Write("Login: ");

            String input = System.Console.ReadLine();
            String userName = null;

            if (input != null) {
                userName = input;
            }

            String password = "";
            System.Console.Write("Password: ");
            input = System.Console.ReadLine();

            if (input != null) {
                password = input;
            }
            System.Console.WriteLine("SessionLess core calls");
            PasswordCredentials passwordCredentials = new PasswordCredentials();
            passwordCredentials.loginName = userName;
            passwordCredentials.password = password;

            UserContext sessionlessUserContext = new UserContext();
            CobrandContext cobrandContext = getCobrandContext();

            sessionlessUserContext.cobrandConversationCredentials = cobrandContext.cobrandConversationCredentials;
            sessionlessUserContext.conversationCredentials = passwordCredentials;

            sessionlessUserContext.applicationId = cobrandContext.applicationId;
            sessionlessUserContext.channelId = cobrandContext.channelId;
            sessionlessUserContext.channelIdSpecified = true;
            sessionlessUserContext.cobrandId = cobrandContext.cobrandId;
            sessionlessUserContext.cobrandIdSpecified = true;
            sessionlessUserContext.ipAddress = cobrandContext.ipAddress;
            sessionlessUserContext.isPasswordExpired = false;
            sessionlessUserContext.locale = cobrandContext.locale;
            sessionlessUserContext.preferenceInfo = cobrandContext.preferenceInfo;
            sessionlessUserContext.tncVersion = cobrandContext.tncVersion;
            sessionlessUserContext.tncVersionSpecified = true;
            sessionlessUserContext.valid = true;
            sessionlessUserContext.validationHandler = cobrandContext.validationHandler;

            Object[] itemSummaries = (Object[])dataService.getItemSummaries(sessionlessUserContext);
            if (itemSummaries == null || itemSummaries.Length == 0)
            {
                System.Console.WriteLine("You have no Items Added.");
            }
            else
            {
                for (int i = 0; i < itemSummaries.Length; i++)
                {
                    ItemSummary itemSummary = (ItemSummary)itemSummaries[i];
                    String displayName = itemSummary.contentServiceInfo.contentServiceDisplayName;
                    System.Console.WriteLine("ItemId: " + itemSummary.itemId + " DisplayName: "
                        + displayName + " errorCode: " + itemSummary.refreshInfo.statusCode +
                           " refreshInfo time: " /**new Date(itemSummary.refreshInfo.lastUpdatedTime * 1000)*/);
                }
            }

        }
    }
}
