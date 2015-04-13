namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using ExperianLib.IdIdentityHub;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation;
	using Ezbob.Backend.Strategies.StoredProcs;
	using Ezbob.Database;

	public class AmlChecker : AStrategy {
		public AmlChecker(int customerId) {
			this.isCustom = false;
			this.customerId = customerId;
			GetPersonalInfo();

			customerAddress = new GetCustomerAddresses(this.customerId, null, DB, Log)
				.FillFirst<GetCustomerAddresses.ResultRow>();
		} // constructor

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
			this.isCustom = true;
			this.customerId = customerId;
			GetPersonalInfo();

			this.idhubHouseNumber = idhubHouseNumber;
			this.idhubHouseName = idhubHouseName;
			this.idhubStreet = idhubStreet;
			this.idhubDistrict = idhubDistrict;
			this.idhubTown = idhubTown;
			this.idhubCounty = idhubCounty;
			this.idhubPostCode = idhubPostCode;
		} // constructor

		public override string Name { get { return "AML check"; } }

		public override void Execute() {
			string result, description;
			decimal authentication;

			bool hasError = this.isCustom
				? GetAmlDataCustom(out result, out authentication, out description)
				: GetAmlData(out result, out authentication, out description);

			if (hasError || authentication < CurrentValues.Instance.MinAuthenticationIndexToPassAml)
				result = "Warning";

			DB.ExecuteNonQuery("UpdateAmlResult", CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId),
				new QueryParameter("AmlResult", result),
				new QueryParameter("AmlScore", (int)authentication),
				new QueryParameter("AmlDescription", description)
			);

			new SilentAutomation(this.customerId).SetTag(SilentAutomation.Callers.Aml).Execute();
		} // Execute

		private bool GetAmlData(out string result, out decimal authentication, out string description) {
			Log.Info(
				"Starting standard AML check with parameters: FirstName={0} Surname={1} Gender={2} DateOfBirth={3} {4}",
				this.firstName,
				this.lastName,
				this.gender,
				this.dateOfBirth,
				this.customerAddress
			);

			bool hasError = GetAml(AddressCurrency.Current, out result, out authentication, out description);

			bool stillHasError = hasError &&
				(this.timeAtAddress == 1) &&
				!string.IsNullOrWhiteSpace(this.customerAddress[6, AddressCurrency.Previous]);

			if (stillHasError)
				hasError = GetAml(AddressCurrency.Previous, out result, out authentication, out description);

			return hasError;
		} // GetAmlData

		private bool GetAmlDataCustom(out string result, out decimal authentication, out string description) {
			Log.Info(
				"Starting custom AML check with parameters: " +
				"FirstName={0} Surname={1} Gender={2} DateOfBirth={3} " +
				"idhubHouseNumber={4} idhubHouseName={5} idhubStreet={6} " +
				"idhubDistrict={7} idhubTown={8} idhubCounty={9} idhubPostCode={10}",
				this.firstName,
				this.lastName,
				this.gender,
				this.dateOfBirth,
				this.idhubHouseNumber,
				this.idhubHouseName,
				this.idhubStreet,
				this.idhubDistrict,
				this.idhubTown,
				this.idhubCounty,
				this.idhubPostCode
			);

			AuthenticationResults authenticationResults = this.idHubService.AuthenticateForcedWithCustomAddress(
				this.firstName,
				null,
				this.lastName,
				this.gender,
				this.dateOfBirth,
				this.idhubHouseNumber,
				this.idhubHouseName,
				this.idhubStreet,
				this.idhubDistrict,
				this.idhubTown,
				this.idhubCounty,
				this.idhubPostCode,
				this.customerId
			);

			return CreateAmlResultFromAuthenticationReuslts(
				authenticationResults,
				out result,
				out authentication,
				out description
			);
		} // GetAmlDataCustom

		private bool GetAml(
			AddressCurrency nCurrency,
			out string result,
			out decimal authentication,
			out string description
		) {
			AuthenticationResults authenticationResults = this.idHubService.Authenticate(
				this.firstName,
				null,
				this.lastName,
				this.gender,
				this.dateOfBirth,
				this.customerAddress[1, nCurrency],
				this.customerAddress[2, nCurrency],
				this.customerAddress[3, nCurrency],
				this.customerAddress[4, nCurrency],
				null,
				this.customerAddress[6, nCurrency],
				this.customerId
			);

			return CreateAmlResultFromAuthenticationReuslts(
				authenticationResults,
				out result,
				out authentication,
				out description
			);
		} // GetAml

		private bool CreateAmlResultFromAuthenticationReuslts(
			AuthenticationResults results,
			out string result,
			out decimal authentication,
			out string description
		) {
			description = results.AuthIndexText;
			authentication = results.AuthenticationIndex;

			if (results.HasError) {
				Log.Info("Error getting AML data: {0}", results.Error);
				result = string.Empty;
				return true;
			} // if

			result = "Passed";

			if (results.ReturnedHRP.Any(returnedHrp => this.warningRules.Contains(returnedHrp.HighRiskPolRuleID)))
				result = "Warning";

			return false;
		} // CreateAmlResultFromAuthenticationReuslts

		private void GetPersonalInfo() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
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
		} // GetPersonalInfo

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

		private readonly SortedSet<string> warningRules = new SortedSet<string> {
			"U001", "U004", "U007", "U013", "U015", "U131", "U133", "U135", "U018", "U0132", "U0134",
		};
	} // class AmlChecker
} // namespace
