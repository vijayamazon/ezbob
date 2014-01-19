namespace EzBob.Backend.Strategies
{
	using System;
	using ExperianLib.IdIdentityHub;

	public class BwaChecker
	{
		private readonly IdHubService idHubService = new IdHubService();
		private readonly bool isCustom;
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
		private readonly int customerId;
		private string sortCode;
		private string accountNumber;
		private string idhubHouseNumber;
		private string idhubHouseName;
		private string idhubStreet;
		private string idhubDistrict;
		private string idhubTown;
		private string idhubCounty;
		private string idhubPostCode;
		private string idhubBranchCode;
		private string idhubAccountNumber;
		private string experianBwaAccountStatus;
		private decimal experianBwaNameScore;
		private decimal experianBwaAddressScore;
		private string bankAccountType;

		public BwaChecker(int customerId, string firstName, string surname, string gender, DateTime dateOfBirth, string bankAccountType,
			string line1Current, string line2Current, string line3Current, string line4Current, string line6Current,
			string line1Prev, string line2Prev, string line3Prev, string line4Prev, string line6Prev,
			int timeAtAddress)
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
			this.bankAccountType = bankAccountType;
		}

		public BwaChecker(int customerId, string firstName, string surname, string gender, DateTime dateOfBirth, string bankAccountType,
			string idhubHouseNumber, string idhubHouseName, string idhubStreet,
			string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode, string idhubBranchCode, string idhubAccountNumber)
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
			this.idhubBranchCode = idhubBranchCode;
			this.idhubAccountNumber = idhubAccountNumber;
			this.bankAccountType = bankAccountType;
		}

		public string Check()
		{
			bool hasError = isCustom ? GetBwaDataCustom() : GetBwaData();

			return CalculateBwaResult(hasError);
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
				//Log.Info("account status: {0}, name score: {1}, address score: {2}", results.AccountStatus, results.NameScore, results.AddressScore);
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
	}
}
