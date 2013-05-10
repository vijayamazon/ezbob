using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;

namespace PaymentServices.Calculators
{
    public class LoanPaymentFacade
    {
        private readonly ILoanHistoryRepository _historyRepository;

        public LoanPaymentFacade()
        {
        }

        public LoanPaymentFacade(ILoanHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
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

            var payEarlyCalc = new PayEarlyCalculator2(loan, paymentTime);
            payEarlyCalc.PayEarly(amount);

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

                var c = new PayEarlyCalculator2(loan, term);
                var state = c.GetState();
                var late = loan.Schedule.Where(s => s.Status == LoanScheduleStatus.Late).Sum(s => s.LoanRepayment) +
                           state.Interest + state.Fees + state.LateCharges;
                var money = Math.Min(amount, late);
                PayLoan(loan, transId, money, null, date);
                amount = amount - money;
            }
        }

        public PayFastResult MakePayment(string transId, decimal amount, string ip, string type, int loanId, Customer customer, DateTime? date = null, string description = "payment from customer", string paymentType = null)
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

            var payFastModel = new PayFastResult
                {
                    PaymentAmount = amount,
                    Saved = oldInterest > 0 ? Math.Round(savedPounds / oldInterest * 100) : 0,
                    SavedPounds = savedPounds,
                    TransactionRefNumbers = transactionRefNumbers.ToList(),
                    RolloverWasPaid = rolloverWasPaid
                };

            return payFastModel;
        }

        public void PayInstallment(LoanScheduleItem installment, decimal amount, string transId, string ip, DateTime date)
        {
            var oldInstallment = installment.Clone();
            var loan = installment.Loan;

            var transactionItem = new PaypointTransaction()
            {
                Amount = amount,
                Description = "payment from customer",
                PostDate = date,
                Status = LoanTransactionStatus.Done,
                PaypointId = transId,
                IP = ip,
            };

            loan.AddTransaction(transactionItem);

            var payEarlyCalc = new PayEarlyCalculator2(loan, date);
            payEarlyCalc.PayEarly(amount);

            loan.UpdateStatus(date);
            loan.UpdateBalance();

            if (_historyRepository != null)
            {
                var historyRecord = new LoanHistory(loan, oldInstallment, date);
                _historyRepository.Save(historyRecord);
            }
        }

        public LoanScheduleItem GetStateAt(Loan loan, DateTime dateTime)
        {
            var payEarlyCalc = new PayEarlyCalculator2(loan, dateTime);
            return payEarlyCalc.GetState();
        }

        public void Recalculate(Loan loan, DateTime dateTime)
        {
            var payEarlyCalc = new PayEarlyCalculator2(loan, dateTime);
            payEarlyCalc.GetState();
        }
    }
}