﻿using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;

namespace PaymentServices.Calculators
{
    public class LoanPaymentFacade
    {
        private readonly ILoanHistoryRepository _historyRepository;
        private readonly IConfigurationVariablesRepository _configVariables;

        public LoanPaymentFacade()
        {
        }

        public LoanPaymentFacade(ILoanHistoryRepository historyRepository, IConfigurationVariablesRepository configVariables)
        {
            _historyRepository = historyRepository;
            _configVariables = configVariables;
        }


        /// <summary>
        /// Заплатить за кредит. Платёж может быть произвольный. Early, Ontime, Late.
        /// </summary>
        /// <param name="loan"></param>
        /// <param name="transId"></param>
        /// <param name="amount"></param>
        /// <param name="ip"></param>
        /// <param name="term"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public virtual decimal PayLoan(Loan loan, string transId, decimal amount, string ip, DateTime? term = null, string description = "payment from customer", bool interestOnly = false)
        {
            var paymentTime = term ?? DateTime.UtcNow;

            var oldLoan = loan.Clone();

            var transactionItem = new PaypointTransaction()
            {
                Amount = amount,
                Description = description,
                PostDate = paymentTime,
                Status = LoanTransactionStatus.Done,
                PaypointId = transId,
                IP = ip,
                LoanRepayment = oldLoan.Principal - loan.Principal,
                Interest = loan.InterestPaid - oldLoan.InterestPaid,
                InterestOnly = interestOnly
            };
            
            loan.AddTransaction(transactionItem);

	        List<InstallmentDelta> deltas = loan.Schedule.Select(inst => new InstallmentDelta(inst)).ToList();

            var calculator = new LoanRepaymentScheduleCalculator(loan, paymentTime, _configVariables);
            calculator.RecalculateSchedule();

            if (_historyRepository != null)
            {
                var historyRecord = new LoanHistory(loan, paymentTime);
                _historyRepository.Save(historyRecord);
            }

            loan.UpdateStatus(paymentTime);

            if (loan.Customer != null)
            {
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
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public decimal CalculateSavings(Customer customer, DateTime? term = null)
        {
            if (term == null) term = DateTime.UtcNow;
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
        /// <param name="customer"></param>
        /// <param name="amount"></param>
        /// <param name="transId"></param>
        /// <param name="term"></param>
        public void PayAllLoansForCustomer(Customer customer, decimal amount, string transId, DateTime? term = null)
        {
            var date = term ?? DateTime.Now;
            var loans = customer.ActiveLoans;
            foreach (var loan in loans)
            {
                if (amount <= 0) break;
                var money = Math.Min(amount, loan.TotalEarlyPayment(term));
                PayLoan(loan, transId, money, null, date);
                amount = amount - money;
            }
        }

        public void PayAllLateLoansForCustomer(Customer customer, decimal amount, string transId, DateTime? term = null)
        {
            var date = term ?? DateTime.Now;
            var loans = customer.ActiveLoans.Where(l => l.Status == LoanStatus.Late);
            foreach (var loan in loans)
            {
                if (amount <= 0) break;

                var c = new LoanRepaymentScheduleCalculator(loan, term, _configVariables);
                var state = c.GetState();
                var late = loan.Schedule.Where(s => s.Status == LoanScheduleStatus.Late).Sum(s => s.LoanRepayment) +
                           state.Interest + state.Fees + state.LateCharges;
                var money = Math.Min(amount, late);
                PayLoan(loan, transId, money, null, date);
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
        /// <returns></returns>
        public PaymentResult MakePayment(string transId, decimal amount, string ip, string type, int loanId, Customer customer, DateTime? date = null, string description = "payment from customer", string paymentType = null)
        {
            decimal oldInterest;
            decimal newInterest;
            bool rolloverWasPaid = false;

            if (type == "total")
            {
                oldInterest = customer.Loans.Sum(l => l.Interest);
                rolloverWasPaid =
                    (from l in customer.Loans
                     from s in l.Schedule
                     from r in s.Rollovers
                     where r.Status == RolloverStatus.New
                     select r).Any();

                PayAllLoansForCustomer(customer, amount, transId, date);
                newInterest = customer.Loans.Sum(l => l.Interest);
            }
            else if (type == "totalLate")
            {
                rolloverWasPaid =
                    (from l in customer.Loans
                     from s in l.Schedule
                     from r in s.Rollovers
                     where r.Status == RolloverStatus.New
                     select r).Any();
                oldInterest = customer.Loans.Sum(l => l.Interest);
                PayAllLateLoansForCustomer(customer, amount, transId, date);
                newInterest = customer.Loans.Sum(l => l.Interest);
            }
            else if (paymentType == "nextInterest")
            {
                oldInterest = 0;
                var loan = customer.GetLoan(loanId);
                PayLoan(loan, transId, amount, ip, date, description, true);
                newInterest = 0;
            }
            else
            {
                var loan = customer.GetLoan(loanId);
                oldInterest = loan.Interest;
                var rollover = (from s in loan.Schedule
                                from r in s.Rollovers
                                where r.Status == RolloverStatus.New
                                select r).FirstOrDefault();
                PayLoan(loan, transId, amount, ip, date, description);
                newInterest = loan.Interest;

                rolloverWasPaid = rollover != null && rollover.Status == RolloverStatus.Paid;
            }

            var savedPounds = oldInterest - newInterest;

            var transactionRefNumbers = from l in customer.Loans
                                        from t in l.TransactionsWithPaypoint
                                        where t.Id == 0
                                        select t.RefNumber;

            var payFastModel = new PaymentResult
                {
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

        public LoanScheduleItem GetStateAt(Loan loan, DateTime dateTime)
        {
            var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, dateTime, _configVariables);
            return payEarlyCalc.GetState();
        }

        public void Recalculate(Loan loan, DateTime dateTime)
        {
            var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, dateTime, _configVariables);
            payEarlyCalc.GetState();
        }
    }
}