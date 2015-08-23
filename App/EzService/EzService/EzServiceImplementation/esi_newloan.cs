namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan;
	using EzService.ActionResults;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public partial class EzServiceImplementation : IEzServiceNewLoan {
		public NLLongActionResult AddCashRequest(int userID, NL_CashRequests cashRequest) {
			AddCashRequest strategy;
			var amd = ExecuteSync(out strategy, cashRequest.CustomerID, userID, cashRequest);
			return new NLLongActionResult {
				MetaData = amd,
				Value = strategy.CashRequestID,
				Error = strategy.Error
			};
		} // AddCashRequest

		public NLLongActionResult AddDecision(int userID, int customerID, NL_Decisions decision, long? oldCashRequest, IEnumerable<NL_DecisionRejectReasons> decisionRejectReasons) {
			AddDecision strategy;
			var amd = ExecuteSync(out strategy, customerID, userID, decision, oldCashRequest, decisionRejectReasons);
			return new NLLongActionResult {
				MetaData = amd,
				Value = strategy.DecisionID,
				Error = strategy.Error
			};
		} // AddDecision

		public NLLongActionResult AddOffer(int userID, int customerID, NL_Offers offer, List<NL_OfferFees> fees = null) {
			AddOffer strategy;
			var amd = ExecuteSync(out strategy, customerID, userID, offer, fees);
			return new NLLongActionResult {
				MetaData = amd,
				Value = strategy.OfferID,
				Error = strategy.Error
			};
		} // AddOffer

		public NLLongActionResult AddLoanLegals(int userID, int customerID, NL_LoanLegals loanLegals) {
			AddLoanLegals strategy;
			var amd = ExecuteSync(out strategy, customerID, userID, customerID, loanLegals);
			return new NLLongActionResult {
				MetaData = amd,
				Value = strategy.LoanLegalsID,
				Error = strategy.Error
			};
		} // AddLoanLegals

		public NL_Offers GetLastOffer(int userID, int customerID) {
			GetLastOffer stra;
			var amd = ExecuteSync(out stra, customerID, userID, customerID);
			return stra.Offer;
		} // GetLastOffer

		public NLLongActionResult AddLoan(int? userID, int? customerID, NL_Model model) {
			AddLoan strategy;
			var amd = ExecuteSync(out strategy, model.CustomerID, model.UserID, model);
			return new NLLongActionResult {
				MetaData = amd,
				Value = strategy.LoanID,
				Error = strategy.Error
			};
		} // AddLoan

		public NL_Model AddPayment(NL_Model loanModel) {
			AddPayment strategy;
			var amd = ExecuteSync(out strategy, loanModel.CustomerID, loanModel.UserID, loanModel);
			return strategy.NLModel;
		} // AddPayment

		public NLLongActionResult AddLoanOptions(int userID, int customerID, NL_LoanOptions loanOptions, int? oldLoanId) {
			AddLoanOptions stra;
			var amd = ExecuteSync(out stra, customerID, userID, customerID, loanOptions, oldLoanId);
			return new NLLongActionResult {
				MetaData = amd,
				Value = stra.LoanOptionsID
			};
		} // AddLoanOptions

		public ReschedulingActionResult RescheduleLoan(int userID, int customerID, ReschedulingArgument reAgrs) {
			Type t;

			try {
				t = Type.GetType(reAgrs.LoanType);
			} catch (Exception e) {
				Log.Alert("Fail to get type from the argument {0}. ReschedulingArgument: {1}; error: {2}", reAgrs.LoanType, reAgrs, e);
				return null;
			} // try

			if (t == null) {
				Log.Alert("Type t (of loan) not found");
				return null;
			} // if

			//Log.Debug("t is {0}, t.Name={1}", t, t.Name);

			ReschedulingResult result = new ReschedulingResult();

			try {
				if (t.Name == "Loan") {
					RescheduleLoan<Loan> strategy;
					var amd = ExecuteSync(out strategy, customerID, userID, new Loan(), reAgrs);
					return new ReschedulingActionResult {
						MetaData = amd,
						Value = strategy.Result
					};
				} // if

				if (t.Name == "NL_Model") {
					//RescheduleLoan<NL_Model> strategy;
					// TODO
				} // if
			} catch (Exception e) {
				Log.Alert("Reschedule; exception: ", e);
				result.Error = "InternalServerError";
			} // try

			return new ReschedulingActionResult {
				Value = result
			};
		} // RescheduleLoan

		public NewLoanModelActionResult CalculateLoanSchedule(int? userID, int? customerID, NL_Model model) {
			ActionMetaData amd = null;
			CalculateLoanSchedule strategy = new CalculateLoanSchedule(model);

			try {
				amd = ExecuteSync(out strategy, customerID, userID, model);
				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Log.Alert("CalculateLoanSchedule exception: ", e);
				strategy.Result.Error = "InternalServerError";
			}

			return new NewLoanModelActionResult() {
				MetaData = amd,
				Value = strategy.Result
			};
		} // CalculateLoanSchedule
	} // class EzServiceImplementation
} // namespace
