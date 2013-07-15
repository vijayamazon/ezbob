using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.Web.Areas.Customer.Models;

namespace PaymentServices.Calculators
{
    public class PayEarlyCalculator0 : IPayEarlyCalculator
    {
        /// <summary>
        /// Заем, для которого будет осуществляться перерасчет
        /// </summary>
        private readonly Loan _loan;

        /// <summary>
        /// Дата, для которой будет осуществляться перерасчет
        /// </summary>
        private readonly DateTime _term;
        
        /// <summary>
        /// Первый запланированный неоплаченный платеж по кредиту
        /// </summary>
        private readonly LoanScheduleItem _firstScheduledItem;
        
        /// <summary>
        /// Дата последнего полностью оплаченного платежа
        /// </summary>
        private readonly DateTime _lastPaymentDate;
        
        /// <summary>
        /// Последний полностью оплаченный платеж
        /// </summary>        
        private readonly LoanScheduleItem _lastPayment;
        
        /// <summary>
        /// Количестов дней, которое прошло с момента последней полной выплаты по кредту
        /// </summary>        
        private readonly int _daysFromLastPayment;
        
        /// <summary>
        /// Баланс кредита, на момент расчета
        /// </summary>
        private readonly decimal _originalBalance;
        
        ///<summary>
        /// Доход банка, который он должне получить, за пероди от начала месяца до даты расчета
        /// </summary>       
        private readonly decimal _periodInterest;
               
        /// <summary>
        /// Количество дней в месяце, для которого производится расчет
        /// </summary>
        private readonly int _daysInMonth;

        public PayEarlyCalculator0(Loan loan, DateTime? term)
        {
            _loan = loan;
            _term = term == null ? DateTime.Now : term.Value;

            _firstScheduledItem = _loan.Schedule
                .LastOrDefault(x =>
                                    x.Status == LoanScheduleStatus.PaidEarly
                                    && (x.Date >= _term)
                                    );

            if(_firstScheduledItem == null)
            {
                _firstScheduledItem = _loan.Schedule.FirstOrDefault(s => (s.Status == LoanScheduleStatus.StillToPay) || (s.Status == LoanScheduleStatus.Late));
            }


            _lastPaymentDate = _loan.Date;
            _lastPayment = _loan.Schedule.LastOrDefault(x => 
                                                                x.Status == LoanScheduleStatus.PaidOnTime || 
                                                                x.Status == LoanScheduleStatus.PaidEarly ||
                                                                x.Status == LoanScheduleStatus.Paid );
            
            if (_lastPayment != null)
            {
                _lastPaymentDate = _lastPayment.Date;
            }

            _daysFromLastPayment = (int)Math.Floor(Math.Max(0, _term.Subtract(_lastPaymentDate.Date).TotalDays));
            _daysInMonth = DateTime.DaysInMonth(_lastPaymentDate.Date.Year, _lastPaymentDate.Date.Month);

            if (_firstScheduledItem == null) return;

            _originalBalance = _firstScheduledItem.Balance +_firstScheduledItem.LoanRepayment;
            
            _periodInterest = Math.Round(_originalBalance * _loan.InterestRate * _daysFromLastPayment / _daysInMonth);
        }

