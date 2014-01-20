namespace EzBob.Backend.Strategies
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using ExperianLib.IdIdentityHub;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class AmlChecker : AStrategy
	{
		private readonly IdHubService idHubService = new IdHubService();
		private int timeAtAddress;
		private string line1Current;
		private string line2Current;
		private string line3Current;
		private string line4Current;
		private string line6Current;
		private string line1Prev;
		private string line2Prev;
		private string line3Prev;
		private string line4Prev;
		private string line6Prev;
		private string firstName;
		private string surname;
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

		public AmlChecker(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
			GetPersonalInfo();
			GetAddresses();
		}

		public AmlChecker(int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet, string idhubDistrict, string idhubTown, 
			string idhubCounty, string idhubPostCode, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
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
		
		public override string Name
		{
			get { return "AML check"; }
		} // Name

		public override void Execute()
		{
			string result;
			decimal authentication;
			bool hasError = isCustom ? GetAmlDataCustom(out result, out authentication) : GetAmlData(out result, out authentication);

			if (hasError || authentication < 40)
			{
				result = "Warning";
			}

			DB.ExecuteNonQuery("UpdateAmlResult", CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("AmlResult", result));
		}

		private bool GetAmlData(out string result, out decimal authentication)
		{
			Log.Info("Starting aml check with params: FirstName={0} Surname={1} Gender={2} DateOfBirth={3} Line1={4} Line2={5} Line3={6} Line4={7} Line6={8} PrevLine1={9} PrevLine2={10} PrevLine3={11} PrevLine4={12} PrevLine6={13}",
				firstName, surname, gender, dateOfBirth, line1Current, line2Current, line3Current, line4Current, line6Current,
				line1Prev, line2Prev, line3Prev, line4Prev, line6Prev);
			bool hasError = GetAml(line1Current, line2Current, line3Current, line4Current, line6Current, out result, out authentication);

			if (hasError && timeAtAddress == 1 && line6Prev != null)
			{
				hasError = GetAml(line1Prev, line2Prev, line3Prev, line4Prev, line6Prev, out result, out authentication);
			}

			return hasError;
		}

		private bool GetAmlDataCustom(out string result, out decimal authentication)
		{
			Log.Info("Starting custom aml check with params: FirstName={0} Surname={1} Gender={2} DateOfBirth={3} idhubHouseNumber={4} idhubHouseName={5} idhubStreet={6} idhubDistrict={7} idhubTown={8} idhubCounty={9} idhubPostCode={10}",
				firstName, surname, gender, dateOfBirth, idhubHouseNumber, idhubHouseName, idhubStreet,
				idhubDistrict, idhubTown, idhubCounty, idhubPostCode);
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
				Log.Info("Error getting aml data. error:{0}", results.Error);
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

		private void GetAddresses()
		{
			DataTable dt = DB.ExecuteReader("GetCustomerAddresses", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			var addressesResults = new SafeReader(dt.Rows[0]);
			line1Current = addressesResults["Line1"];
			line2Current = addressesResults["Line2"];
			line3Current = addressesResults["Line3"];
			line4Current = addressesResults["Line4"];
			line6Current = addressesResults["Line6"];
			line1Prev = addressesResults["Line1Prev"];
			line2Prev = addressesResults["Line2Prev"];
			line3Prev = addressesResults["Line3Prev"];
			line4Prev = addressesResults["Line4Prev"];
			line6Prev = addressesResults["Line6Prev"];
		}

		private void GetPersonalInfo()
		{
			DataTable dt = DB.ExecuteReader("GetPersonalInfo", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			var results = new SafeReader(dt.Rows[0]);

			firstName = results["FirstName"];
			surname = results["Surname"];
			gender = results["Gender"];
			dateOfBirth = results["DateOfBirth"];
			timeAtAddress = results["TimeAtAddress"];
		}
	}
}
