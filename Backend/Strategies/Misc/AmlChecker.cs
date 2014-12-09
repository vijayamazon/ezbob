namespace Ezbob.Backend.Strategies.Misc 
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using ExperianLib.IdIdentityHub;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StoredProcs;

	public class AmlChecker : AStrategy 
	{
		public AmlChecker(int customerId) {
			this.customerId = customerId;
			GetPersonalInfo();
			customerAddress = new GetCustomerAddresses(this.customerId, null, DB, Log).FillFirst<GetCustomerAddresses.ResultRow>();
		}

		public AmlChecker(
			int customerId,
			string idhubHouseNumber,
			string idhubHouseName,
			string idhubStreet,
			string idhubDistrict,
			string idhubTown,
			string idhubCounty,
			string idhubPostCode
		) {
			isCustom = true;
			this.customerId = customerId;
			GetPersonalInfo();

			this.idhubHouseNumber = idhubHouseNumber;
			this.idhubHouseName = idhubHouseName;
			this.idhubStreet = idhubStreet;
			this.idhubDistrict = idhubDistrict;
			this.idhubTown = idhubTown;
			this.idhubCounty = idhubCounty;
			this.idhubPostCode = idhubPostCode;
		}

		public override string Name { get { return "AML check"; } }
		
		public override void Execute() 
		{
			string result, description;
			decimal authentication;

			bool hasError = isCustom
				? GetAmlDataCustom(out result, out authentication, out description)
				: GetAmlData(out result, out authentication, out description);

			if (hasError || authentication < CurrentValues.Instance.MinAuthenticationIndexToPassAml)
			{
				result = "Warning";
			}

			DB.ExecuteNonQuery("UpdateAmlResult", CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("AmlResult", result),
				new QueryParameter("AmlScore", (int)authentication),
				new QueryParameter("AmlDescription", description)
			);
		} 

		private bool GetAmlData(out string result, out decimal authentication, out string description) 
		{
			Log.Info("Starting standard AML check with parameters: FirstName={0} Surname={1} Gender={2} DateOfBirth={3} {4}",
				firstName, lastName, gender, dateOfBirth, customerAddress
			);

			bool hasError = GetAml(AddressCurrency.Current, out result, out authentication, out description);

			if (hasError && (timeAtAddress == 1) && !string.IsNullOrWhiteSpace(customerAddress[6, AddressCurrency.Previous]))
			{
				hasError = GetAml(AddressCurrency.Previous, out result, out authentication, out description);
			}

			return hasError;
		}

		private bool GetAmlDataCustom(out string result, out decimal authentication, out string description) 
		{
			Log.Info(
				"Starting custom AML check with parameters: FirstName={0} Surname={1} Gender={2} DateOfBirth={3} idhubHouseNumber={4} idhubHouseName={5} idhubStreet={6} idhubDistrict={7} idhubTown={8} idhubCounty={9} idhubPostCode={10}",
				firstName,
				lastName,
				gender,
				dateOfBirth,
				idhubHouseNumber,
				idhubHouseName,
				idhubStreet,
				idhubDistrict,
				idhubTown,
				idhubCounty,
				idhubPostCode
			);

			AuthenticationResults authenticationResults = idHubService.AuthenticateForcedWithCustomAddress(
				firstName,
				null,
				lastName,
				gender,
				dateOfBirth,
				idhubHouseNumber,
				idhubHouseName,
				idhubStreet,
				idhubDistrict,
				idhubTown,
				idhubCounty,
				idhubPostCode,
				customerId
			);

			return CreateAmlResultFromAuthenticationReuslts(authenticationResults, out result, out authentication, out description);
		}

		private bool GetAml(AddressCurrency nCurrency, out string result, out decimal authentication, out string description)
		{
			AuthenticationResults authenticationResults = idHubService.Authenticate(
				firstName,
				null,
				lastName,
				gender,
				dateOfBirth,
				customerAddress[1, nCurrency],
				customerAddress[2, nCurrency],
				customerAddress[3, nCurrency],
				customerAddress[4, nCurrency],
				null,
				customerAddress[6, nCurrency],
				customerId
			);

			return CreateAmlResultFromAuthenticationReuslts(authenticationResults, out result, out authentication, out description);
		}

		private bool CreateAmlResultFromAuthenticationReuslts(AuthenticationResults results, out string result, out decimal authentication, out string description) 
		{
			description = results.AuthIndexText;
			authentication = results.AuthenticationIndex;

			if (results.HasError) 
			{
				Log.Info("Error getting AML data: {0}", results.Error);
				result = string.Empty;
				return true;
			}

			result = "Passed";

			if (results.ReturnedHRP.Any(returnedHrp => warningRules.Contains(returnedHrp.HighRiskPolRuleID)))
			{
				result = "Warning";
			}

			return false;
		}

		private void GetPersonalInfo() 
		{
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => 
				{
					firstName = sr["FirstName"];
					lastName = sr["Surname"];
					gender = sr["Gender"];
					dateOfBirth = sr["DateOfBirth"];
					timeAtAddress = sr["TimeAtAddress"];
					return ActionResult.SkipAll;
				},
				"GetPersonalInfo",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);
		}

		private readonly IdHubService idHubService = new IdHubService();
		private int timeAtAddress;

		private readonly GetCustomerAddresses.ResultRow customerAddress;

		private string firstName;
		private string lastName;
		private string gender;
		private DateTime dateOfBirth;
		private readonly bool isCustom;
		private readonly int customerId;

		private readonly string idhubHouseNumber;
		private readonly string idhubHouseName;
		private readonly string idhubStreet;
		private readonly string idhubDistrict;
		private readonly string idhubTown;
		private readonly string idhubCounty;
		private readonly string idhubPostCode;

		private readonly SortedSet<string> warningRules = new SortedSet<string> 
		{
			"U001", "U004", "U007", "U013", "U015", "U131", "U133", "U135", "U018", "U0132", "U0134",
		};
	}
}
