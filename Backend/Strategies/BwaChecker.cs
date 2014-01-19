namespace EzBob.Backend.Strategies
{
	using System;
	using System.Data;
	using ExperianLib.IdIdentityHub;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BwaChecker : AStrategy
	{
		private readonly IdHubService idHubService = new IdHubService();
		private readonly bool isCustom;
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
		private readonly int customerId;
		private string bankAccountType;
		private string sortCode;
		private string accountNumber;
		private readonly string idhubHouseNumber;
		private readonly string idhubHouseName;
		private readonly string idhubStreet;
		private readonly string idhubDistrict;
		private readonly string idhubTown;
		private readonly string idhubCounty;
		private readonly string idhubPostCode;
		private readonly string idhubBranchCode;
		private readonly string idhubAccountNumber;
		private string experianBwaAccountStatus;
		private decimal experianBwaNameScore;
		private decimal experianBwaAddressScore;

		public BwaChecker(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
			GetPersonalInfo();
			GetAddresses();
		}

		public BwaChecker(int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet, string idhubDistrict, string idhubTown, 
			string idhubCounty, string idhubPostCode, string idhubBranchCode, string idhubAccountNumber, AConnection oDb, ASafeLog oLog)
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
			this.idhubBranchCode = idhubBranchCode;
			this.idhubAccountNumber = idhubAccountNumber;
		}

		public override string Name
		{
			get { return "BWA check"; }
		} // Name

		public override void Execute()
		{
			bool hasError = isCustom ? GetBwaDataCustom() : GetBwaData();

			string result = CalculateBwaResult(hasError);

			DB.ExecuteNonQuery("UpdateBwaResult", CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("BwaResult", result));
		}

		private bool GetBwaData()
		{
			bool hasError = GetBwa(line1Current, line2Current, line3Current, line4Current, line6Current);

			if (hasError && timeAtAddress == 1 && line6Prev != null)
			{
				hasError = GetBwa(line1Prev, line2Prev, line3Prev, line4Prev, line6Prev);
			}

			return hasError;
		}

		private bool GetBwa(string line1, string line2, string line3, string line4, string line6)
		{
			AccountVerificationResults accountVerificationResults = idHubService.AccountVerification(
				firstName, null, surname, gender, dateOfBirth, line1, line2,
				line3, line4, null, line6, sortCode, accountNumber, customerId);

			return CreateBwaResultFromAccountVerificationResults(accountVerificationResults);
		}

		private bool GetBwaDataCustom()
		{
			AccountVerificationResults accountVerificationResults = idHubService.AccountVerificationForcedWithCustomAddress(
				firstName, null, surname, gender, dateOfBirth, idhubHouseNumber, idhubHouseName, idhubStreet,
				idhubDistrict, idhubTown, idhubCounty, idhubPostCode, idhubBranchCode, idhubAccountNumber, customerId);

			return CreateBwaResultFromAccountVerificationResults(accountVerificationResults);
		}

		private bool CreateBwaResultFromAccountVerificationResults(AccountVerificationResults results)
		{
			if (!results.HasError)
			{
				Log.Info("account status: {0}, name score: {1}, address score: {2}", results.AccountStatus, results.NameScore, results.AddressScore);
				experianBwaAccountStatus = results.AccountStatus;
				experianBwaNameScore = results.NameScore;
				experianBwaAddressScore = results.AddressScore;
				return false;
			}
			
			return true;
		} // CreateBwaResultFromAccountVerificationResults

		private string CalculateBwaResult(bool hasError)
		{
			if (sortCode == null && accountNumber == null)
				return "Not performed";
			
			if (hasError)
				return "Warning";

			if (bankAccountType == "Business")
				return "Not performed";
			
			if (experianBwaAccountStatus == "No Match" ||
				experianBwaAccountStatus == "Unable to check" ||
				experianBwaNameScore == 1 ||
				experianBwaNameScore == 2 ||
				experianBwaNameScore == 3 ||
				experianBwaNameScore == 4 ||
				experianBwaAddressScore == 1 ||
				experianBwaAddressScore == 2 ||
				experianBwaAddressScore == 3 ||
				experianBwaAddressScore == 4)
			{
				return "Warning";
			}

			return "Passed";
		}

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
			accountNumber = results["AccountNumber"];
			sortCode = results["SortCode"];
			bankAccountType = results["BankAccountType"];
		}
	}
}
