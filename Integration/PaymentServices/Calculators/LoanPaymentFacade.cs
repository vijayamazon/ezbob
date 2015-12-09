﻿namespace PaymentServices.Calculators {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using EzServiceAccessor;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using log4net;
	using NHibernate.Linq;
	using StructureMap;

	public class LoanPaymentFacade {
		public LoanPaymentFacade() {
			loanTransactionMethodRepository = ObjectFactory.GetInstance<LoanTransactionMethodRepository>();
			_historyRepository = ObjectFactory.GetInstance<LoanHistoryRepository>();
			this.amountToChargeFrom = CurrentValues.Instance.AmountToChargeFrom;
		} // constructor

		public LoanPaymentFacade(
			ILoanHistoryRepository historyRepository,
			ILoanTransactionMethodRepository loanTransactionMethodRepository,
			int? amountToChargeFrom = null
		) {
			_historyRepository = historyRepository;
			this.loanTransactionMethodRepository = loanTransactionMethodRepository;
			this.amountToChargeFrom = amountToChargeFrom ?? CurrentValues.Instance.AmountToChargeFrom;
		} // constructor

		/// <summary>
		/// Заплатить за кредит. Платёж может быть произвольный. Early, On time, Late.
		/// Perform loan payment. Payment can be manual. Early, On time, Late.
		/// </summary>
		public virtual decimal PayLoan(
			Loan loan,
			string transId,
			decimal amount,
			string ip,            
			DateTime? term = null,
			string description = "payment from customer",
			bool interestOnly = false,
			string sManualPaymentMethod = null,       
            int userId = 1,
            NL_Payments nlPayment = null){

			var paymentTime = term ?? DateTime.UtcNow;

			var oldLoan = loan.Clone();

			const string Manual = "--- manual ---";
			string otherMethod = transId == Manual ? "Manual" : "Auto";

			var transactionItem = new PaypointTransaction {
				Amount = amount,
				Description = description,
				PostDate = paymentTime,
				Status = LoanTransactionStatus.Done,
				PaypointId = transId,
				IP = ip,
				LoanRepayment = oldLoan.Principal - loan.Principal,
				Interest = loan.InterestPaid - oldLoan.InterestPaid,
				InterestOnly = interestOnly,
				LoanTransactionMethod = loanTransactionMethodRepository.FindOrDefault(sManualPaymentMethod, otherMethod)
			};

			loan.AddTransaction(transactionItem);

		    if (nlPayment != null) {
                nlPayment.PaypointTransactions.Add(new NL_PaypointTransactions() {
                    TransactionTime = DateTime.UtcNow,
                    Amount = amount,
                    Notes = description,
                    PaypointUniqueID = transId,                    
                    IP = ip,
                    PaypointTransactionStatusID = (int)LoanTransactionStatus.Done,
                    PaypointCardID = loan.Customer.PayPointCards.FirstOrDefault(x => x.TransactionId == transId).Id
                });  
		    }

			List<InstallmentDelta> deltas = loan.Schedule.Select(inst => new InstallmentDelta(inst)).ToList();

			var calculator = new LoanRepaymentScheduleCalculator(loan, paymentTime, this.amountToChargeFrom);
			calculator.RecalculateSchedule();

			if (_historyRepository != null) {
				var historyRecord = new LoanHistory(loan, paymentTime);
				_historyRepository.SaveOrUpdate(historyRecord);
			} // if

			loan.UpdateStatus(paymentTime);

			if (loan.Customer != null)
				loan.Customer.UpdateCreditResultStatus();		    
			if (loan.Id > 0) {
				foreach (InstallmentDelta dlt in deltas) {
					dlt.SetEndValues();

					if (dlt.IsZero)
						continue;

					loan.ScheduleTransactions.Add(new LoanScheduleTransaction {
						Date = DateTime.UtcNow,
						FeesDelta = dlt.Fees.EndValue - dlt.Fees.StartValue,
						InterestDelta = dlt.Interest.EndValue - dlt.Interest.StartValue,
						Loan = loan,
						PrincipalDelta = dlt.Principal.EndValue - dlt.Principal.StartValue,
						Schedule = dlt.Installment,
						StatusAfter = dlt.Status.EndValue,
						StatusBefore = dlt.Status.StartValue,
						Transaction = transactionItem
					});
				} // for each delta
			} // if
            if (nlPayment != null) {
                ObjectFactory.GetInstance<IEzServiceAccessor>().AddPayment(loan.Customer.Id, nlPayment, userId);
            }

		    return amount;
		} // PayLoan

		/// <summary>
		/// Сколько будет сэкономлено денег, если пользователь погасит все свои кредиты.
		/// How much money will be saved, if customer pay all his loans now
		/// </summary>
		/// <param name="customer"></param>
		/// <param name="term"></param>
		/// <returns></returns>
		public decimal CalculateSavings(Customer customer, DateTime? term = null) {
			if (term == null)
				term = DateTime.UtcNow;

			decimal totalToPay = customer.TotalEarlyPayment();

			decimal oldInterest = customer.Loans.Sum(l => l.Interest);

			Customer clonedCustomer = new Customer();
			clonedCustomer.Loans.AddAll(customer.Loans.Select(l => l.Clone()).ToList());
			PayAllLoansForCustomer(clonedCustomer, totalToPay, "", term);

			decimal newInterest = clonedCustomer.Loans.Sum(l => l.Interest);

			decimal savedSetupFee = 0;
			string setupFeeCharge = new ConfigurationVariable(CurrentValues.Instance.SpreadSetupFeeCharge).Name;

			clonedCustomer.Loans.ForEach(l => l.Charges
				.Where(c => (c.ChargesType.Name == setupFeeCharge) && (c.State == "Active"))
				.ForEach(c => savedSetupFee += c.Amount - c.AmountPaid)
			);

			return oldInterest - newInterest + savedSetupFee;
		} // CalculateSavings

		/// <summary>
		/// Оплатить все кредиты клиента.
		/// </summary>
		public void PayAllLoansForCustomer(
			Customer customer,
			decimal amount,
			string transId,
			DateTime? term = null,
			string description = null,
			string sManualPaymentMethod = null
		) {
			var date = term ?? DateTime.Now;

			var loans = customer.Loans.Where(x => x.Status != LoanStatus.PaidOff || x.Id != 0).ToList();
            var nl_loans = ObjectFactory.GetInstance<IEzServiceAccessor>().GetCustomerLoans(customer.Id).Where( x=> x.LoanStatusID == 0 || x.LoanStatusID ==1).ToList();

			foreach (var loan in loans) {
				if (amount <= 0)
					break;
                var money = Math.Min(amount, loan.TotalEarlyPayment(term));
			    NL_Payments nlPayment = null;

			    Log.Info("DOR - "+ loan.Id + "         :" + loan.ToString());

			    var firstOrDefault = nl_loans.FirstOrDefault(x => x.OldLoanID == loan.Id);
			    
                if (firstOrDefault != null) {
                    nlPayment = new NL_Payments() {
                        Amount = money,
                        CreatedByUserID = 1,
                        CreationTime = DateTime.UtcNow,
                        LoanID = firstOrDefault.LoanID,
                        PaymentTime = DateTime.UtcNow,
                        Notes = "Pay Early Payment",
                        PaymentStatusID = (int)NLPaymentStatuses.Active,
                        PaymentMethodID = (int)NLLoanTransactionMethods.Manual
                    };    
			    }
			    try {
                    PayLoan(loan, transId, money, null, date, description, false, sManualPaymentMethod, 1, nlPayment);
                    amount = amount - money;
			    } catch (Exception ex ) {
                    Log.Error("DOR - " + ex + ex.StackTrace);
			        throw;
			    }

			} // for

		} // PayAllLoansForCustomer

		public void PayAllLateLoansForCustomer(
			Customer customer,
			decimal amount,
			string transId,
			DateTime? term = null,
			string description = null,
			string sManualPaymentMethod = null) {

			DateTime date = term ?? DateTime.Now;

			IEnumerable<Loan> loans = customer.ActiveLoans.Where(l => l.Status == LoanStatus.Late);
            var nl_loans = ObjectFactory.GetInstance<IEzServiceAccessor>().GetCustomerLoans(customer.Id).Where(x => x.LoanStatusID == 0 || x.LoanStatusID == 1);

			foreach (var loan in loans) {
				if (amount <= 0)
					break;

				LoanRepaymentScheduleCalculator c = new LoanRepaymentScheduleCalculator(loan, term, this.amountToChargeFrom);
				LoanScheduleItem state = c.GetState();

				decimal late = loan.Schedule.Where(s => s.Status == LoanScheduleStatus.Late).Sum(s => s.LoanRepayment) +
					state.Interest +
					state.Fees +
					state.LateCharges;

				decimal money = Math.Min(amount, late);
			    var firstOrDefault = nl_loans.FirstOrDefault(x => x.OldLoanID == loan.Id);
			    
                NL_Payments nlPayment = null;
                if (firstOrDefault != null) {
			        var nl_LoanId = firstOrDefault.LoanID;

			        nlPayment = new NL_Payments(){
			            Amount = money,
			            CreatedByUserID = 1,
			            CreationTime = DateTime.UtcNow,
			            LoanID = nl_LoanId,
			            PaymentTime = DateTime.UtcNow,
			            Notes = "Pay All Late Loans For Customer",
			            PaymentStatusID = (int)NLPaymentStatuses.Active,
			            PaymentMethodID = (int)NLLoanTransactionMethods.Manual                    
			        };
                }
			    PayLoan(loan, transId, money, null, date, description, false, sManualPaymentMethod, 1, nlPayment);

			    amount = amount - money;
			} // for
		} // PayAllLateLoansForCustomer

	    /// <summary>
	    /// Main method for making payments
	    /// </summary>
	    /// <param name="transId">pay point transaction id</param>
	    /// <param name="amount"></param>
	    /// <param name="ip"></param>
	    /// <param name="type"></param>
	    /// <param name="loanId"></param>
	    /// <param name="customer"></param>
	    /// <param name="date">payment date</param>
	    /// <param name="description"></param>
	    /// <param name="paymentType">If payment type is null - ordinary payment(reduces principal),
	    /// if nextInterest then it is for Interest Only loans, and reduces interest in the future.</param>
	    /// <param name="sManualPaymentMethod"></param>
	    /// <param name="userId"></param>
	    /// <returns></returns>
	    public PaymentResult MakePayment(            
			string transId,
			decimal amount,
			string ip,
			string type,
			int loanId,
			Customer customer,
			DateTime? date = null,
			string description = "payment from customer",
			string paymentType = null,
			string sManualPaymentMethod = null,
            int userId = 1) {

			Log.DebugFormat(
				"MakePayment transId: {0}, amount: {1}, ip: {2}, type: {3}, loanId: {4}, customer: {5}," +
				"date: {6}, description: {7}, paymentType: {8}, manualPaymentMethod: {9}",
				transId,
				amount,
				ip,
				type,
				loanId,
				customer.Id,
				date,
				description,
				paymentType,
				sManualPaymentMethod
			);

			decimal oldInterest;
			decimal newInterest;
			bool rolloverWasPaid = false;
			description = string.Format("{0} {1} {2}", description, type, paymentType);

			SortedSet<int> openLoansBefore = new SortedSet<int>(
				customer.Loans.Where(l => l.Status != LoanStatus.PaidOff).Select(l => l.Id)
			);

			if (type == "total") {
				oldInterest = customer.Loans.Sum(l => l.Interest);

				rolloverWasPaid = (
					from l in customer.Loans
					from s in l.Schedule
					from r in s.Rollovers
					where r.Status == RolloverStatus.New
					select r
				).Any();

				PayAllLoansForCustomer(
					customer,
					amount,
					transId,
					date,
					description,
					sManualPaymentMethod: sManualPaymentMethod
				);

				newInterest = customer.Loans.Sum(l => l.Interest);
			} else if (type == "totalLate") {
				rolloverWasPaid = (
					from l in customer.Loans
					from s in l.Schedule
					from r in s.Rollovers
					where r.Status == RolloverStatus.New
					select r
				).Any();

				oldInterest = customer.Loans.Sum(l => l.Interest);

				PayAllLateLoansForCustomer(
					customer,
					amount,
					transId,
					date,
					description,
					sManualPaymentMethod: sManualPaymentMethod
				);

				newInterest = customer.Loans.Sum(l => l.Interest);
			} else if (paymentType == "nextInterest") {
				oldInterest = 0;
				var loan = customer.GetLoan(loanId);
                var nl_LoanId = ObjectFactory.GetInstance<IEzServiceAccessor>().GetLoanByOldID(loanId);
                
                NL_Payments nlPayment = new NL_Payments()
                {
                    Amount = amount,
                    CreatedByUserID = userId,
                    CreationTime = DateTime.UtcNow,
                    LoanID = nl_LoanId,
                    PaymentTime = DateTime.UtcNow,
                    Notes = "Next Interest",
                    PaymentStatusID = (int)NLPaymentStatuses.Active,
                    PaymentMethodID = (int)NLLoanTransactionMethods.Manual
                };

                PayLoan(loan, transId, amount, ip, date, description, true, sManualPaymentMethod,1, nlPayment);
				newInterest = 0;
			} else {
				Loan loan = customer.GetLoan(loanId);

				oldInterest = loan.Interest;

				var rollover = (
					from s in loan.Schedule
					from r in s.Rollovers
					where r.Status == RolloverStatus.New
					select r
				).FirstOrDefault();

                var nl_LoanId = ObjectFactory.GetInstance<IEzServiceAccessor>().GetLoanByOldID(loanId);

                NL_Payments nlPayment = new NL_Payments()
                {
                    Amount = amount,
                    CreatedByUserID = userId,
                    CreationTime = DateTime.UtcNow,
                    LoanID = nl_LoanId,
                    PaymentTime = DateTime.UtcNow,
                    Notes = "Interest",
                    PaymentStatusID = (int)NLPaymentStatuses.Active,
                    PaymentMethodID = (int)NLLoanTransactionMethods.Manual
                };

                PayLoan(loan, transId, amount, ip, date, description, false, sManualPaymentMethod, 1, nlPayment);
			   
				newInterest = loan.Interest;

				rolloverWasPaid = rollover != null && rollover.Status == RolloverStatus.Paid;
			} // if

			SortedSet<int> openLoansAfter = new SortedSet<int>(
				customer.Loans.Where(l => l.Status != LoanStatus.PaidOff).Select(l => l.Id)
			);

			var loansClosedNow = new SortedSet<int>(openLoansBefore.Except(openLoansAfter));

			decimal savedSetupFee = 0;
			string setupFeeCharge = new ConfigurationVariable(CurrentValues.Instance.SpreadSetupFeeCharge).Name;

			customer.Loans.Where(l => loansClosedNow.Contains(l.Id)).ForEach(l => l.Charges
				.Where(c => (c.ChargesType.Name == setupFeeCharge) && (c.State == "Active"))
				.ForEach(c => savedSetupFee += c.Amount - c.AmountPaid)
			);

			var savedPounds = oldInterest - newInterest + savedSetupFee;

			var transactionRefNumbers =
				from l in customer.Loans
				from t in l.TransactionsWithPaypoint
				where t.Id == 0
				select t.RefNumber;

			var payFastModel = new PaymentResult {
				PaymentAmount = amount,
				Saved = oldInterest + savedSetupFee > 0 ? Math.Round(savedPounds / (oldInterest + savedSetupFee) * 100) : 0,
				SavedPounds = savedPounds,
				TransactionRefNumbers = transactionRefNumbers.ToList(),
				RolloverWasPaid = rolloverWasPaid
			};

			customer.TotalPrincipalRepaid = customer.Loans
				.SelectMany(l => l.Transactions)
				.OfType<PaypointTransaction>()
				.Where(l => l.Status != LoanTransactionStatus.Error)
				.Sum(l => l.LoanRepayment);

			return payFastModel;
		} // MakePayment

		public LoanScheduleItem GetStateAt(Loan loan, DateTime dateTime) {
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, dateTime, this.amountToChargeFrom);
			return payEarlyCalc.GetState();
		} // GetStateAt

		public void Recalculate(Loan loan, DateTime dateTime) {
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, dateTime, this.amountToChargeFrom);
			payEarlyCalc.GetState();
		} // Recalculate

		private readonly ILoanHistoryRepository _historyRepository;
		private static readonly ILog Log = LogManager.GetLogger(typeof(LoanPaymentFacade));
		private readonly ILoanTransactionMethodRepository loanTransactionMethodRepository;
		private readonly int amountToChargeFrom;
	} // class LoanPaymentFacade
} // namespace
