namespace PaymentServices.Calculators {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using StructureMap;
	using log4net;

	public class LoanPaymentFacade {
		private readonly ILoanHistoryRepository _historyRepository;
		private static readonly ILog Log = LogManager.GetLogger(typeof(LoanPaymentFacade));
		private readonly ILoanTransactionMethodRepository loanTransactionMethodRepository;
		private readonly int amountToChargeFrom;
		public LoanPaymentFacade() {
			loanTransactionMethodRepository = ObjectFactory.GetInstance<LoanTransactionMethodRepository>();
			_historyRepository = ObjectFactory.GetInstance<LoanHistoryRepository>();
			this.amountToChargeFrom = CurrentValues.Instance.AmountToChargeFrom;
		}

		public LoanPaymentFacade(ILoanHistoryRepository historyRepository, ILoanTransactionMethodRepository loanTransactionMethodRepository, int? amountToChargeFrom = null) {
			_historyRepository = historyRepository;
			this.loanTransactionMethodRepository = loanTransactionMethodRepository;
			this.amountToChargeFrom = amountToChargeFrom ?? CurrentValues.Instance.AmountToChargeFrom;
		}

		/// <summary>
		/// Заплатить за кредит. Платёж может быть произвольный. Early, On time, Late.
		/// Perform loan payment. Payment can be manual. Early, On time, Late.
		/// </summary>
		public virtual decimal PayLoan(Loan loan, string transId, decimal amount, string ip, DateTime? term = null, string description = "payment from customer", bool interestOnly = false, string sManualPaymentMethod = null) {
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
            
            //TODO add payment to new payment table
		    Log.InfoFormat("Add payment of {0} to loan {1}", amount, loan.Id);
			// elina: TODO : get distribution of amount to loan schedules from new loan calculator ALoanCalculator.AssignPaymentToLoan (princpal, interest) and fees and save to DB (design doc: "PayPointCharger" page 13-15; “Manual payment – by Customer” page 19; )

			List<InstallmentDelta> deltas = loan.Schedule.Select(inst => new InstallmentDelta(inst)).ToList();

			var calculator = new LoanRepaymentScheduleCalculator(loan, paymentTime, this.amountToChargeFrom);
			calculator.RecalculateSchedule();

			if (_historyRepository != null) {
				var historyRecord = new LoanHistory(loan, paymentTime);
				_historyRepository.SaveOrUpdate(historyRecord);
			}

			loan.UpdateStatus(paymentTime);

			if (loan.Customer != null) {
				loan.Customer.UpdateCreditResultStatus();
			}

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

			return amount;
		}

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
			var totalToPay = customer.TotalEarlyPayment();

			var oldInterest = customer.Loans.Sum(l => l.Interest);

			var clonedCustomer = new Customer();
			clonedCustomer.Loans.AddAll(customer.Loans.Select(l => l.Clone()).ToList());
			PayAllLoansForCustomer(clonedCustomer, totalToPay, "", term);

			var newInterest = clonedCustomer.Loans.Sum(l => l.Interest);

			return oldInterest - newInterest;
		}

		/// <summary>
		/// Оплатить все кредиты клиента.
		/// </summary>
		public void PayAllLoansForCustomer(Customer customer, decimal amount, string transId, DateTime? term = null, string description = null, string sManualPaymentMethod = null) {
			var date = term ?? DateTime.Now;
			var loans = customer.ActiveLoans;
			foreach (var loan in loans) {
				if (amount <= 0)
					break;
				var money = Math.Min(amount, loan.TotalEarlyPayment(term));
				PayLoan(loan, transId, money, null, date, description, sManualPaymentMethod: sManualPaymentMethod);
				amount = amount - money;
			}
		}

