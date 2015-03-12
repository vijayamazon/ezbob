namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using ConfigManager;
	using Ezbob.Database;

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
			TypeOfBusiness = results["TypeOfBusiness"];
		} // constructor

		public string BwaBusinessCheck { get; private set; }
		public string AppBankAccountType { get; private set; }
		public string AppAccountNumber { get; private set; }
		public string AppSortCode { get; private set; }
		public DateTime? LastStartedMainStrategyEndTime { get; private set; }
		public string TypeOfBusiness { get; private set; }
	} // class PreliminaryData
} // namespace
