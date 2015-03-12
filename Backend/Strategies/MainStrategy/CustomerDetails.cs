namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Database;

	internal class CustomerDetails {
		public CustomerDetails(int customerID) {
			Library.Instance.Log.Info("Getting personal info for customer:{0}", customerID);

			SafeReader results = Library.Instance.DB.GetFirst(
				"GetPersonalInfo",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerID)
			);

			ID = customerID;
			CustomerStatusIsEnabled = results["CustomerStatusIsEnabled"];
			CustomerStatusIsWarning = results["CustomerStatusIsWarning"];
			CustomerStatusName = results["CustomerStatusName"];
			IsOffline = results["IsOffline"];
			AppEmail = results["CustomerEmail"];
			AppFirstName = results["FirstName"];
			AppSurname = results["Surname"];
			AppGender = results["Gender"];
			IsOwnerOfMainAddress = results["IsOwnerOfMainAddress"];
			IsOwnerOfOtherProperties = results["IsOwnerOfOtherProperties"];
			PropertyStatusDescription = results["PropertyStatusDescription"];
			AllMPsNum = results["NumOfMps"];
			AppRegistrationDate = results["RegistrationDate"];
			NumOfLoans = results["NumOfLoans"];
			NumOfHmrcMps = results["NumOfHmrcMps"];
			IsAlibaba = results["IsAlibaba"];
			BrokerId = results["BrokerId"];
			NumOfYodleeMps = results["NumOfYodleeMps"];
			EarliestHmrcLastUpdateDate = results["EarliestHmrcLastUpdateDate"];
			EarliestYodleeLastUpdateDate = results["EarliestYodleeLastUpdateDate"];
			IsTest = results["IsTest"];

			var scoreStrat = new GetExperianConsumerScore(customerID);
			scoreStrat.Execute();
			ExperianConsumerScore = scoreStrat.Score;
		} // constructor

		public int ID { get; private set; }
		public bool CustomerStatusIsEnabled { get; private set; }
		public bool CustomerStatusIsWarning { get; private set; }
		public string CustomerStatusName { get; private set; }
		public bool IsOffline { get; private set; }
		public bool IsTest { get; private set; }
		public string AppEmail { get; private set; }
		public string AppFirstName { get; private set; }
		public string AppSurname { get; private set; }
		public string AppGender { get; private set; }
		public bool IsOwnerOfMainAddress { get; private set; }
		public bool IsOwnerOfOtherProperties { get; private set; }
		public string PropertyStatusDescription { get; private set; }
		public int AllMPsNum { get; private set; }
		public DateTime AppRegistrationDate { get; private set; }
		public int NumOfLoans { get; private set; }
		public int NumOfHmrcMps { get; private set; }
		public bool IsAlibaba { get; private set; }
		public int? BrokerId { get; private set; }
		public int ExperianConsumerScore { get; private set; }
		public int NumOfYodleeMps { get; private set; }
		public DateTime? EarliestHmrcLastUpdateDate { get; private set; }
		public DateTime? EarliestYodleeLastUpdateDate { get; private set; }
	} // class CustomerDetails
} // namespace
