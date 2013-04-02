using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;

namespace PaymentServices.Calculators
{
    public class LoanScheduleCalculator
    {
        private decimal _setUpFee = .008M;
        private decimal _setUpfFeeMin = 30;
        private int _term = 3;
        private decimal _interest = 0.06M;

        public decimal SetUpFee
        {
            get { return _setUpFee; }
            set { _setUpFee = value; }
        }

        public decimal SetUpFeeMin
        {
            get { return _setUpfFeeMin; }
            set { _setUpfFeeMin = value; }
        }

        public int Term
        {
            get { return Math.Max(_term, 1) ; }
            set { _term = value; }
        }

        public decimal Interest
        {
            get { return _interest; }
            set { _interest = value; }
        }

        /// <summary>
        /// Calculates loan schedule
        /// </summary>
        /// <param name="total">Total amount, that is taken</param>
        /// <param name="loan">Loan object, or null. If loan is present, schedule is added to it</param>
        /// <param name="startDate">Starting day of loan. First payment is mounth later</param>
        /// <returns></returns>
        public IList<LoanScheduleItem> Calculate(decimal total, Loan loan = null, DateTime? startDate = null)
        {
            var schedule = loan == null ? new List<LoanScheduleItem>(Term) : loan.Schedule;

            if (!startDate.HasValue)
            {
                startDate = DateTime.UtcNow;
            }

            var loanType = loan == null ? new StandardLoanType() : loan.LoanType;
            var balances = loanType.GetBalances(total, Term).ToArray();

            var balance = total;
            var repayment = balance;

            for (int m = 0; m < Term; m++)
            {
                LoanScheduleItem item = null;

                if (schedule.Count != Term)
                {
                    item = new LoanScheduleItem {Status = LoanScheduleStatus.StillToPay};
                    schedule.Add(item);
                }
                else
                {
                    item = schedule[m];
                }

                var interest = Math.Round(balance * Interest, 2);

                repayment = balance - balances[m];
                item.BalanceBeforeRepayment = balance;
                balance = balances[m];


                item.Loan = loan;
                item.Date = startDate.Value.AddMonths(m + 1);
                item.Balance = balance;
                item.Interest = interest;
                item.InterestRate = Interest;
                item.AmountDue = repayment + interest;
                item.LoanRepayment = repayment;
            }

            if (loan != null)
            {
                loan.Interest = schedule.Sum(x => x.Interest);
                loan.LoanAmount = total;
                loan.Principal = total;
                loan.Balance = loan.LoanAmount + loan.Interest;
                loan.InterestRate = Interest;
                loan.Date = startDate.Value;
                loan.UpdateNexPayment();
            }

            return schedule;
        }
    }
}