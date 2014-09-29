using System;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using com.yodlee.sampleapps.util;

namespace com.yodlee.sampleapps
{
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using YodleeProdTerminal;

	/// <summary>
	/// Yodlee Console Application
	/// </summary>
	public class YodleeConsoleMain : ApplicationSuper
	{
		public UserContext userContext = null;
		public String userName = null;

		// Navigation
		private static int OPTION_CNT = 0;
		private static int NAV_QUIT = OPTION_CNT++;
		private static int NAV_REG_USER = OPTION_CNT++;
		private static int NAV_SSO_REG_USER = OPTION_CNT++;
		private static int NAV_LOGIN_USER = OPTION_CNT++;
		private static int NAV_LOGIN_USER2 = OPTION_CNT++;
		private static int NAV_SSO_LOGIN_USER = OPTION_CNT++;
		private static int NAV_SESSIONLESSCALL_SUBMENU = OPTION_CNT++;
		private static int NAV_UNREGISTER_USER = OPTION_CNT++;
		private static int NAV_GETOAUTHTOKEN = OPTION_CNT++;
		private static int NAV_ITEM_MANAGEMENT = OPTION_CNT++;
		private static int NAV_TRANSACTIONS = OPTION_CNT++;
		private static int NAV_MANAGE_ALERTS = OPTION_CNT++;
		private static int NAV_MANAGE_CONTENT_SERVICES = OPTION_CNT++;
		private static int itemOptionCount = 1;
		/** Navigation Menu Choice. **/
		private static int NAV_VIEW_ITEMS = itemOptionCount++;
		/** Navigation Menu Choice. **/
		private static int NAV_VIEW_ITEM = itemOptionCount++;
		/** Navigation Menu Choice. **/
		private static int NAV_DUMP_ITEM = itemOptionCount++;
		/** Navigation Menu Choice. **/
		private static int NAV_ADD_ITEM = itemOptionCount++;

		private static int NAV_EDIT_ITEM = itemOptionCount++;
		/** Navigation Menu Choice. **/
		private static int NAV_REMOVE_ITEM = itemOptionCount++;
		/** Navigation Menu Choice. **/
		private static int NAV_REFRESH_ITEM = itemOptionCount++;
		/** Navigation Menu Choice. **/
		private static int NAV_REFRESH_ALL = itemOptionCount++;
		/** Navigation Menu Choice. **/


		// Services

		ContentServiceTraversalService contentServieTravelService = null;
		ServerVersionManagementService serverVersionManagementService = null;

		//Helper CS files

		//ItemManagementMenu itemManagementMenu;

		public YodleeConsoleMain()
		{
			contentServieTravelService = new ContentServiceTraversalService();
			contentServieTravelService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + contentServieTravelService.GetType().FullName;
			serverVersionManagementService = new ServerVersionManagementService();
			serverVersionManagementService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + serverVersionManagementService.GetType().FullName;
			printVersionInfo();
			//itemManagementMenu = new ItemManagementMenu(getCobrandContext(),userContext);
		}


		private void printVersionInfo()
		{
			ServerVersion serverVersion =
				serverVersionManagementService.getServerVersion(getCobrandContext());

			System.Console.WriteLine("\n**** Server Version ****");

			System.Console.WriteLine(serverVersion.majorVersion + "." +
									 serverVersion.minorVersion + "." +
									 serverVersion.patch + "." +
									 serverVersion.maintenance + "_" +
									 serverVersion.buildDate + "_" +
									 serverVersion.buildTime);

			string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

			System.Console.WriteLine("\n**** Yodlee.NET.dll version Information ****");
			//Assembly asmembly = Assembly.LoadFile(appPath + "\\Yodlee.NET.dll");

			//System.Console.WriteLine(asmembly.ToString());

			//object[] objDescription = asmembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
			//System.Console.WriteLine(((AssemblyDescriptionAttribute)objDescription[0]).Description);

			System.Console.WriteLine("\n");

		}

