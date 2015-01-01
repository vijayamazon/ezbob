namespace com.yodlee.sampleapps {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Services.Protocols;
	using com.yodlee.sampleapps.datatypes;
	using com.yodlee.sampleapps.util;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ContentServiceHelper : ApplicationSuper {
		public ContentServiceHelper(UserContext userContext) {
			this.userContext = userContext;
			this.cst = new ContentServiceTraversalService();
			this.cst.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "ContentServiceTraversalService";
		}

		public void contentServiceMenu() {
			Boolean loop = true;

			while (loop) {
				System.Console.WriteLine("Choose one of the options");
				System.Console.WriteLine("********************");
				System.Console.WriteLine(NAV_SEARCH_CONTENT_SERVICES + ". Search content services");
				System.Console.WriteLine(NAV_VIEW_SINGLE_SERVICE_DETAILS + ". View Single content servce");
				System.Console.WriteLine(NAV_VIEW_ALL_CONTENT_SERVICES + ". View all content services (slow)");
				System.Console.WriteLine(NAV_VIEW_CONTAINER_SERVICES + ". View services for a specific container");
				System.Console.WriteLine(NAV_VIEW_UKBANK_SERVICES + ". View UK Bank services");
				System.Console.WriteLine(NAV_VIEW_SYSTEM_SERVICES + ". Generate Report of services");
				System.Console.WriteLine(NAV_QUIT + ". Exit Sub-menu");
				System.Console.WriteLine("********************");

				System.Console.Write("Enter Choice : ");
				String inputStr = System.Console.ReadLine();
				int choice = int.Parse(inputStr);
				System.Console.WriteLine();

				try {
					if (choice == NAV_SEARCH_CONTENT_SERVICES) {
						System.Console.Write("Enter Search Text : ");
						String searchString = System.Console.ReadLine();
						Search s = new Search();
						s.searchByKeywords(searchString);
					} else if (choice == NAV_VIEW_SINGLE_SERVICE_DETAILS)
						viewSingleServiceDetails();
					else if (choice == NAV_VIEW_ALL_CONTENT_SERVICES)
						viewAllContentServices();
					else if (choice == NAV_VIEW_CONTAINER_SERVICES)
						viewContainerServices();
					else if (choice == NAV_VIEW_UKBANK_SERVICES) {
						Search s = new Search();
						s.viewUkBank("UK");
					} else if (choice == NAV_VIEW_SYSTEM_SERVICES) {
						Search s = new Search();
						var yodlee = s.viewUkBank("UK");
						var system = getAllSystemServices();
						findMissingBanks(system, yodlee);
					} else if (choice == NAV_QUIT)
						loop = false;
				} catch (SoapException soapEx) {
					System.Console.WriteLine("Exception:" + soapEx.StackTrace);
					/*CoreException coreEx = ExceptionHandler.processException(soapEx);
					if (coreEx != null)
					{
						System.Console.WriteLine(coreEx.message);
						System.Console.WriteLine(coreEx.trace);
						System.Console.WriteLine(soapEx.StackTrace);
					}*/
				} catch (Exception e) {
					System.Console.WriteLine(e.StackTrace);
				}
			}
		}

		public void viewAllContentServices() {
			Entry[] serviceMap = this.cst.getContentServicesByContainerType(getCobrandContext());
			System.Console.WriteLine("Retrieved the following number of services:");

			for (int i = 0; i < serviceMap.Length; i++) {
				System.Console.Write("ContainerType : " + (serviceMap[i].key));
				System.Console.Write("  No of ContentServices : " + ((ContentServiceInfo[])(serviceMap[i].value)).Length);
				System.Console.Write("\n");
			}

			System.Console.Write("Do you want a dump of all the services? [y/n]:");
			String choice = IOUtils.readStr();

			if (choice.Equals("y", StringComparison.OrdinalIgnoreCase)) {
				System.Console.Write("\n");
				for (int i = 0; i < serviceMap.Length; i++) {
					System.Console.WriteLine("****************************************************");
					System.Console.WriteLine("ContainerType : " + (serviceMap[i].key));
					ContentServiceInfo[] csis = (ContentServiceInfo[])(serviceMap[i].value);
					printBriefContentServices(csis);
					System.Console.WriteLine("****************************************************");
				}
			}
		}

		public void viewSingleServiceDetails() {
			System.Console.WriteLine("Enter in the content service ID to view details >> : ");
			long choice = IOUtils.readLong();

			// passing a "1" as the third arg gets keywords
			ContentServiceInfo csi = this.cst.getContentServiceInfo(getCobrandContext(), choice, true);
			printContentServiceInfo(csi);
		}

		public Dictionary<long, string> getAllSystemServices() {
			var log = new LegacyLog();
			var conn = new SqlConnection(log);

			var banks = new Dictionary<long, string>();

			conn.ForEachRowSafe((row, bRowsetStart) => {
				long csid = row[0];
				string name = row[1];

				banks.Add(csid, name);

				try {
					ContentServiceInfo csi = this.cst.getContentServiceInfo(getCobrandContext(), csid, true);
					if (csi.contentServiceDisplayName.Split(' ')
						.First()
						.ToLowerInvariant() != name.Split(' ')
							.First()
							.ToLowerInvariant())
						Console.WriteLine("{2} Name difference system:{1} yodlee:{0}", csi.contentServiceDisplayName, name, csid);
					if (csi.containerInfo.containerName != "bank")
						Console.WriteLine("{0} not a bank {1}", csid, csi.containerInfo.containerName);
				} catch (Exception ex) {
					Console.WriteLine("Error in retrieving data for {0} {1}: {2}", csid, name, ex.Message);
				}

				return ActionResult.Continue;
			}, "GetYodleeBanks");

			if (banks.Count > 0)
				Console.WriteLine("Found {0} banks", banks.Count);
			else
				throw new Exception(string.Format("System Banks not found"));

			return banks;
		}

		public void findMissingBanks(Dictionary<long, string> system, Dictionary<long, string> yodlee) {
			foreach (var key in yodlee.Keys) {
				if (!system.ContainsKey(key))
					Console.WriteLine("Missing system bank {0} {1}", key, yodlee[key]);
			}
		}

		public void printContentServiceInfo(ContentServiceInfo csi) {
			System.Console.WriteLine("Content Service id: " + csi.contentServiceId);
			System.Console.WriteLine("Display name: " + csi.contentServiceDisplayName);
			System.Console.WriteLine("Container type: " + csi.containerInfo.containerName);
			System.Console.WriteLine("Registration URL: " + csi.registrationUrl);
			System.Console.WriteLine("Home URL: " + csi.homeUrl);
			System.Console.WriteLine("Login URL: " + csi.loginUrl);

			String autoLoginType = "UNKNOWN";
			switch (csi.autoLoginType) {
			case 3: //AutoLoginManagementService.CLIENT_ENABLED: 
				autoLoginType = "CLIENT_ENABLED";
				break;
			case 4: //AutoLoginManagementService.HTTP: 
				autoLoginType = "HTTP";
				break;
			case 5: //AutoLoginManagementService.NOT_SUPPORTED: 
				autoLoginType = "NOT_SUPPORTED ";
				break;
			case 2: //AutoLoginManagementService.PROXY: 
				autoLoginType = "PROXY";
				break;
			case 1: //AutoLoginManagementService.SIMPLE: 
				autoLoginType = "SIMPLE";
				break;
			}

			System.Console.Write("AutoLogin Type: " + autoLoginType);

			String[] keywords = csi.keywords;
			if (keywords != null) {
				System.Console.Write("Search keywords: ");
				for (int i = 0; i < keywords.Length; i++) {
					String keyword = keywords[i];
					System.Console.Write(keyword + ",");
				}
			}
			System.Console.WriteLine("");

			if (csi.hasSiblingContentServices) {
				ContentServiceInfo[] siblingCSIDs = this.cst.getSiblingContentServices(getCobrandContext(), csi.contentServiceId, true);

				System.Console.WriteLine(siblingCSIDs.Length + " sibling sites available");
				for (int i = 0; i < siblingCSIDs.Length; i++) {
					System.Console.WriteLine("  Sibling: "
						+ siblingCSIDs[i].contentServiceDisplayName
						+ " ("
						+ siblingCSIDs[i].contentServiceId
						+ ")");
				}
			} else
				System.Console.WriteLine("No sibling sites available");

			ContentServiceInfo[] sharedCSIDs = this.cst.getContentServicesBySite(getCobrandContext(), csi.siteId, true);
			if (sharedCSIDs.Length > 1) {
				System.Console.WriteLine((sharedCSIDs.Length - 1) + " shared sites available");
				for (int i = 0; i < sharedCSIDs.Length; i++) {
					if (csi.contentServiceId != sharedCSIDs[i].contentServiceId) {
						System.Console.WriteLine("  Shared: "
							+ sharedCSIDs[i].contentServiceDisplayName
							+ " ("
							+ sharedCSIDs[i].contentServiceId
							+ ")");
					}
				}
			} else
				System.Console.WriteLine("No shared sites available");

			System.Console.WriteLine("\n\n");
		}

		public void viewContainerServices() {
			System.Console.WriteLine("The following are the containers to get the sites for");
			System.Console.WriteLine(
				ContainerTypes.AIR_RESERVATION + ", " +
					ContainerTypes.AUCTION + ", " +
					ContainerTypes.BANK + ", " +
					ContainerTypes.BILL + ", " +
					ContainerTypes.BILL_PAY_SERVICE + ", " +
					ContainerTypes.CABLE_SATELLITE + ", " +
					ContainerTypes.CALENDAR + ", " +
					ContainerTypes.CAR_RESERVATION + ", " +
					ContainerTypes.CHARTS + ", " +
					ContainerTypes.CHAT + ", " +
					ContainerTypes.CONSUMER_GUIDE + ", " +
					ContainerTypes.CREDIT_CARD + ", " +
					ContainerTypes.DEAL + ", " +
					ContainerTypes.HOTEL_RESERVATION + ", " +
					ContainerTypes.INSURANCE + ", " +
					ContainerTypes.INVESTMENT + ", " +
					ContainerTypes.JOB + ", " +
					ContainerTypes.LOAN + ", " +
					ContainerTypes.MAIL + ", " +
					ContainerTypes.MESSAGE_BOARD + ", " +
					ContainerTypes.MINUTES + ", " +
					ContainerTypes.MISCELLANEOUS + ", " +
					ContainerTypes.MORTGAGE + ", " +
					ContainerTypes.NEWS + ", " +
					ContainerTypes.ORDER + ", " +
					ContainerTypes.OTHER_ASSETS + ", " +
					ContainerTypes.OTHER_LIABILITIES + ", " +
					ContainerTypes.RESERVATION + ", " +
					ContainerTypes.REWARD_PROGRAM + ", " +
					ContainerTypes.TELEPHONE + ", " +
					ContainerTypes.UTILITIES
				);

			System.Console.Write("Type the name of the container you want the services for >> ");
			String choice = IOUtils.readStr();

			if (isValid(choice)) {
				ContentServiceInfo[] csis = this.cst.getContentServicesByContainerType2(getCobrandContext(), choice);
				if (csis != null)
					printBriefContentServices(csis);
				else
					System.Console.WriteLine("No Content Services avialable for " + choice + " container");
			} else
				System.Console.WriteLine("Invalid content service entered: " + choice);
		}

		public void printBriefContentServices(ContentServiceInfo[] csis) {
			for (int i = 0; i < csis.Length; i++) {
				ContentServiceInfo csi = csis[i];
				System.Console.WriteLine(
					csi.contentServiceId + "#" +
						csi.containerInfo.containerName + "#" +
						csi.contentServiceDisplayName + "#" +
						csi.homeUrl + "#" +
						(csi.loginForm == null ? "null" : "value")
					);
			}
		}

		public String getMfATypeId(long content_service_id) {
			ContentServiceInfo csi = this.cst.getContentServiceInfo(getCobrandContext(), content_service_id, true);
			return (csi.mfaType == null) ? null : csi.mfaType.GetType()
				.ToString();
		}

		protected ContentServiceTraversalService cst;
		protected UserContext userContext;

		private bool isValid(String choice) {
			string[] containerTypes = ConverToArray();
			foreach (string containerType in containerTypes) {
				if (choice.Equals(containerType))
					return true;
			}
			return false;
		}

		private string[] ConverToArray() {
			string[] containerArray = new string[] {
				ContainerTypes.AIR_RESERVATION,
				ContainerTypes.AUCTION,
				ContainerTypes.BANK,
				ContainerTypes.BILL,
				ContainerTypes.BILL_PAY_SERVICE,
				ContainerTypes.CABLE_SATELLITE,
				ContainerTypes.CALENDAR,
				ContainerTypes.CAR_RESERVATION,
				ContainerTypes.CHARTS,
				ContainerTypes.CHAT,
				ContainerTypes.CONSUMER_GUIDE,
				ContainerTypes.CREDIT_CARD,
				ContainerTypes.DEAL,
				ContainerTypes.HOTEL_RESERVATION,
				ContainerTypes.INSURANCE,
				ContainerTypes.INVESTMENT,
				ContainerTypes.JOB,
				ContainerTypes.LOAN,
				ContainerTypes.MAIL,
				ContainerTypes.MESSAGE_BOARD,
				ContainerTypes.MINUTES,
				ContainerTypes.MISCELLANEOUS,
				ContainerTypes.MORTGAGE,
				ContainerTypes.NEWS,
				ContainerTypes.ORDER,
				ContainerTypes.OTHER_ASSETS,
				ContainerTypes.OTHER_LIABILITIES,
				ContainerTypes.RESERVATION,
				ContainerTypes.REWARD_PROGRAM,
				ContainerTypes.TELEPHONE,
				ContainerTypes.UTILITIES
			};

			return containerArray;
		}

		private static readonly int NAV_SEARCH_CONTENT_SERVICES = OPTION_CNT++;
		private static readonly int NAV_VIEW_SINGLE_SERVICE_DETAILS = OPTION_CNT++;
		private static readonly int NAV_VIEW_ALL_CONTENT_SERVICES = OPTION_CNT++;
		private static readonly int NAV_VIEW_CONTAINER_SERVICES = OPTION_CNT++;
		private static readonly int NAV_VIEW_UKBANK_SERVICES = OPTION_CNT++;
		private static readonly int NAV_VIEW_SYSTEM_SERVICES = OPTION_CNT++;
		private static readonly int NAV_QUIT = OPTION_CNT++;
		private static int OPTION_CNT = 1;
		/**
		 * This method will give a summary of how many services there are per container and
		 * give the user the option to print them all out
		 */
		/**
		 * View the details of a single content service
		 */
		/**
		 * This method prints a subset of the ContentServiceInfo fields
		 * @param csi Content Service info object to show info for
		 */
		/**
		 * This method is to get the MFAtype of the particular content service id. 
		 * @param content_service_id
		 * @return
		 */
	}
}
