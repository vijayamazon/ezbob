namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using ExperianLib.IdIdentityHub;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StoredProcs;

	public class AmlChecker : AStrategy {
		#region public

		#region constructor

		public AmlChecker(int customerId, AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
			m_nCustomerID = customerId;
			GetPersonalInfo();
			m_oCustomerAddress = new GetCustomerAddresses(m_nCustomerID, DB, Log).FillFirst<GetCustomerAddresses.ResultRow>();
		} // constructor

		public AmlChecker(
			int customerId,
			string idhubHouseNumber,
			string idhubHouseName,
			string idhubStreet,
			string idhubDistrict,
			string idhubTown,
			string idhubCounty,
			string idhubPostCode,
			AConnection oDb,
			ASafeLog oLog
		) : base(oDb, oLog) {
			m_bIsCustom = true;
			m_nCustomerID = customerId;
			GetPersonalInfo();

			m_sIdhubHouseNumber = idhubHouseNumber;
			m_sIdhubHouseName = idhubHouseName;
			m_sIdhubStreet = idhubStreet;
			m_sIdhubDistrict = idhubDistrict;
			m_sIdhubTown = idhubTown;
			m_sIdhubCounty = idhubCounty;
			m_sIdhubPostCode = idhubPostCode;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "AML check"; }
		} // Name

		#endregion property Name

		#region method Execute
		
		public override void Execute() {
			string result, description;
			decimal authentication;

			bool hasError = m_bIsCustom
				? GetAmlDataCustom(out result, out authentication, out description)
				: GetAmlData(out result, out authentication, out description);
			
			if (hasError || authentication < CurrentValues.Instance.MinAuthenticationIndexToPassAml)
				result = "Warning";

			DB.ExecuteNonQuery("UpdateAmlResult", CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", m_nCustomerID),
				new QueryParameter("AmlResult", result),
				new QueryParameter("AmlScore", (int)authentication),
				new QueryParameter("AmlDescription", description)
			);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		#region method GetAmlData

		private bool GetAmlData(out string result, out decimal authentication, out string description) {
			Log.Info("Starting standard AML check with parameters: FirstName={0} Surname={1} Gender={2} DateOfBirth={3} {4}",
				m_sFirstName, m_sLastName, m_sGender, m_oDateOfBirth, m_oCustomerAddress
			);

			bool hasError = GetAml(AddressCurrency.Current, out result, out authentication, out description);

			if (hasError && (m_nTimeAtAddress == 1) && !string.IsNullOrWhiteSpace(m_oCustomerAddress[6, AddressCurrency.Previous]))
				hasError = GetAml(AddressCurrency.Previous, out result, out authentication, out description);

			return hasError;
		} // GetAmlData

		#endregion method GetAmlData

		#region method GetAmlDataCustom

		private bool GetAmlDataCustom(out string result, out decimal authentication, out string description) {
			Log.Info(
				"Starting custom AML check with parameters: FirstName={0} Surname={1} Gender={2} DateOfBirth={3} idhubHouseNumber={4} idhubHouseName={5} idhubStreet={6} idhubDistrict={7} idhubTown={8} idhubCounty={9} idhubPostCode={10}",
				m_sFirstName,
				m_sLastName,
				m_sGender,
				m_oDateOfBirth,
				m_sIdhubHouseNumber,
				m_sIdhubHouseName,
				m_sIdhubStreet,
				m_sIdhubDistrict,
				m_sIdhubTown,
				m_sIdhubCounty,
				m_sIdhubPostCode
			);

			AuthenticationResults authenticationResults = m_oIdHubService.AuthenticateForcedWithCustomAddress(
				m_sFirstName,
				null,
				m_sLastName,
				m_sGender,
				m_oDateOfBirth,
				m_sIdhubHouseNumber,
				m_sIdhubHouseName,
				m_sIdhubStreet,
				m_sIdhubDistrict,
				m_sIdhubTown,
				m_sIdhubCounty,
				m_sIdhubPostCode,
				m_nCustomerID
			);

			return CreateAmlResultFromAuthenticationReuslts(authenticationResults, out result, out authentication, out description);
		} // GetAmlDataCustom

		#endregion method GetAmlDataCustom

		#region method GetAml

		private bool GetAml(AddressCurrency nCurrency, out string result, out decimal authentication, out string description)
		{
			AuthenticationResults authenticationResults = m_oIdHubService.Authenticate(
				m_sFirstName,
				null,
				m_sLastName,
				m_sGender,
				m_oDateOfBirth,
				m_oCustomerAddress[1, nCurrency],
				m_oCustomerAddress[2, nCurrency],
				m_oCustomerAddress[3, nCurrency],
				m_oCustomerAddress[4, nCurrency],
				null,
				m_oCustomerAddress[6, nCurrency],
				m_nCustomerID
			);

			return CreateAmlResultFromAuthenticationReuslts(authenticationResults, out result, out authentication, out description);
		} // GetAml

		#endregion method GetAml

		#region method CreateAmlResultFromAuthenticationReuslts

		private bool CreateAmlResultFromAuthenticationReuslts(AuthenticationResults results, out string result, out decimal authentication, out string description) {
			if (results.HasError) {
				Log.Info("Error getting AML data: {0}", results.Error);
				result = string.Empty;
				description = string.Empty;
				authentication = 0;
				return true;
			} // if

			authentication = results.AuthenticationIndexType;
			result = "Passed";
			description = results.AuthIndexText;

			foreach (var returnedHrp in results.ReturnedHRP) {
				if (m_oWarningRules.Contains(returnedHrp.HighRiskPolRuleID)) {
					result = "Warning";
					break;
				} // if
			} // for each

			return false;
		} // CreateAmlResultFromAuthenticationReuslts

		#endregion method CreateAmlResultFromAuthenticationReuslts

		#region method GetPersonalInfo

		private void GetPersonalInfo() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					m_sFirstName = sr["FirstName"];
					m_sLastName = sr["Surname"];
					m_sGender = sr["Gender"];
					m_oDateOfBirth = sr["DateOfBirth"];
					m_nTimeAtAddress = sr["TimeAtAddress"];
					return ActionResult.SkipAll;
				},
				"GetPersonalInfo",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", m_nCustomerID)
			);
		} // GetPersonalInfo

		#endregion method GetPersonalInfo

		#region fields

		private readonly IdHubService m_oIdHubService = new IdHubService();
		private int m_nTimeAtAddress;

		private readonly GetCustomerAddresses.ResultRow m_oCustomerAddress;

		private string m_sFirstName;
		private string m_sLastName;
		private string m_sGender;
		private DateTime m_oDateOfBirth;
		private readonly bool m_bIsCustom;
		private readonly int m_nCustomerID;

		private readonly string m_sIdhubHouseNumber;
		private readonly string m_sIdhubHouseName;
		private readonly string m_sIdhubStreet;
		private readonly string m_sIdhubDistrict;
		private readonly string m_sIdhubTown;
		private readonly string m_sIdhubCounty;
		private readonly string m_sIdhubPostCode;

		private readonly SortedSet<string> m_oWarningRules = new SortedSet<string> {
			"U001", "U004", "U007", "U013", "U015", "U131", "U133", "U135", "U018", "U0132", "U0134",
		};

		#endregion fields

		#endregion private
	} // class AmlChecker
} // namespace