		public void printChoices()
		{
			System.Console.WriteLine("********************");
			if (userContext != null)
			{
				System.Console.WriteLine("Welcome " + userName);
			}
			else
			{
				System.Console.WriteLine("Please Login!");
			}
			System.Console.WriteLine("********************");


			System.Console.WriteLine(NAV_REG_USER + ". Register User");
			System.Console.WriteLine(NAV_SSO_REG_USER + ". SSO Register User");
			System.Console.WriteLine(NAV_LOGIN_USER + ". Login User (from db)");
			System.Console.WriteLine(NAV_LOGIN_USER2 + ". Login User creds");
			System.Console.WriteLine(NAV_SSO_LOGIN_USER + ". SSO Login User");
			System.Console.WriteLine(NAV_SESSIONLESSCALL_SUBMENU + ". Sessionless Call [sub menu]");

			if (userContext != null)
			{
				System.Console.WriteLine(NAV_UNREGISTER_USER + ". Unregister User");
				System.Console.WriteLine(NAV_GETOAUTHTOKEN + ". Get Oauth Token");
				System.Console.WriteLine(NAV_ITEM_MANAGEMENT + ". Item Management [sub menu]");
				System.Console.WriteLine(NAV_TRANSACTIONS + ". Transactions [sub menu]");
				System.Console.WriteLine(NAV_MANAGE_ALERTS + ". Manage Alerts [sub menu]");
				System.Console.WriteLine(NAV_MANAGE_CONTENT_SERVICES + ". Manage Content Services [sub menu]");
			}
			System.Console.WriteLine(NAV_QUIT + ". Quit");
			System.Console.WriteLine("********************");
		}

		public void loop()
		{
			Boolean loop = true;
			while (loop)
			{
				printChoices();
				System.Console.Write("Choice: ");
				long choice = IOUtils.readInt();
				System.Console.WriteLine();
				try
				{
					if (choice == NAV_REG_USER)
					{
						registerUser();
					}
					else if (choice == NAV_SSO_REG_USER)
					{
						ssoRegisterUser();
					}
					else if (choice == NAV_LOGIN_USER)
					{
						loginUser();
					}
					else if (choice == NAV_LOGIN_USER2)
					{
						loginUser("","");
					}
					else if (choice == NAV_SSO_LOGIN_USER)
					{
						ssoLoginUser();
					}
					else if (choice == NAV_SESSIONLESSCALL_SUBMENU)
					{
						sessionlessCallMenu();
					}
					else if (choice == NAV_UNREGISTER_USER)
					{
						unregisterUser();
					}
					else if (choice == NAV_GETOAUTHTOKEN)
					{
						getAccessToken();
					}
					else if (choice == NAV_ITEM_MANAGEMENT)
					{
						itemManagementMenu();
					}
					else if (choice == NAV_MANAGE_CONTENT_SERVICES)
					{
						contentServiceMenu();
					}
					else if (choice == NAV_TRANSACTIONS)
					{
						transactionsMenu(userContext);
					}
					else if (choice == NAV_QUIT)
					{
						System.Environment.Exit(0);
					}
					else
					{
						System.Console.WriteLine("Invalid Entry. Try Again.");
					}
				}
				catch (SoapException soapEx)
				{

					System.Console.WriteLine("Other Exception: " + soapEx.Message);
				}
			}
		}


		private void ssoRegisterUser()
		{
			throw new NotImplementedException();
		}

		public void transactionsMenu(UserContext userContext)
		{
			Transactions transactions = new Transactions();
			transactions.doMenu(userContext);
		}

		private void ssoLoginUser()
		{
			System.Console.WriteLine("Enter Content ServiceId: ");
			long csid = IOUtils.readInt();
			AddItem ad = new AddItem();
			//long itemId = ad.doAddItem(userContext, csid);

			//throw new NotImplementedException();
		}