		public void PayAllLateLoansForCustomer(Customer customer, decimal amount, string transId, DateTime? term = null, string description = null, string sManualPaymentMethod = null) {
			var date = term ?? DateTime.Now;
			var loans = customer.ActiveLoans.Where(l => l.Status == LoanStatus.Late);
			foreach (var loan in loans) {
				if (amount <= 0)
					break;

				var c = new LoanRepaymentScheduleCalculator(loan, term, this.amountToChargeFrom);
				var state = c.GetState();
				var late = loan.Schedule.Where(s => s.Status == LoanScheduleStatus.Late).Sum(s => s.LoanRepayment) +
						   state.Interest + state.Fees + state.LateCharges;
				var money = Math.Min(amount, late);
				PayLoan(loan, transId, money, null, date, description, sManualPaymentMethod: sManualPaymentMethod);
				amount = amount - money;
			}
		}
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
		/// <param name="paymentType">If payment type is null - ordinary payment(reduces principal), if nextInterest then it is
		/// for Interest Only loans, and reduces interest in the future.</param>
		/// <param name="sManualPaymentMethod"></param>
		/// <returns></returns>
		public PaymentResult MakePayment(string transId, decimal amount, string ip, string type, int loanId, Customer customer, DateTime? date = null, string description = "payment from customer", string paymentType = null, string sManualPaymentMethod = null) {
			Log.DebugFormat("MakePayment transId: {0}, amount: {1}, ip: {2}, type: {3}, loanId: {4}, customer: {5}, date: {6}, description: {7}, paymentType: {8}, manualPaymentMethod: {9}",
				transId, amount, ip, type, loanId, customer.Id, date, description, paymentType, sManualPaymentMethod);

			decimal oldInterest;
			decimal newInterest;
			bool rolloverWasPaid = false;
			description = string.Format("{0} {1} {2}", description, type, paymentType);

			if (type == "total") {
				oldInterest = customer.Loans.Sum(l => l.Interest);
				rolloverWasPaid =
					(from l in customer.Loans
					 from s in l.Schedule
					 from r in s.Rollovers
					 where r.Status == RolloverStatus.New
					 select r).Any();

				PayAllLoansForCustomer(customer, amount, transId, date, description, sManualPaymentMethod: sManualPaymentMethod);
				newInterest = customer.Loans.Sum(l => l.Interest);
			}
			else if (type == "totalLate") {
				rolloverWasPaid =
					(from l in customer.Loans
					 from s in l.Schedule
					 from r in s.Rollovers
					 where r.Status == RolloverStatus.New
					 select r).Any();
				oldInterest = customer.Loans.Sum(l => l.Interest);
				PayAllLateLoansForCustomer(customer, amount, transId, date, description, sManualPaymentMethod: sManualPaymentMethod);
				newInterest = customer.Loans.Sum(l => l.Interest);
			}
			else if (paymentType == "nextInterest") {
				oldInterest = 0;
				var loan = customer.GetLoan(loanId);
				PayLoan(loan, transId, amount, ip, date, description, true, sManualPaymentMethod: sManualPaymentMethod);
				newInterest = 0;
			}
			else {
				var loan = customer.GetLoan(loanId);
				oldInterest = loan.Interest;
				var rollover = (from s in loan.Schedule
								from r in s.Rollovers
								where r.Status == RolloverStatus.New
								select r).FirstOrDefault();
				PayLoan(loan, transId, amount, ip, date, description, sManualPaymentMethod: sManualPaymentMethod);
				newInterest = loan.Interest;

				rolloverWasPaid = rollover != null && rollover.Status == RolloverStatus.Paid;
			}

			var savedPounds = oldInterest - newInterest;

			var transactionRefNumbers = from l in customer.Loans
										from t in l.TransactionsWithPaypoint
										where t.Id == 0
										select t.RefNumber;

			var payFastModel = new PaymentResult {
				PaymentAmount = amount,
				Saved = oldInterest > 0 ? Math.Round(savedPounds / oldInterest * 100) : 0,
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
		}

		public LoanScheduleItem GetStateAt(Loan loan, DateTime dateTime) {
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, dateTime, this.amountToChargeFrom);
			return payEarlyCalc.GetState();
		}

		public void Recalculate(Loan loan, DateTime dateTime) {
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, dateTime, this.amountToChargeFrom);
			payEarlyCalc.GetState();
		}
	}
}
