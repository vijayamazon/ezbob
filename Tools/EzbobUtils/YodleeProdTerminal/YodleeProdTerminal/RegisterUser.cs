using System;
using System.Web.Services.Protocols;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Encapsulates user registration APIs of the Yodlee software platform.
	/// </summary>
	public class RegisterUser : ApplicationSuper
	{
		UserRegistrationService registerService = null;

		public RegisterUser()
		{
			registerService = new UserRegistrationService();
            registerService.EnableDecompression = true;
            registerService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + registerService.GetType().FullName;
		}

		public void UnregisterUser(UserContext user)
		{
			registerService.unregister(user);
		}
        public UserContext registerUser(String loginName,
                                     String password,
                                     String email)
        {
            // Create UserCredentials
            PasswordCredentials pc = new PasswordCredentials(); 
            pc.loginName = loginName;
            pc.password = password;

            // Create UserProfile
            UserProfile up = new UserProfile();

            Entry[] upEntries = new Entry[4];

            Entry upEntry1 = new Entry();
            upEntry1.key = "EMAIL_ADDRESS";
            upEntry1.value = email;

            Entry upEntry2 = new Entry();
            upEntry2.key = "ADDRESS_1";
            upEntry2.value = "3600 Bridge Parkway";

            Entry upEntry3 = new Entry();
            upEntry3.key = "CITY";
            upEntry3.value = "Redwood City";

            Entry upEntry4 = new Entry();
            upEntry4.key = "COUNTRY";
            upEntry4.value = "US";

            upEntries[0] = upEntry1;
            upEntries[1] = upEntry2;
            upEntries[2] = upEntry3;
            upEntries[3] = upEntry4;

            up.values = upEntries;

            NVPair singlePref = null;
            singlePref = new NVPair();
            singlePref.name = "com.yodlee.userprofile.LOCALE";
            singlePref.type = 1;
            object[] obj = new object[1];
            obj[0] = (object)"en-US";
            singlePref.values = obj;

            NVPair pincodePref = null;
            pincodePref = new NVPair();
            pincodePref.name = "com.yodlee.userprofile.ZIP_CODE_1";
            pincodePref.type = 1;
            object[] obj1 = new object[1];
            obj1[0] = (object)"33444";
            pincodePref.values = obj1;

            NVPair[] nvPairs =  new NVPair[2];

            nvPairs[0] = singlePref;
            nvPairs[1] = pincodePref;

            //System.Console.WriteLine("singlePref  " + nvPairs[0].type);
            //System.Console.WriteLine("singlePref  " + nvPairs[1].type);

            // Register the user
            UserInfo1 ui = registerService.register3(getCobrandContext(), pc, up, nvPairs);            
            return ui.userContext;
        }

		/// <summary>
		/// Obtains and changes the email of the registered user.
		/// </summary>
		/// <param name="userContext"></param>
		/// <param name="newEmailAddress"></param>
		public void changeEmail(UserContext userContext,
								String newEmailAddress)
		{
			FieldInfoSingle emailAddress = (FieldInfoSingle)registerService.getEmail(userContext);
			System.Console.WriteLine("\tExisting email address is: ", emailAddress.value);

			System.Console.WriteLine("\tChanging email address to: ", newEmailAddress);
			FieldInfoSingle newEmail = new FieldInfoSingle();
			newEmail.name            = "EMAIL";
			newEmail.value           = newEmailAddress;
			newEmail.displayName     = "EMAIL";
			newEmail.isOptional      = false;

			registerService.updateEmail(userContext, newEmail);
		}

		/// <summary>
		/// Unregisters a user from the Yodlee platform.
		/// </summary>
		/// <param name="userContext"></param>
		public void unregisterUser(UserContext userContext)
		{
            try
            {
                registerService.unregister(userContext);
            }
            catch (SoapException soapEx)
            {
                System.Console.WriteLine(soapEx.StackTrace);
                throw new Exception("Error unregistering user: " + soapEx.Message);

            }
		}
	}
}
