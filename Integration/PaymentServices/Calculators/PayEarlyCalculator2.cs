using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Web.Areas.Customer.Models;

namespace PaymentServices.Calculators
{
    public class PayEarlyCalculator2 : IPayEarlyCalculator
    {
        private readonly Loan _loan;
        private DateTime _term;
        private readonly IList<LoanScheduleItem> _schedule;
        private readonly List<PaypointTransaction> _payments;
        private readonly List<LoanCharge> _charges;
        private decimal _interestRate;


        //state variables

        //деньги, которые реально находятся у клиента на руках. баланс кредита без процентов и fee
        private decimal _principal;

        //ожидаемый баланс.
        private decimal _expectedPrincipal;

        //Ожидаемый баланс. При каждом installment увеличивается, в независимости от того в будущем этот installment, или нет
        private decimal _totalPrincipalToPay;

        //Сумма реально оплаченного principal
        private decimal _paidPrincipal;

        //дата последнего действия. нужна для расчета процентов
        private DateTime _lastActionDate;
        private DateTime? _lastPaymentDate;

        //список обработанных installment'ов
        private List<LoanScheduleItem> _processed = new List<LoanScheduleItem>();

        //последовательность событий, относящихся к кредиту
        private List<PayEarlyCalculator2Event> _events = new List<PayEarlyCalculator2Event>();

        //Доход банка за все время заема
        private decimal _totalInterestToPay = 0;

        //Сколько всего клиент заплатил процентов
        private decimal _paidInterest = 0;


        //Штрафы, которые надо было заплатить
        private decimal _totalFeesToPay = 0;

        //Штрафы, уплаченные клиентом
        private decimal _paidFees = 0;

        private decimal _paidRollOvers = 0;
        private decimal _totalRollOversToPay = 0;

        private PayEarlyCalculator2Event _eventDayStart;
        private PayEarlyCalculator2Event _eventDayEnd;

        //количество дней в месяце, в текущем периоде
        private int _daysInMonth;

        //текущий roll over, который должен быть оплачен при платеже
        private List<PaymentRollover> _currentRollover = new List<PaymentRollover>();
        private List<PaymentRollover> _rollovers;

        //было ли расписание сдвинуто
        private bool _shifted = false;

        private List<LoanScheduleItem> _rescentLate = new List<LoanScheduleItem>(); 

        private List<LoanCharge> _chargesToPay = new List<LoanCharge>();

        private DateTime _prevInstallmentDate = DateTime.MinValue;

        public PayEarlyCalculator2(Loan loan, DateTime? term)
        {
            _loan = loan;
            _schedule = loan.Schedule;
            _payments = loan.TransactionsWithPaypointSuccesefull;
            _charges = loan.Charges.OrderBy(x => x.Date).ToList();

            _term = (term ?? DateTime.Now).Date;

            _eventDayStart = new PayEarlyCalculator2Event(_term);
            _eventDayEnd = new PayEarlyCalculator2Event(_term.AddHours(23).AddMinutes(59).AddSeconds(59));

            Init();
        }

        private void Init()
        {
            _shifted = false;

            _chargesToPay.Clear();
            _rescentLate.Clear();

            _interestRate = _loan.InterestRate;

            _principal = _loan.LoanAmount;
            _expectedPrincipal = _loan.LoanAmount;

            _lastActionDate = _loan.Date.Date;
            _prevInstallmentDate = _loan.Date.Date;

            _daysInMonth = DaysInMonth(_loan.Date);

            _totalPrincipalToPay = 0;
            _paidFees = 0;
            _totalFeesToPay = 0;
            _paidInterest = 0;
            _paidPrincipal = 0;
            _totalInterestToPay = 0;
            _paidRollOvers = 0;
            _totalRollOversToPay = 0;
            _currentRollover.Clear();
            _processed.Clear();

            _rollovers = _schedule.SelectMany(s => s.Rollovers).Where(s => s.Status != RolloverStatus.Removed).ToList();

            //Ajdust interest rate
            if (_schedule.All(i => i.InterestRate == 0))
            {
                foreach (var installment in _schedule)
                {
                    installment.InterestRate = _loan.InterestRate;
                }
            }

            var lateChargeEvents = _charges.Select(p => new PayEarlyCalculator2Event(new DateTime(p.Date.Year, p.Date.Month, p.Date.Day, 23, 59, 57), p)).ToList();
            var paymentEvents = _payments.Select(p => new PayEarlyCalculator2Event(new DateTime(p.PostDate.Year, p.PostDate.Month, p.PostDate.Day, 23, 59, 58), p)).ToList();
            var installmentEvents = _schedule.Select(i => new PayEarlyCalculator2Event(new DateTime(i.Date.Year, i.Date.Month, i.Date.Day, 23, 59, 59), i));

            var rollOverEvents = _rollovers.Select(p => new PayEarlyCalculator2Event(p.Created.Date, p)).ToList();

            _events = installmentEvents
                                 .Union(paymentEvents)
                                 .Union(lateChargeEvents)
                                 .Union(rollOverEvents)
                                 .Union(new[] { _eventDayStart, _eventDayEnd })
                                 .OrderBy(e => e.Date)
                                 .ThenBy(e => e.Priority)
                                 .ToList();
        }

