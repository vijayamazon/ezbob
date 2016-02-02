namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Database;

	internal class CustomerDetails {
		public CustomerDetails(int customerID) {
			RequestedID = customerID;
			ID = 0;
		} // constructor

		public void Load() {
			if (RequestedID <= 0)
				return;

			if (RequestedID == ID)
				return;

			Library.Instance.Log.Debug("Getting personal info for customer {0}...", RequestedID);

			SafeReader results = Library.Instance.DB.GetFirst(
				"GetPersonalInfo",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", RequestedID)
			);

			if (results.IsEmpty)
				return;

			results.Fill(this);

			var scoreStrat = new GetExperianConsumerScore(RequestedID);
			scoreStrat.Execute();
			ExperianConsumerScore = scoreStrat.Score;

			ID = RequestedID;

			Library.Instance.Log.Debug("Getting personal info for customer {0} complete.", RequestedID);
		} // Load

		public int ID { get; private set; }
		public int RequestedID { get; private set; }

		public bool CustomerStatusIsEnabled { get; set; }
		public bool CustomerStatusIsWarning { get; set; }
		public string CustomerStatusName { get; set; }
		public bool IsOffline { get; set; }
		public bool IsTest { get; set; }
		[FieldName("CustomerEmail")]
		public string AppEmail { get; set; }
		[FieldName("FirstName")]
		public string AppFirstName { get; set; }
		[FieldName("Surname")]
		public string AppSurname { get; set; }
		[FieldName("Gender")]
		public string AppGender { get; set; }
		public bool IsOwnerOfMainAddress { get; set; }
		public bool IsOwnerOfOtherProperties { get; set; }
		public string PropertyStatusDescription { get; set; }
		[FieldName("NumOfMps")]
		public int AllMPsNum { get; set; }
		[FieldName("RegistrationDate")]
		public DateTime AppRegistrationDate { get; set; }
		public int NumOfLoans { get; set; }
		public int NumOfHmrcMps { get; set; }
		public bool IsAlibaba { get; set; }
		public int? BrokerId { get; set; }
		public int NumOfYodleeMps { get; set; }
		public DateTime? EarliestHmrcLastUpdateDate { get; set; }
		public DateTime? EarliestYodleeLastUpdateDate { get; set; }
		public bool FilledByBroker { get; set; }
		public int NumOfPreviousApprovals { get; set; }
		public string FullName { get; set; }
		public string Origin { get; set; }
		public int OriginID { get; set; }
		public int ExperianConsumerScore { get; private set; }

		public bool OwnsProperty {
			get { return IsOwnerOfMainAddress || IsOwnerOfOtherProperties; }
		} // OwnsProperty

		public bool IsValid {
			get { return (ID > 0) && (ID == RequestedID); }
		} // IsValid
	} // class CustomerDetails
} // namespace
