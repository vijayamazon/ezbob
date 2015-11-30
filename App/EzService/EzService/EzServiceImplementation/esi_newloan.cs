﻿namespace EzService.EzServiceImplementation {
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
            ExecuteSync(out stra, customerID, userID, customerID);
            return stra.Offer;
        } // GetLastOffer

        public ActionMetaData AddLoan(int? userID, int? customerID, NL_Model model) {
            return Execute<AddLoan>(model.CustomerID, model.UserID, model);
        } // AddLoan

        public NLLongActionResult AddPayment(int customerID, NL_Payments payment, int userID) {
            AddPayment strategy;
			 var amd = ExecuteSync(out strategy, customerID, userID, customerID, payment, userID);
            return new NLLongActionResult {
                MetaData = amd,
                Value = strategy.PaymentID,
                Error = strategy.Error
            };
        } // AddPayment

        public ListNewLoanActionResult GetCustomerLoans(int customerID, int userID) {
            GetCustomerLoans strategy;
            var amd = ExecuteSync(out strategy, customerID, userID);
            return new ListNewLoanActionResult {
                MetaData = amd,
                Value = strategy.Loans,
                Error = strategy.Error
            };
        } // GetCustomerLoans*/

        public NewLoanModelActionResult GetLoanState(int customerID, long loanID, DateTime utcNow, int userID) {
            GetLoanState strategy;
            var amd = ExecuteSync(out strategy, customerID, userID, customerID, loanID, utcNow, userID);
            return new NewLoanModelActionResult {
                MetaData = amd,
                Value = strategy.Result,
                Error = strategy.Error
            };
        } // GetLoanState

        public NLLongActionResult GetLoanByOldID(int oldId, int customerID = 1, int userID = 1) {
			GetLoanIDByOldID strategy;
            var amd = ExecuteSync(out strategy, customerID, userID, oldId);
            return new NLLongActionResult {
                MetaData = amd,
                Value = strategy.LoanID,
                Error = strategy.Error
            };
        } // GetLoanByOldID

        public NLLongActionResult AddLoanOptions(int userID, int customerID, NL_LoanOptions loanOptions, int? oldLoanId, List<String> PropertiesUpdateList = null) {
            AddLoanOptions stra;
            var amd = ExecuteSync(out stra, customerID, userID, loanOptions, oldLoanId, PropertiesUpdateList);
            return new NLLongActionResult {
                MetaData = amd,
                Value = stra.LoanOptionsID
            };
        } // AddLoanOptions

        public NLLongActionResult AddLoanInterestFreeze(int userID,
                                                        int customerID,
                                                        int? oldLoanId,
                                                        NL_LoanInterestFreeze loanInterestFreeze) {
            AddLoanInterestFreeze stra;
            var amd = ExecuteSync(out stra, customerID, userID, oldLoanId, loanInterestFreeze);
            return new NLLongActionResult {
                MetaData = amd,
                Value = stra.LoanInterestFreezeID
            };
        } // AddLoanOptions

        public NLLongActionResult DeactivateLoanInterestFreeze(int userID,
                                                                int customerID,
                                                                int? oldLoanId,
                                                                int oldLoanInterestFreezeID,
                                                                DateTime? deactivationDate) {
            DeactivateLoanInterestFreeze stra;
            var amd = ExecuteSync(out stra, customerID, userID, oldLoanId, oldLoanInterestFreezeID, deactivationDate);
            return new NLLongActionResult {
                MetaData = amd,
                Value = stra.OldLoanInterestFreezeID
            };
        } // AddLoanOptions

        public ReschedulingActionResult RescheduleLoan(int userID, int customerID, ReschedulingArgument reAgrs) {
            Type t;

            try {
                t = Type.GetType(reAgrs.LoanType);
            }
            catch (Exception e) {
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
            }
            catch (Exception e) {
                Log.Alert("Reschedule; exception: ", e);
                result.Error = "InternalServerError";
            } // try

            return new ReschedulingActionResult {
                Value = result
            };
        } // RescheduleLoan

        public NewLoanModelActionResult BuildLoanFromOffer(int? userID, int? customerID, NL_Model model) {
            ActionMetaData amd = null;
            BuildLoanFromOffer strategy = new BuildLoanFromOffer(model);
            try {
                amd = ExecuteSync(out strategy, customerID, userID, model);
                // ReSharper disable once CatchAllClause
            }
            catch (Exception e) {
                Log.Alert("BuildLoanFromOffer failed: {0}", e);
                strategy.Result.Error = "InternalServerError";
            }
            return new NewLoanModelActionResult() {
                MetaData = amd,
                Value = strategy.Result
            };
        }

		 public NLLongActionResult CancelPayment(int customerID, NL_Payments payment, int userID) {
			 CancelPayment strategy;
			 var amd = ExecuteSync(out strategy, customerID, userID, customerID, payment, userID);
			 return new NLLongActionResult {
				 MetaData = amd,
				 Error = strategy.Error
			 };
		 } // AddPayment

    } // class EzServiceImplementation
} // namespace
