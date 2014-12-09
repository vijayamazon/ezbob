namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Encapsulates user registration APIs of the Yodlee software platform.
	/// </summary>
	public class UpdateUserInfo : ApplicationSuper
	{
		UserProfileManagementService userProfileManagementService = null;

		public UpdateUserInfo()
		{
			userProfileManagementService = new UserProfileManagementService();
			userProfileManagementService.EnableDecompression = true;
			userProfileManagementService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + userProfileManagementService.GetType().FullName;
		}

		public void DoUpdateUserInfo(UserContext userContext, string email, Address address)
		{
			// Create UserCredentials

			// Create UserProfile
			UserProfile up = new UserProfile();

			Entry[] upEntries = new Entry[4];

			Entry upEntry1 = new Entry();
			upEntry1.key = "EMAIL_ADDRESS";
			upEntry1.value = email;

			Entry upEntry2 = new Entry();
			upEntry2.key = "ADDRESS_1";
			upEntry2.value = address.address1;

			Entry upEntry3 = new Entry();
			upEntry3.key = "CITY";
			upEntry3.value = address.city;

			Entry upEntry4 = new Entry();
			upEntry4.key = "COUNTRY";
			upEntry4.value = address.country;

			upEntries[0] = upEntry1;
			upEntries[1] = upEntry2;
			upEntries[2] = upEntry3;
			upEntries[3] = upEntry4;

			up.values = upEntries;

			userProfileManagementService.updateUserProfile(userContext, up);
		}
	}
}