        public decimal PayEarly(decimal amount)
        {
            var schedule = _loan.Schedule;

            var oldTotalInterst = schedule.Sum(x => x.Interest);

            if (_firstScheduledItem == null)
            {
                // you cannot pay this loan
                return 0;
            }

            if (_daysFromLastPayment < 0)
            {
                //something worng with dates
                return 0;
            }

            if (TotalEarlyPayment() < amount)
            {
                //amount is too big
                return 0;
            }

            var repayment = Math.Max(amount - _periodInterest, 0);

            _loan.Principal = _loan.Principal - repayment;

            var restInterst = Math.Round((_originalBalance - repayment) * _loan.InterestRate * (_daysInMonth - _daysFromLastPayment) / _daysInMonth);

            var capitalizedInterest = 0M;

            _firstScheduledItem.Interest = _periodInterest + restInterst;
            _firstScheduledItem.InterestPaid = _firstScheduledItem.InterestPaid + _periodInterest;
            _firstScheduledItem.LoanRepayment = Math.Max(_firstScheduledItem.LoanRepayment - repayment, 0); 
            
            
            if (_firstScheduledItem.LoanRepayment > 0)
            {
                _firstScheduledItem.AmountDue = _firstScheduledItem.LoanRepayment + restInterst;
                _firstScheduledItem.Balance = _firstScheduledItem.Balance;
            }
            else
            {
                //customer pays more than he should in this period
                _firstScheduledItem.Balance = _originalBalance - repayment;
                _firstScheduledItem.AmountDue = 0;
                _firstScheduledItem.RepaymentAmount += repayment;
            }

            //_firstScheduledItem.Balance += _firstScheduledItem.Balance == 0 ? 0 : restInterst;
            if (_firstScheduledItem.Balance > 0 && _firstScheduledItem.AmountDue == 0)
            {
                capitalizedInterest += restInterst;
            } else
            {
                //_firstScheduledItem.AmountDue += restInterst;
            }

            if(_firstScheduledItem.AmountDue == 0)
            {
                _firstScheduledItem.UpdateStatus(_term);
            }

            var balance = _firstScheduledItem.Balance + capitalizedInterest;

            foreach (var next in schedule.SkipWhile(x => x.Date <= _firstScheduledItem.Date))
            {
                next.Interest = Math.Round(balance * _loan.InterestRate);
                next.LoanRepayment = Math.Max(0, balance - next.Balance);
                next.AmountDue = next.LoanRepayment + next.Interest;
                next.Balance = balance - next.LoanRepayment;
                balance = next.Balance;
                
                if (next.AmountDue == 0)
                {
                    next.UpdateStatus(_term);
                }
            }

            var newInterest = schedule.Sum(x => x.Interest);

            decimal savings = 0;
            if (oldTotalInterst > 0)
            {
                savings = Math.Round(100 * (oldTotalInterst - newInterest) / oldTotalInterst) / 100;
            }

            _loan.RepaymentsNum += 1;
            _loan.Repayments += amount;

            _loan.UpdateBalance();
            _loan.UpdateNexPayment();

            return savings;
        }

        public LoanScheduleItem GetState()
        {
            throw new NotImplementedException();
        }

        public decimal NextEarlyPayment()
        {

            if (_firstScheduledItem == null)
            {
                // you cannot pay this loan
                return 0;
            }

            if (_daysFromLastPayment < 0)
            {
                //something wrong with dates
                return 0;
            }

            if (_firstScheduledItem.LoanRepayment == 0)
            {
                var item = _loan.Schedule.FirstOrDefault(s => s.Status == LoanScheduleStatus.StillToPay && s.LoanRepayment > 0);
                if(item != null) return item.LoanRepayment;
                return 0;
            }

            return _firstScheduledItem.LoanRepayment + _periodInterest;
        }

        public decimal TotalEarlyPayment()
        {
            if (_firstScheduledItem == null)
            {
                // you cannot pay this loan
                return 0;
            }

            if (_daysFromLastPayment < 0)
            {
                //something wrong with dates
                return 0;
            }

            return _originalBalance + _periodInterest;
        }
    }

    public static class PayEarlyExtensions
    {
        public static decimal NextEarlyPayment(this Loan loan, DateTime? term = null)
        {
            var calc = new LoanRepaymentScheduleCalculator(loan, term);
            return calc.NextEarlyPayment();
        }

        public static decimal TotalEarlyPayment(this Loan loan, DateTime? term = null)
        {
            var calc = new LoanRepaymentScheduleCalculator(loan, term);
            return calc.TotalEarlyPayment();
        }

        public static decimal TotalEarlyPayment(this Customer customer, DateTime? term = null)
        {
            return customer.Loans.Sum(l => l.TotalEarlyPayment(term));
        }
    }
}