        public decimal NextEarlyPayment()
        {
            decimal amount = 0;
            _eventDayEnd.Action = () =>
            {
                if (_loan.Status == LoanStatus.PaidOff) return;

                var principal = _loan.LoanAmount - _paidPrincipal;

                //если пользователь пропустил платеж
                if (principal > _expectedPrincipal)
                {
                    amount = Math.Round(principal - _expectedPrincipal + InterestToPay, 2);
                    return;
                }

                var next = _schedule.FirstOrDefault(s => s.Date >= _lastActionDate && s.LoanRepayment > 0) ??
                           _schedule.Last();
                var expectedPrincipal = next.Balance;
                amount = Math.Round(Math.Max(0, principal - expectedPrincipal + InterestToPay + FeesToPay), 2);
            };
            Recalculate();
            _eventDayEnd.Action = null;
            return amount;
        }

        public decimal InterestToPay { get { return _totalInterestToPay - _paidInterest; } }
        public decimal FeesToPay { get { return _totalFeesToPay - _paidFees; } }

        public decimal InterestRate
        {
            get
            {
                var item = _schedule.FirstOrDefault(i => i.Date >= _lastActionDate);
                if (item == null) item = _schedule.Last();
                if (item == null) return _loan.InterestRate;
                return item.InterestRate;
            }
        }

        public decimal TotalEarlyPayment()
        {
            decimal amount = 0;
            _eventDayEnd.Action = () =>
            {
                var payment = Math.Max(0, InterestToPay )+ FeesToPay + _loan.LoanAmount - _paidPrincipal;
                var rollovers = _rollovers.Where(r => r.Status != RolloverStatus.Expired).Sum(r => r.Payment - r.PaidPaymentAmount);
                amount = Math.Round(payment + rollovers + FeesToPay, 2);
            };
            Recalculate();
            _eventDayEnd.Action = null;
            return amount;
        }

        public decimal PayEarly(decimal amount)
        {
            do
            {
                Recalculate();
            } while (_shifted);
            return 0;
        }

        public LoanScheduleItem GetState()
        {
            var item = new LoanScheduleItem() { Loan = _loan, Date = _term };

            _eventDayEnd.Action = () =>
            {
                item.Interest = Math.Round(InterestToPay, 2);
                item.Fees = Math.Round(FeesToPay, 2);
                item.LoanRepayment = Math.Max(0, _totalPrincipalToPay);
                item.AmountDue = item.Interest + item.Fees + item.LoanRepayment;

                var rollover = _totalRollOversToPay - _paidRollOvers;
                item.AmountDue += rollover;
                item.Fees += rollover;
            };
            Recalculate();
            _eventDayEnd.Action = null;
            return item;
        }

        private void Recalculate()
        {
            Init();
            foreach (var e in _events)
            {
                UpdateState(e.Date);

                if (e.Installment != null)
                {
                    ProcessInstallment(e.Installment);
                }
                if (e.Charge != null)
                {
                    ProcessCharge(e.Charge);
                }
                if (e.Rollover != null)
                {
                    ProcessRollover(e.Rollover);
                }
                if (e.Payment != null)
                {
                    ProcessPayment(e.Payment);
                }
                if (e.Action != null)
                {
                    e.Action();
                }
            }
            _loan.UpdateBalance();
            _loan.UpdateStatus(_term);
        }

        private void ProcessRollover(PaymentRollover rollover)
        {
            if (rollover.Status == RolloverStatus.Removed) return;

            _currentRollover.Add(rollover);
            rollover.PaidPaymentAmount = 0;
            _totalRollOversToPay += rollover.Payment;
        }

        private void ProcessCharge(LoanCharge charge)
        {
            if (_loan.Status == LoanStatus.PaidOff)
            {
                charge.State = "Expired";
                return;
            }         

            var lastInstallment = _processed.Where(i => i.Status == LoanScheduleStatus.Late || i.Status == LoanScheduleStatus.StillToPay).LastOrDefault();

            if (lastInstallment != null)
            {
                if (lastInstallment.Status == LoanScheduleStatus.Late || (lastInstallment.Status == LoanScheduleStatus.StillToPay && _schedule.Count == _processed.Count))
                {
                    lastInstallment.AmountDue += charge.Amount;
                    lastInstallment.Fees += charge.Amount;
                }
            }

            _totalFeesToPay += charge.Amount;

            charge.AmountPaid = 0;
            _chargesToPay.Add(charge);
        }

