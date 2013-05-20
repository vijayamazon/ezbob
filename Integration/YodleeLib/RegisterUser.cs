namespace YodleeLib
{
	using StructureMap;
	using config;

	/// <summary>
	/// Encapsulates user registration APIs of the Yodlee software platform.
	/// </summary>
	public class RegisterUser : ApplicationSuper
	{
		private UserRegistrationService registerService;
		private static IYodleeMarketPlaceConfig _config;

		public RegisterUser()
		{
			_config = ObjectFactory.GetInstance<IYodleeMarketPlaceConfig>();
			registerService = new UserRegistrationService();
            registerService.EnableDecompression = true;
			registerService.Url = _config.soapServer + "/" + registerService.GetType().FullName;
		}

		public UserContext DoRegisterUser(string loginName, string password, string email)
        {
			// These values are hard coded since we don't have them at the time of the registration
			string country = "US";
			string town = "Redwood City";
			string address = "3600 Bridge Parkway";
			string zipcode = "33444";

            // Create UserCredentials
            var pc = new PasswordCredentials(); 
            pc.loginName = loginName;
            pc.password = password;

            // Create UserProfile
            var up = new UserProfile();

            var upEntries = new Entry[4];

            var upEntry1 = new Entry();
            upEntry1.key = "EMAIL_ADDRESS";
            upEntry1.value = email;

            var upEntry2 = new Entry();
            upEntry2.key = "ADDRESS_1";
            upEntry2.value = address;

            var upEntry3 = new Entry();
            upEntry3.key = "CITY";
			upEntry3.value = town;

            var upEntry4 = new Entry();
            upEntry4.key = "COUNTRY";
			upEntry4.value = country;
            
            upEntries[0] = upEntry1;
            upEntries[1] = upEntry2;
            upEntries[2] = upEntry3;
            upEntries[3] = upEntry4;

            up.values = upEntries;
            
            var singlePref = new NVPair();
            singlePref.name = "com.yodlee.userprofile.LOCALE";
            singlePref.type = 1;
            var obj = new object[1];
            obj[0] = (object)"en-US";
            singlePref.values = obj;

            NVPair pincodePref = null;
            pincodePref = new NVPair();
            pincodePref.name = "com.yodlee.userprofile.ZIP_CODE_1";
            pincodePref.type = 1;
            var obj1 = new object[1];
			obj1[0] = zipcode;
            pincodePref.values = obj1;
			
            var nvPairs =  new NVPair[2];

            nvPairs[0] = singlePref;
            nvPairs[1] = pincodePref;

            // Register the user
			UserInfo1 ui = registerService.register3(GetCobrandContext(), pc, up, nvPairs);            
            return ui.userContext;
        }
        
		/// <summary>
		/// Obtains and changes the email of the registered user.
		/// </summary>
		/// <param name="userContext"></param>
		/// <param name="newEmailAddress"></param>
		//public void changeEmail(UserContext userContext,
		//						string newEmailAddress)
		//{
		//	FieldInfoSingle emailAddress = (FieldInfoSingle)registerService.getEmail(userContext);
		//	System.Console.WriteLine("\tExisting email address is: ", emailAddress.value);

		//	System.Console.WriteLine("\tChanging email address to: ", newEmailAddress);
		//	FieldInfoSingle newEmail = new FieldInfoSingle();
		//	newEmail.name            = "EMAIL";
		//	newEmail.value           = newEmailAddress;
		//	newEmail.displayName     = "EMAIL";
		//	newEmail.isOptional      = false;
			
		//	registerService.updateEmail(userContext, newEmail);
		//}

		///// <summary>
		///// Unregisters a user from the Yodlee platform.
		///// </summary>
		///// <param name="userContext"></param>
		//public void unregisterUser(UserContext userContext)
		//{
		//	try
		//	{
		//		registerService.unregister(userContext);
		//	}
		//	catch (SoapException soapEx)
		//	{
		//		System.Console.WriteLine(soapEx.StackTrace);
		//		throw new Exception("Error unregistering user: " + soapEx.Message);
              
		//	}
		//}
	}
}
