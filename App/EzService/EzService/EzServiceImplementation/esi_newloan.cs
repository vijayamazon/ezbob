namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan;

	public partial class EzServiceImplementation : IEzServiceNewLoan {

		public IntActionResult AddCashRequest(int userID, NL_CashRequests cashRequest) {
			AddCashRequest stra;
			var amd = ExecuteSync(out stra, cashRequest.CustomerID, userID, cashRequest);
			return new IntActionResult {
				MetaData = amd,
				Value = stra.CashRequestID
			};
		}

		public IntActionResult AddDecision(int userID, int customerID, NL_Decisions decision, long? oldCashRequest, IEnumerable<NL_DecisionRejectReasons> decisionRejectReasons) {
			AddDecision stra;
			var amd = ExecuteSync(out stra, customerID, userID, decision, oldCashRequest, decisionRejectReasons);
			return new IntActionResult {
				MetaData = amd,
				Value = stra.DecisionID
			};
		}

		public IntActionResult AddOffer(int userID, int customerID, NL_Offers offer) {
			AddOffer stra;
			var amd = ExecuteSync(out stra, customerID, userID, offer);
			return new IntActionResult {
				MetaData = amd,
				Value = stra.OfferID
			};
		}

		public IntActionResult AddLoanLegals(int userID, int customerID, NL_LoanLegals loanLegals) {
			AddLoanLegals stra;
			var amd = ExecuteSync(out stra, customerID, userID, customerID, loanLegals);
			return new IntActionResult {
				MetaData = amd,
				Value = stra.LoanLegalsID
			};
		}

		public NL_Offers GetLastOffer(int userID, int customerID) {
			GetLastOffer stra;
			var amd = ExecuteSync(out stra, customerID, userID, customerID);
			return stra.Offer;
		}

		

		//public DateTimeActionResult ExampleMethod(int userID, int customerID) {
		//	LoaderStrategy loaderStrategy;
		//	Action<Loader> action = loader => loader.ExampleMethod(customerID);
		//	ActionMetaData amd = ExecuteSync(out loaderStrategy, customerID, userID, action);
		//	return new DateTimeActionResult {
		//		MetaData = amd,
		//		Value = loaderStrategy.NLModel.SomeTime,
		//	};
		//} // ExampleMethod

		//public ActionMetaData ExampleOtherMethod() {
		//	LoaderStrategy loaderStrategy;
		//	Action<Loader> action = loader => loader.ExampleOtherMethod();
		//	ActionMetaData amd = ExecuteSync(out loaderStrategy, null, null, action);
		//	return amd;
		//} // ExampleOtherMethod

		public IntActionResult AddLoan(NL_Model loanModel) {
			AddLoan strategy;
			var amd = ExecuteSync(out strategy, loanModel.CustomerID, loanModel.UserID, loanModel);
			return new IntActionResult {
				MetaData = amd,
				Value = strategy.LoanID
			};
		}

		public NL_Model AddPayment(NL_Model loanModel) {
			Console.WriteLine("++++++++++++++++++++++ESI++++++++++++++++++++++++++++");
			AddPayment strategy;
			var amd = ExecuteSync(out strategy, loanModel.CustomerID, loanModel.UserID, loanModel);
			return strategy.NLModel;
		}

		public IntActionResult AddLoanOptions(int userID, int customerID, NL_LoanOptions loanOptions) {
			AddLoanOptions stra;
			var amd = ExecuteSync(out stra, customerID, userID, customerID, loanOptions);
			return new IntActionResult {
				MetaData = amd,
				Value = stra.LoanOptionsID
			};
		}

	}
}