        private void UpdateState(DateTime date)
        {
            //доход банка, за расчетный период
            var interest = _principal * GetInterestRate(_lastActionDate, date);
            _totalInterestToPay += interest;

            //время с момента последнего расчета
            _lastActionDate = date;

            //помечяем устаревшие rollovers и удаляем их из списка
            foreach (var rollover in _currentRollover)
            {
                if (rollover != null && rollover.ExpiryDate.Value.Date <= date.Date && date.Date <= _term)
                {
                    _totalRollOversToPay -= rollover.Payment - rollover.PaidPaymentAmount;
                    rollover.Status = RolloverStatus.Expired;
                }
            }
            _currentRollover = _currentRollover.Where(r => r.Status != RolloverStatus.Expired).ToList();
        }

        /// <summary>
        /// Возвращает процентную ставку, для конкретного периода.
        ///  </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public decimal GetInterestRate(DateTime start, DateTime end)
        {
            var rate = 0m;

            for (var start2 = start.AddMonths(1); start2 <= end; )
            {
                rate += GetInterestRateOneMonth(start, start2);
                _daysInMonth = DaysInMonth(start2);
                start = start2;
                start2 = start2.AddMonths(1);
            }

            rate += GetInterestRateOneMonth(start, end);
            return rate;
        }

        private decimal GetInterestRateOneMonth(DateTime start, DateTime end)
        {
            var span = (end.Date - start.Date);
            var days = (decimal)Math.Floor(span.TotalDays);

            return days * InterestRate / _daysInMonth;
        }

        private static int DaysInMonth(DateTime date)
        {
            return DateTime.DaysInMonth(date.Year, date.Month);
        }

        private void ProcessPayment(PaypointTransaction payment)
        {
            payment.Rollover = 0;

            var money = payment.Amount;
            var interestToPay = Math.Max(0, _totalInterestToPay - _paidInterest);
            var feestToPay = _totalFeesToPay - _paidFees;
            _lastPaymentDate = payment.PostDate;


            //Платим Fees
            var amount = Math.Min(money, Math.Round(feestToPay, 2));
            _paidFees += amount;
            money = money - amount;
            payment.Fees = amount;
            RecordFeesPayment(amount);

            //Платим Interest
            amount = payment.InterestOnly ? money : Math.Min(money, Math.Round(interestToPay, 2));
            _paidInterest += amount;
            money = money - amount;
            payment.Interest = amount;
            RecordInterestPayment(amount);

            //Платим тело rollover
            foreach (var rollover in _rollovers)
            {
                money = PayRollover(rollover, payment, money);
            }
            //удаляем оплаченные rollover
            _currentRollover = _currentRollover.Where(r => r.Status != RolloverStatus.Paid).ToList();

            //Платим тело кредита
            amount = Math.Min(money, _principal);
            _principal = _principal - amount;
            money = money - amount;
            _paidPrincipal += amount;
            payment.LoanRepayment = amount;
            RecordPrincipalPayment(amount);

            UpdateInstallmentsState();
        }

        private decimal PayRollover(PaymentRollover rollover, PaypointTransaction payment, decimal money)
        {
            decimal amount;
            if (
                rollover != null &&
                rollover.Created.Date <= _lastActionDate.Date &&
                _lastActionDate < rollover.ExpiryDate &&
                _totalRollOversToPay > _paidRollOvers
                //_currentRollover.Status == RolloverStatus.New
                )
            {
                amount = Math.Min(money, rollover.Payment - rollover.PaidPaymentAmount);
                //_currentRollover.Payment = _currentRollover.Payment - amount;
                rollover.PaidPaymentAmount += amount;
                payment.Rollover += amount;
                _paidRollOvers += amount;
                money = money - amount;

                //если rollover оплачен
                if (_paidRollOvers >= _totalRollOversToPay)
                {
                    //тогда сдвигаем расписание в случае "первой" оплаты
                    if (rollover.Status == RolloverStatus.New)
                    {
                        _loan.ShiftPayments(rollover.LoanSchedule.Date, rollover.MounthCount);
                        ShiftEvents(rollover.LoanSchedule.Date, rollover.MounthCount);
                        rollover.CustomerConfirmationDate = payment.PostDate;
                        rollover.PaymentNewDate = rollover.LoanSchedule.Date;
                    }
                    rollover.Status = RolloverStatus.Paid;
                }
            }
            return money;
        }

        private void ShiftEvents(DateTime date, int months)
        {
            foreach (var e in _events.Where(e => e.Date.Date <= date.Date))
            {
                e.Date = e.Date.AddMonths(months);
                _shifted = true;
            }
        }

