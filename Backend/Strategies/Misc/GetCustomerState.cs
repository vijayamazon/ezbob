namespace Ezbob.Backend.Strategies.Misc 
{
	using System;
	using ConfigManager;
	using Ezbob.Database;

	public class GetCustomerState : AStrategy
	{
		public string Result { get; private set; }
		
		public GetCustomerState(int customerID) {
			this.customerID = customerID;
		}
		public override string Name {
			get { return "GetCustomerState"; }
		}
		
		public override void Execute()
		{
			Result = string.Empty;
			try {
				var instance = new GetAvailableFunds();
				instance.Execute();
				decimal availableFunds = instance.AvailableFunds;

				SafeReader sr = DB.GetFirst("GetCustomerDetailsForStateCalculation", 
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", this.customerID));

				int minLoanAmount = CurrentValues.Instance.MinLoan;
				int xMinLoanAmount = CurrentValues.Instance.XMinLoan;
				int numOfActiveLoans = sr["NumOfActiveLoans"];
				bool isTest = sr["IsTest"];
				string creditResult = sr["CreditResult"];
				string status = sr["Status"];
				bool isEnabled = sr["IsEnabled"];
				bool hasLateLoans = sr["HasLateLoans"];
				DateTime offerStart = sr["ApplyForLoan"];
				DateTime offerValidUntil = sr["ValidFor"];
				bool hasFunds = isTest ? availableFunds >= xMinLoanAmount : availableFunds >= minLoanAmount;
				bool blockTakingLoan = sr["BlockTakingLoan"];
				bool canTakeAnotherLoan = numOfActiveLoans < (int)CurrentValues.Instance.NumofAllowedActiveLoans;

				if (!isEnabled || !canTakeAnotherLoan)
					Result = "disabled";
				else if (hasLateLoans)
					Result = "late";
				else if (string.IsNullOrEmpty(creditResult) || creditResult == "WaitingForDecision" || blockTakingLoan)
					Result = "wait";
				else if (status == "Rejected")
					Result = "bad";
				else if (status == "Manual")
					Result = "wait";
				else if (hasFunds && DateTime.UtcNow >= offerStart && DateTime.UtcNow <= offerValidUntil && status == "Approved")
					Result = "get";
				else if (hasFunds && DateTime.UtcNow < offerStart && offerStart < offerValidUntil && status == "Approved")
					Result = "wait";
				else if (!hasFunds || DateTime.UtcNow > offerValidUntil)
					Result = "apply";
			} catch (Exception e) {
				Log.Error("Exception occurred during calculation of customer's state. The exception:{0}", e);
			}
		}

		private readonly int customerID;
	}
}
