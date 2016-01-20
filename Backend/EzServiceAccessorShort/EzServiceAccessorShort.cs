namespace EzServiceShortcut {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.ModelsWithDB;
    using Ezbob.Backend.ModelsWithDB.Experian;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.Investor;
    using Ezbob.Backend.Strategies.Experian;
    using Ezbob.Backend.Strategies.MailStrategies;
    using Ezbob.Backend.Strategies.Misc;
    using Ezbob.Backend.Strategies.NewLoan;
    using Ezbob.Backend.Strategies.NewLoan.Exceptions;
    using Ezbob.Backend.Strategies.VatReturn;
    using Ezbob.Utils;
    using EzServiceAccessor;

    public class EzServiceAccessorShort : IEzServiceAccessor {
        public ElapsedTimeInfo SaveVatReturnData(
            int nCustomerMarketplaceID,
            int nHistoryRecordID,
            int nSourceID,
            VatReturnRawData[] oVatReturn,
            RtiTaxMonthRawData[] oRtiMonths
        ) {
            var stra = new SaveVatReturnData(nCustomerMarketplaceID, nHistoryRecordID, nSourceID, oVatReturn, oRtiMonths);
            stra.Execute();
            return stra.ElapsedTimeInfo;
        } // SaveVatReturnData

        public VatReturnFullData LoadVatReturnFullData(int nCustomerID, int nCustomerMarketplaceID) {
            var stra = new LoadVatReturnFullData(nCustomerID, nCustomerMarketplaceID);
            stra.Execute();

            return new VatReturnFullData {
                VatReturnRawData = stra.VatReturnRawData,
                RtiTaxMonthRawData = stra.RtiTaxMonthRawData,
                Summary = stra.Summary,
                BankStatement = stra.BankStatement,
                BankStatementAnnualized = stra.BankStatementAnnualized,
            };
        }

        public ExperianConsumerData ParseExperianConsumer(long nServiceLogID) {
            var stra = new ParseExperianConsumerData(nServiceLogID);
            stra.Execute();
            return stra.Result;
        }

        public ExperianConsumerData LoadExperianConsumer(int userId, int customerId, int? directorId, long? nServiceLogId) {
            var stra = new LoadExperianConsumerData(customerId, directorId, nServiceLogId);
            stra.Execute();
            return stra.Result;
        }

        // LoadVatReturnFullData

        public ExperianLtd ParseExperianLtd(long nServiceLogID) {
            var stra = new ParseExperianLtd(nServiceLogID);
            stra.Execute();
            return stra.Result;
        } // ParseExperianLtd

        public ExperianLtd LoadExperianLtd(long nServiceLogID) {
            var stra = new LoadExperianLtd(null, nServiceLogID);
            stra.Execute();
            return stra.Result;
        } // LoadExperianLtd

        public ExperianLtd CheckLtdCompanyCache(int userId, string sCompanyRefNum) {
            var stra = new LoadExperianLtd(sCompanyRefNum, 0);
            stra.Execute();
            return stra.Result;
        } // CheckLtdCompanyCache

        public void EmailHmrcParsingErrors(int nCustomerID, int nCustomerMarketplaceID, SortedDictionary<string, string> oErrorsToEmail) {
            new EmailHmrcParsingErrors(nCustomerID, nCustomerMarketplaceID, oErrorsToEmail).Execute();
        } // EmailHmrcParsingErrors

        public CompanyData GetNonLimitedData(int underwriterId, string refNumber) {
            GetCompanyDataForCompanyScore strategyInstance = new GetCompanyDataForCompanyScore(refNumber);

            strategyInstance.Execute();
            return strategyInstance.Data;
        }

        public CompanyDataForCreditBureau GetCompanyDataForCreditBureau(int underwriterId, string refNumber) {
            GetCompanyDataForCreditBureau strategyInstance = new GetCompanyDataForCreditBureau(refNumber);

            strategyInstance.Execute();

            return new CompanyDataForCreditBureau {
                LastUpdate = strategyInstance.LastUpdate,
                Score = strategyInstance.Score,
                Errors = strategyInstance.Errors
            };
        }

        public WriteToLogPackage.OutputData ServiceLogWriter(WriteToLogPackage package) {
            var stra = new ServiceLogWriter(package);
            stra.Execute();
            return stra.Package.Out;
        }

        /// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
        public void AddPayment(int customerID, NL_Payments payment, int userID) {
			var stra = new AddPayment(customerID, payment, userID);
            stra.Context.CustomerID = customerID;
            stra.Context.UserID = userID;
            stra.Execute();
        }
		public void LinkPaymentToInvestor(int userID, int loanTransactionID, int loanID, int customerID, decimal amount, DateTime transactionDate) {
			LinkRepaymentToInvestor stra = new LinkRepaymentToInvestor(loanID, loanTransactionID, amount, transactionDate, userID);
			stra.Execute();
		}

        public List<NL_Loans> GetCustomerLoans(int customerID, int userID) {
            var stra = new GetCustomerLoans(customerID);
            stra.Context.CustomerID = customerID;
            stra.Context.UserID = userID;
            stra.Execute();
            return stra.Loans.ToList();
        }

	    /// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
	    public NL_Model GetLoanState(int customerID, long loanID, DateTime utcNow, int userID, bool getCalculatorState = true) {
            var stra = new GetLoanState(customerID, loanID, utcNow);
            stra.Context.CustomerID = customerID;
            stra.Context.UserID = userID;
            stra.Execute();
            return stra.Result;
        }

        public long GetLoanByOldID(int loanId, int customerID = 1, int userID = 1) {
			var stra = new GetLoanIDByOldID(loanId);
            stra.Context.CustomerID = customerID;
            stra.Context.UserID = userID;
            stra.Execute();
            return stra.LoanID;
        }

		public void AcceptRollover(int customerID, long loanID)  {
			var stra = new AcceptRollover(customerID, loanID);
			stra.Context.CustomerID = customerID;
			stra.Context.UserID = customerID;
			stra.Execute();
		}

    } // class EzServiceAccessorShort
} // namespace EzServiceShortcut