        // при просроченном платеже, необходимо обновить все незакрытые installment
        private void RecordFeesPayment(decimal amount)
        {
            var oldAmount = amount;
            foreach (var installment in _processed.Where(i => i.Fees > 0))
            {
                var money = Math.Min(installment.Fees, amount);
                installment.Fees -= money;
                installment.AmountDue -= money;
                amount = amount - money;
            }

            amount = oldAmount;
            foreach (var charge in _chargesToPay)
            {
                var money = Math.Min(charge.Amount, amount);
                charge.AmountPaid += money;
                if (charge.Amount == charge.AmountPaid)
                {
                    charge.State = "Paid";
                }
                amount = amount - money;
            }
            _chargesToPay = _chargesToPay.Where(c => c.State != "Paid").ToList();
        }

        // при просроченном платеже, необходимо обновить все незакрытые installment
        private void RecordInterestPayment(decimal amount)
        {
            foreach (var installment in _processed.Where(i => i.Interest > 0))
            {
                var money = Math.Min(installment.Interest, amount);
                installment.Interest -= money;
                installment.AmountDue -= money;
            }
        }

        // при просроченном платеже, необходимо обновить все незакрытые installment
        private void RecordPrincipalPayment(decimal amount)
        {
            foreach (var installment in _processed.Where(i => i.LoanRepayment > 0))
            {
                var money = Math.Min(installment.LoanRepayment, amount);
                installment.LoanRepayment -= money;
                installment.AmountDue -= money;
                _totalPrincipalToPay -= money;
                amount = amount - money;
            }
        }

        private void UpdateInstallmentsState()
        {
            foreach (var installment in _processed.Where(i => i.Status == LoanScheduleStatus.Late && i.AmountDue == 0))
            {
                installment.Status = LoanScheduleStatus.Paid;
            }
        }

        private void ProcessInstallment(LoanScheduleItem installment)
        {
            installment.PrevInstallmentDate = _prevInstallmentDate;
            _prevInstallmentDate = installment.Date;
            installment.BalanceBeforeRepayment = _principal;
            installment.LoanRepayment = _expectedPrincipal - installment.Balance;

            _expectedPrincipal = installment.Balance;

            _daysInMonth = DaysInMonth(installment.Date);

            //сколько должны заплатить по телу кредита в рамках этого installment
            var principalToPay = _principal - _expectedPrincipal - _rescentLate.Where(x => x.Status == LoanScheduleStatus.Late).Sum(x => x.LoanRepayment);
            var interestToPay = _totalInterestToPay - _paidInterest - _processed.Sum(x => x.Interest);
            var feesToPay = _totalFeesToPay - _paidFees - _processed.Sum(x => x.Fees);

            installment.Interest = Math.Max(0, Math.Round(interestToPay, 2));
            installment.Fees = Math.Round(feesToPay, 2);

            installment.Status = LoanScheduleStatus.StillToPay;

            //если на момент installment у клиента на руках было меньше денег, чем должно было остаться, то платеж считается оплаченным.
            if (_principal <= _expectedPrincipal)
            {
                _loan.LoanType.BalanceReachedExpected(installment);
                CloseInstallment(installment);
            }
            if (installment.Date < _term && installment.Status == LoanScheduleStatus.StillToPay)
            {
                installment.Status = LoanScheduleStatus.Late;
                _rescentLate.Add(installment);
            }
            else
            {
                _rescentLate.Clear();
            }

            installment.LoanRepayment = Math.Round(Math.Max(0, principalToPay), 2);
            installment.AmountDue = installment.LoanRepayment + installment.Interest + installment.Fees;

            //для installmentов в будущем, которые следует оплатить будем считать, что клиент платит четко по расписанию
            if (_term <= installment.Date && _principal > installment.Balance && installment.Status == LoanScheduleStatus.StillToPay)
            {
                _principal = installment.Balance;
            }

            _totalPrincipalToPay += installment.LoanRepayment;

            _processed.Add(installment);

        }

        private void CloseInstallment(LoanScheduleItem installment)
        {
            if (installment.Interest != 0) return;

            if (!_lastPaymentDate.HasValue)
            {
                throw new Exception("Trying to close installment without payments");
            }

            var insDate = installment.Date.Date;
            var paymentDate = _lastPaymentDate.Value.Date;

            if (paymentDate < insDate)
            {
                installment.Status = LoanScheduleStatus.PaidEarly;
            }
            else if (paymentDate == insDate)
            {
                installment.Status = LoanScheduleStatus.PaidOnTime;
            }
            else
            {
                installment.Status = LoanScheduleStatus.Paid;
            }

            installment.Interest = 0;
            installment.LoanRepayment = 0;
            installment.AmountDue = 0;
        }
    }
}