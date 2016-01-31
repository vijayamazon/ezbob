namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System;
	using ConfigManager;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	internal class PreliminaryData {
		public PreliminaryData(int customerID) {
			BwaBusinessCheck = CurrentValues.Instance.BWABusinessCheck;

			SafeReader results = Library.Instance.DB.GetFirst(
				"GetPersonalInfo",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerID)
			);

			AppBankAccountType = results["BankAccountType"];
			LastStartedMainStrategyEndTime = results["LastStartedMainStrategyEndTime"];
			AppAccountNumber = results["AccountNumber"];
			AppSortCode = results["SortCode"];

			TypeOfBusiness typeOfBusiness;
			
			if (Enum.TryParse(results["TypeOfBusiness"], out typeOfBusiness))
				TypeOfBusiness = typeOfBusiness;
			else
				TypeOfBusiness = TypeOfBusiness.Entrepreneur;
		} // constructor

		public string BwaBusinessCheck { get; private set; }
		public string AppBankAccountType { get; private set; }
		public string AppAccountNumber { get; private set; }
		public string AppSortCode { get; private set; }
		public DateTime? LastStartedMainStrategyEndTime { get; private set; }
		public TypeOfBusiness TypeOfBusiness { get; private set; }
	} // class PreliminaryData
} // namespace