		public void registerUser()
		{
			DateTime currTime = DateTime.Now;
			userName = "user_" + currTime.Date.Ticks;
			System.Console.Write("Login : ");
			userName = IOUtils.readStr();

			String password = "test123";
			System.Console.Write("Password : ");
			password = IOUtils.readStr();

			String email = "sbarham@yodlee.com";
			System.Console.Write("Email : ");
			email = IOUtils.readStr();

			System.Console.WriteLine("Registering users with:\n\tuserName=" + userName
				+ "\n\tpassword=" + password + "\n\temail=" + email);

			RegisterUser registerUser = new RegisterUser();
			try
			{
				userContext = registerUser.registerUser(userName, password, email);
				System.Console.WriteLine("Registration of user '" + userName + "' successful");
				//CertifyUser certifyUser = new CertifyUser();
				//userContext = certifyUser.certifyUser(userContext);
			}
			catch (SoapException se)
			{
				System.Console.WriteLine("Exception Message -> " + se.Message);
				if (se.Message.Equals("IllegalArgumentValueExceptionFaultMessage"))
				{
					System.Console.WriteLine("\n\nGot Illegal Arguments for Registration.");
					System.Console.WriteLine("Please note that Yodlee enforces the following restrictions:");
					System.Console.WriteLine("On username:");
					System.Console.WriteLine("  >= 3 characters");
					System.Console.WriteLine("  <= 150 characters");
					System.Console.WriteLine("  No Whitespace");
					System.Console.WriteLine("  No Control Characters");
					System.Console.WriteLine("  Contains at least one Letter");
					System.Console.WriteLine("\nOn password");
					System.Console.WriteLine("  >= 6 characters");
					System.Console.WriteLine("  <= 50 characters");
					System.Console.WriteLine("  No Whitespace");
					System.Console.WriteLine("  No Control Characters");
					System.Console.WriteLine("  Contains at least one Number");
					System.Console.WriteLine("  Contains at least one Letter");
					System.Console.WriteLine("  Does not contain the same letter/number three or more times in a row.  (e.g. aaa123 would fail for three \"a\"'s in a row, but a1a2a3 would pass)");
					System.Console.WriteLine("  Does not equal username");
					System.Console.WriteLine("\n");
				}
			}
			catch (Exception exc)
			{
				System.Console.WriteLine("Exception Stack Trace -> " + exc.StackTrace);
			}
		}
		public void loginUser()
		{
			try
			{
				//Console.Write("YodleeAccount ID / Customer ID? (y/c)");
				//string type = Console.ReadLine();
				bool isCustomerId = false;
				//switch (type)
				//{
				//	case "y":
				//		isCustomerId = false;
				//		break;
				//	case "c":
				//		isCustomerId = true;
				//		break;
				//	default:
				//		throw new Exception(string.Format("Wrong type of id {0} available values are: y/c", type));
				//}

				Console.Write("Enter YodleeAccount Id: ");
				string id = Console.ReadLine();

				var log = new LegacyLog();
				var conn = new SqlConnection(log);

				SafeReader sr = conn.GetFirst("GetYodleeAccount", new QueryParameter("@IsCustomerId", isCustomerId),
				                                  new QueryParameter("@Id", id));

				string userName = sr[0];

				if (userName == "")
				{
					throw new Exception(string.Format("Account was not found {0}", id));
				}

				//	byte[] passwordBytes = (byte[])dt.Rows[0][1];
				string pS = sr[1];
				String password = Encryptor.Decrypt(pS);

				LoginUser loginUser = new LoginUser();


				userContext = loginUser.loginUser(userName, password);
				viewItems();
				System.Console.WriteLine("User Logged in Successfully..");
			}

			catch (SoapException se)
			{
				System.Console.WriteLine("User login failed -> " + se.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error Happened {0}", ex.Message);
			}
		}

		public void loginUser(string userName, string password)
		{
			Console.Write("username:");
			userName = Console.ReadLine();

			Console.Write("password:");
			password = Console.ReadLine();

			LoginUser loginUser = new LoginUser();
			try
			{
				userContext = loginUser.loginUser(userName, password);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error {0}", ex);
				return;
			}
			
			viewItems();
			loginUser.logoutUser(userContext);
		}
		

		public void sessionlessCallMenu()
		{
			SessionlessCall sessionlessCall = new SessionlessCall();
			sessionlessCall.doSessionlessCallMenu();
		}

		/**
		 * Unregisters the active user from the system.
		 */
		public void unregisterUser()
		{
			System.Console.WriteLine("Do you really wish to unregister user " + this.userName + "? [Y|N]");
			String input = IOUtils.readStr();

			if ("Y".Equals(input) || "y".Equals(input))
			{
				RegisterUser registerUser = new RegisterUser();
				registerUser.unregisterUser(userContext);
				userContext = null;
				System.Console.WriteLine("User " + this.userName + " has been unregistered.");
			}
			else
			{
				System.Console.WriteLine("User " + this.userName + " has NOT been removed and will remained logged in.");
			}
		}

		public void getAccessToken()
		{
			LoginUser login = new LoginUser();
			login.getAccessTokens(userContext);
		}

		public void itemManagementMenu()
		{
			System.Console.WriteLine("********************");
			System.Console.WriteLine(NAV_VIEW_ITEMS + ". View Items");
			System.Console.WriteLine(NAV_VIEW_ITEM + ". View Item");
			System.Console.WriteLine(NAV_DUMP_ITEM + ". Dump Item");
			System.Console.WriteLine(NAV_ADD_ITEM + ". Add Item (Includes MFA)");
			System.Console.WriteLine(NAV_EDIT_ITEM + ". Edit Item");
			System.Console.WriteLine(NAV_REMOVE_ITEM + ". Remove Item");
			System.Console.WriteLine(NAV_REFRESH_ITEM + ". Refresh Item");
			System.Console.WriteLine(NAV_REFRESH_ALL + ". Refresh All");
			System.Console.WriteLine(NAV_QUIT + ". Exit Sub-menu");
			System.Console.WriteLine("********************");
			System.Console.WriteLine("Choice: ");
			String inputStr = System.Console.ReadLine();
			long choice = long.Parse(inputStr);
			System.Console.WriteLine();

			if (choice == NAV_QUIT)
			{
				return;
			}
			if (choice == NAV_VIEW_ITEMS)
			{
				viewItems();
			}
			else if (choice == NAV_VIEW_ITEM)
			{
				viewItem();
			}
			else if (choice == NAV_ADD_ITEM)
			{
				addItem();
			}
			else if (choice == NAV_EDIT_ITEM)
			{
				editItem();
			}

			else if (choice == NAV_REMOVE_ITEM)
			{
				removeItem();
			}
			else if (choice == NAV_REFRESH_ITEM)
			{
				refreshItem();

			}
			else if (choice == NAV_REFRESH_ALL)
			{
				refreshAll();
			}
		}

		public void editItem()
		{
			EditItem editItem = new EditItem();
			long itemId = editItem.listAccounts(userContext);
			if (itemId > 0)
			{
				editItem.getItemInfo(userContext, itemId);
				editItem.updateItem(userContext, itemId);
			}
			System.Console.WriteLine("");

		}

		public void viewItems()
		{
			DisplayItemInfo displayItemInfo = new DisplayItemInfo();
			displayItemInfo.displayItemSummaries(userContext);
		}

		public void viewItem()
		{
			System.Console.Write("Enter ItemId: ");
			String input = System.Console.ReadLine();
			long itemId = long.Parse(input);
			viewItem(itemId);
		}

		public void viewItem(long itemId)
		{
			AccountSummary accountSummary = new AccountSummary();
			accountSummary.displayItemSummary(userContext, itemId);
		}



		public void addItem()
		{
			System.Console.WriteLine("Enter Content ServiceId: ");
			long csid = IOUtils.readInt();
			AddItem ad = new AddItem();
			long itemId = ad.doAddItem(userContext, csid);

			if (itemId != 0)
			{
				RefreshItem refreshItem = new RefreshItem();
				ContentServiceHelper csh = new ContentServiceHelper(userContext);
				String mfatype = csh.getMfATypeId(csid);
				// Start the Refresh
				if (mfatype != null)
				{
					refreshItem.refreshItem(userContext, itemId, true);
				}
				else
				{
					refreshItem.refreshItem(userContext, itemId, false);
				}

				// Poll for the refresh status and display the item
				// summary if refresh succeeds
				if (refreshItem.pollRefreshStatus(userContext, itemId))
				{
					System.Console.WriteLine("Done refreshing, display item:");
					viewItem(itemId);
				}
			}
		}

		public void removeItem()
		{
			DataServiceService dataService = new DataServiceService();
			dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
			object[] itemSummaries = dataService.getItemSummaries(userContext);
			if (itemSummaries != null && itemSummaries.Length >= 0)
			{
				for (int i = 0; i < itemSummaries.Length; i++)
				{
					ItemSummary itemSummary = (ItemSummary)itemSummaries[i];
					System.Console.WriteLine("itemId=" + itemSummary.itemId + " "
					+ itemSummary.itemDisplayName);
				}
			}
			else
			{
				System.Console.WriteLine("No Items to remove");
			}
			RemoveItem removeItem = new RemoveItem();
			System.Console.Write("\nEnter Item ID: ");
			String input = System.Console.ReadLine();
			long itemId = 0;
			if (long.TryParse(input, out itemId))
			{
				if (itemId > 0)
				{
					removeItem.removeItem(userContext, itemId);
				}
			}
		}

		public void refreshAll()
		{
			RefreshAll refreshAll = new RefreshAll();
			refreshAll.refreshAll(userContext);
			refreshAll.pollRefreshStatus(userContext);
		}

		public void refreshItem()
		{
			RefreshItem refreshItem = new RefreshItem();
			System.Console.WriteLine("Enter Item ID:");
			long itemId = IOUtils.readLong();

			DataServiceService dataService = new DataServiceService();
			dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
			ItemSummary itemSummary = dataService.getItemSummaryForItem(userContext, itemId, true);
			if (itemSummary != null)
			{
				bool mfaFlag = false;
				MfaType? mfaType = itemSummary.contentServiceInfo.mfaType;
				if (mfaType != null && mfaType.HasValue) //mfaType.typeId > 0
				{
					mfaFlag = true;
				}
				refreshItem.refreshItem(userContext, itemId, mfaFlag);
				// Poll for the refresh status and display the item
				// summary if refresh succeeds
				if (refreshItem.pollRefreshStatus(userContext, itemId))
				{
					System.Console.WriteLine("\tDone refreshing, display item:");
					viewItem(itemId);
				}
			}
			else
			{
				System.Console.WriteLine("The item does not exist");
			}

		}

		public void contentServiceMenu()
		{
			ContentServiceHelper csh = new ContentServiceHelper(userContext);
			csh.contentServiceMenu();
		}

		public static void Main(string[] args)
		{
			var yc = new YodleeConsoleMain();
			
			//var log = new LegacyLog();
			//var conn = new SqlConnection(log);

			//DataTable dt = conn.ExecuteReader("GetYodleeAccounts");
			//foreach (var row in dt.Rows)
			//{
			//	Console.WriteLine("Yodlee Account Id: {0}", ((DataRow)row)[0].ToString());
			//	string userName = ((DataRow)row)[1].ToString();
			//	//	byte[] passwordBytes = (byte[])dt.Rows[0][1];
			//	string pS = ((DataRow)row)[2].ToString();
			//	String password = Encryptor.Decrypt(pS);
			//	yc.loginUser(userName, password);
			//}
			yc.loop();
		}

	}

	//public class TrustAllCertificatePolicy : System.Net.ICertificatePolicy
	//{
	//    public TrustAllCertificatePolicy()
	//    {

	//    }

	//    public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest req, int problem)
	//    {
	//        return true;
	//        //string s = "123";

	//        //return false;
	//    }
	//}

}