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
			AddCashRequest s = new AddCashRequest(cashRequest);
			s.Context.UserID = userID;
			s.Context.CustomerID = cashRequest.CustomerID;
			var amd = ExecuteSync(out s, cashRequest.CustomerID, userID, cashRequest);
			return new NLLongActionResult {
				MetaData = amd,
				Value = s.CashRequestID,
				Error = s.Error
			};
		} // AddCashRequest

		public NLLongActionResult AddDecision(int userID, int customerID, NL_Decisions decision, long? oldCashRequest, IEnumerable<NL_DecisionRejectReasons> decisionRejectReasons) {
			AddDecision s = new AddDecision(decision, oldCashRequest, decisionRejectReasons);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			var amd = ExecuteSync(out s, customerID, userID, decision, oldCashRequest, decisionRejectReasons);
			return new NLLongActionResult {
				MetaData = amd,
				Value = s.DecisionID,
				Error = s.Error
			};
		} // AddDecision

		public NLLongActionResult AddOffer(int userID, int customerID, NL_Offers offer, List<NL_OfferFees> fees = null) {
			AddOffer s = new AddOffer(offer, fees);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			var amd = ExecuteSync(out s, customerID, userID, offer, fees);
			return new NLLongActionResult {
				MetaData = amd,
				Value = s.OfferID,
				Error = s.Error
			};
		} // AddOffer

		public NLLongActionResult AddLoanLegals(int userID, int customerID, NL_LoanLegals loanLegals) {
			AddLoanLegals s = new AddLoanLegals(customerID, loanLegals);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			var amd = ExecuteSync(out s, customerID, userID, customerID, loanLegals);
			return new NLLongActionResult {
				MetaData = amd,
				Value = s.LoanLegalsID,
				Error = s.Error
			};
		} // AddLoanLegals

		public NL_Offers GetLastOffer(int userID, int customerID) {
			GetLastOffer s = new GetLastOffer(customerID);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			ExecuteSync(out s, customerID, userID, customerID);
			return s.Offer;
		} // GetLastOffer

		public ActionMetaData AddLoan(int? userID, int? customerID, NL_Model model) {
			return Execute<AddLoan>(model.CustomerID, model.UserID, model);
		} // AddLoan

		public NLLongActionResult AddPayment(int customerID, NL_Payments payment, int userID) {
			AddPayment s = new AddPayment(customerID, payment, userID);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			var amd = ExecuteSync(out s, customerID, userID, customerID, payment, userID);
			return new NLLongActionResult {
				MetaData = amd,
				Value = s.PaymentID,
				Error = s.Error
			};
		} // AddPayment

		public ListNewLoanActionResult GetCustomerLoans(int customerID, int userID=1) {
			GetCustomerLoans s = new GetCustomerLoans(customerID);
			s.Context.CustomerID = customerID;
			s.Context.UserID = userID;
			var amd = ExecuteSync(out s, customerID, userID, customerID);
			return new ListNewLoanActionResult {
				MetaData = amd,
				Value = s.Loans,
				Error = s.Error
			};
		} // GetCustomerLoans

		public NLModelActionResult GetLoanState(int customerID, long loanID, DateTime calculationDate, int userID, bool getCalculatorState = true) {
			GetLoanState s = new GetLoanState(customerID, loanID, calculationDate, userID, getCalculatorState);
			s.Context.CustomerID = customerID;
			s.Context.UserID = userID;
			var amd = ExecuteSync(out s, customerID, userID, customerID, loanID, calculationDate, userID, getCalculatorState);
			return new NLModelActionResult {
				MetaData = amd,
				Value = s.Result,
				Error = s.Error
			};
		} // GetLoanState

		public NLLongActionResult GetLoanByOldID(int oldId, int customerID = 1, int userID = 1) {
			GetLoanIDByOldID s = new GetLoanIDByOldID(oldId);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			var amd = ExecuteSync(out s, customerID, userID, oldId);
			return new NLLongActionResult {
				MetaData = amd,
				Value = s.LoanID,
				Error = s.Error
			};
		} // GetLoanByOldID

		public NLLongActionResult AddLoanOptions(int userID, int customerID, NL_LoanOptions loanOptions, int? oldLoanId, List<String> propertiesUpdateList = null) {
			AddLoanOptions s = new AddLoanOptions(loanOptions, oldLoanId, propertiesUpdateList);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			var amd = ExecuteSync(out s, customerID, userID, loanOptions, oldLoanId, propertiesUpdateList);
			return new NLLongActionResult {
				MetaData = amd,
				Value = s.LoanOptionsID
			};
		} // AddLoanOptions

		public NLLongActionResult AddLoanInterestFreeze(int userID,int customerID,NL_LoanInterestFreeze loanInterestFreeze) {
			AddLoanInterestFreeze s = new AddLoanInterestFreeze( loanInterestFreeze);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			var amd = ExecuteSync(out s, customerID, userID, loanInterestFreeze);
			return new NLLongActionResult {	
				MetaData = amd,
				Value = s.LoanFreezeInterval.LoanInterestFreezeID
			};
		} // AddLoanOptions

		public NLLongActionResult DeactivateLoanInterestFreeze(int userID, int customerID, NL_LoanInterestFreeze loanInterestFreeze) {
			DeactivateLoanInterestFreeze s = new DeactivateLoanInterestFreeze(loanInterestFreeze);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			var amd = ExecuteSync(out s, customerID, userID, loanInterestFreeze);
			return new NLLongActionResult {
				MetaData = amd,
				Value = s.LoanFreezeInterval.LoanInterestFreezeID
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
				Log.Alert("Reschedule; exception: ", e.Message);
				result.Error = "InternalServerError";
			} // try

			return new ReschedulingActionResult {
				Value = result
			};
		} // RescheduleLoan

		public NLModelActionResult BuildLoanFromOffer(int? userID, int? customerID, NL_Model model) {
			ActionMetaData amd = null;
			BuildLoanFromOffer strategy = new BuildLoanFromOffer(model);
			try {
				amd = ExecuteSync(out strategy, customerID, userID, model);
				// ReSharper disable once CatchAllClause
			} catch (Exception e) {
				Log.Alert("BuildLoanFromOffer failed: {0}", e);
				strategy.Result.Error = "InternalServerError";
			}
			return new NLModelActionResult() {
				MetaData = amd,
				Value = strategy.Result
			};
		}

		public NLLongActionResult CancelPayment(int customerID, NL_Payments payment, int userID) {
			CancelPayment s;
			var amd = ExecuteSync(out s, customerID, userID, customerID, payment, userID);
			return new NLLongActionResult {
				MetaData = amd,
				Error = s.Error
			};
		} // AddPayment

		public StringActionResult SaveRollover(int userID, int customerID, NL_LoanRollovers rollover, long loanID) {
			SaveRollover s = new SaveRollover(rollover, loanID);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			var amd = ExecuteSync(out s, customerID, userID, rollover, loanID);
			return new StringActionResult {
				Value = s.Error
			};
		} // SaveRollover

		public StringActionResult AcceptRollover(int customerID, long loanID) {
			AcceptRollover s = new AcceptRollover(customerID, loanID);
			s.Context.UserID = customerID;
			s.Context.CustomerID = customerID;
			s.Execute();
			return new StringActionResult {
				Value = s.Error
			};
		} // AcceptRollover

		public StringActionResult SaveFee(int userID, int customerID, NL_LoanFees fee) {
			SaveFee s = new SaveFee(fee);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			var amd = ExecuteSync(out s, customerID, userID, fee);
			return new StringActionResult {
				Value = s.Error
			};
		} // SaveFee

		public StringActionResult CancelFee(int userID, int customerID, NL_LoanFees fee) {
			CancelFee s = new CancelFee(fee);
			s.Context.UserID = userID;
			s.Context.CustomerID = customerID;
			var amd = ExecuteSync(out s, customerID, userID, fee);
			return new StringActionResult {
				Value = s.Error
			};
		} // CancelFee

	} // class EzServiceImplementation
} // namespace
