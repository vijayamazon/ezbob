namespace EzBob.Backend.Strategies
{
	using System;
	using System.Collections.Generic;
	using ExperianLib.IdIdentityHub;

	public class AmlChecker
	{
		private readonly IdHubService idHubService = new IdHubService();
		private readonly int timeAtAddress;
		private readonly string line1Current;
		private readonly string line2Current;
		private readonly string line3Current;
		private readonly string line4Current;
		private readonly string line6Current;
		private readonly string line1Prev;
		private readonly string line2Prev;
		private readonly string line3Prev;
		private readonly string line4Prev;
		private readonly string line6Prev;
		private readonly string firstName;
		private readonly string surname;
		private readonly string gender;
		private readonly DateTime dateOfBirth;
		private readonly bool isCustom;
		private readonly int customerId;
		private readonly string idhubHouseNumber;
		private readonly string idhubHouseName;
		private readonly string idhubStreet;
		private readonly string idhubDistrict;
		private readonly string idhubTown;
		private readonly string idhubCounty;
		private readonly string idhubPostCode;

		private readonly Dictionary<string, bool> warningRules = new Dictionary<string, bool>
			{
				{"U001", false},
				{"U004", false},
				{"U007", false},
				{"U013", false},
				{"U015", false},
				{"U131", false},
				{"U133", false},
				{"U135", false},
				{"U018", false},
				{"U0132", false},
				{"U0134", false}
			};

		public AmlChecker(int customerId, string firstName, string surname, string gender, DateTime dateOfBirth,
			string line1Current, string line2Current, string line3Current, string line4Current, string line6Current,
			string line1Prev, string line2Prev, string line3Prev, string line4Prev, string line6Prev, int timeAtAddress)
		{
			this.customerId = customerId;
			this.timeAtAddress = timeAtAddress;
			this.line1Current = line1Current;
			this.line2Current = line2Current;
			this.line3Current = line3Current;
			this.line4Current = line4Current;
			this.line6Current = line6Current;
			this.line1Prev = line1Prev;
			this.line2Prev = line2Prev;
			this.line3Prev = line3Prev;
			this.line4Prev = line4Prev;
			this.line6Prev = line6Prev;
			this.firstName = firstName;
			this.surname = surname;
			this.gender = gender;
			this.dateOfBirth = dateOfBirth;
		}

		public AmlChecker(int customerId, string firstName, string surname, string gender, DateTime dateOfBirth,
			string idhubHouseNumber, string idhubHouseName, string idhubStreet,
			string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode)
		{
			isCustom = true;
			this.customerId = customerId;
			this.firstName = firstName;
			this.surname = surname;
			this.gender = gender;
			this.dateOfBirth = dateOfBirth;
			this.idhubHouseNumber = idhubHouseNumber;
			this.idhubHouseName = idhubHouseName;
			this.idhubStreet = idhubStreet;
			this.idhubDistrict = idhubDistrict;
			this.idhubTown = idhubTown;
			this.idhubCounty = idhubCounty;
			this.idhubPostCode = idhubPostCode;
		}

		public string Check()
		{
			string result;
			decimal authentication;
			bool hasError = isCustom ? GetAmlDataCustom(out result, out authentication) : GetAmlData(out result, out authentication);
			
			if (hasError || authentication < 40)
				return "Warning";

			return result;
		}

		private bool GetAmlData(out string result, out decimal authentication)
		{
			bool hasError = GetAml(line1Current, line2Current, line3Current, line4Current, line6Current, out result, out authentication);

			if (hasError && timeAtAddress == 1 && line6Prev != null)
			{
				hasError = GetAml(line1Prev, line2Prev, line3Prev, line4Prev, line6Prev, out result, out authentication);
			}

			return hasError;
		}

		private bool GetAmlDataCustom(out string result, out decimal authentication)
		{
			AuthenticationResults authenticationResults = idHubService.AuthenticateForcedWithCustomAddress(
				firstName, null, surname, gender, dateOfBirth, idhubHouseNumber, idhubHouseName, idhubStreet,
				idhubDistrict, idhubTown, idhubCounty, idhubPostCode, customerId);
			return CreateAmlResultFromAuthenticationReuslts(authenticationResults, out result, out authentication);
		}

		private bool GetAml(string line1, string line2, string line3, string line4, string line6, out string result, out decimal authentication)
		{
			AuthenticationResults authenticationResults = idHubService.Authenticate(
				firstName, null, surname, gender, dateOfBirth,
				line1, line2, line3, line4, null, line6, customerId);

			return CreateAmlResultFromAuthenticationReuslts(authenticationResults, out result, out authentication);
		}

		private bool CreateAmlResultFromAuthenticationReuslts(AuthenticationResults results, out string result, out decimal authentication)
		{
			if (results.HasError)
			{
				//Log.Info("Error getting aml data. error:{0}", results.Error);
				result = string.Empty;
				authentication = 0;
				return true;
			}

			authentication = results.AuthenticationIndexType;
			result = "Passed";

			foreach (var returnedHrp in results.ReturnedHRP)
			{
				if (warningRules.ContainsKey(returnedHrp.HighRiskPolRuleID))
				{
					result = "Warning";
				}
			} // foreach

			return false;
		} // CreateAmlResultFromAuthenticationReuslts
	}
}
