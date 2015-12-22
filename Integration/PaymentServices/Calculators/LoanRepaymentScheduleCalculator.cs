namespace PaymentServices.Calculators {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;

	public class LoanRepaymentScheduleCalculator : ILoanRepaymentScheduleCalculator {
		//private static readonly ILog _log = LogManager.GetLogger(typeof(LoanRepaymentScheduleCalculator));
		private readonly Loan _loan;
		private DateTime _term;
		private readonly IList<LoanScheduleItem> _schedule;
		private readonly List<PaypointTransaction> _payments;
		private readonly List<LoanCharge> _charges;

		//state variables

		/// <summary>
		///деньги, которые реально находятся у клиента на руках. баланс кредита без процентов и fee
		///money, that really have customer. loan balance without interest and fees
		/// </summary>
		private decimal _principal;

		/// <summary>
		/// ожидаемый баланс.
		/// expected balance
		/// </summary>
		private decimal _expectedPrincipal;

		/// <summary>
		/// Ожидаемый баланс. При каждом installment увеличивается, в независимости от того в будущем этот installment, или нет
		/// expected balance. In each installment increases, no matter if this installment is in future or not
		/// </summary>
		private decimal _totalPrincipalToPay;

		/// <summary>
		/// Сумма реально оплаченного principal
		/// Amount of really paid principal
		/// </summary>
		private decimal _paidPrincipal;

		/// <summary>
		/// дата последнего действия. нужна для расчета процентов
		/// date of last action. Needed for interest calculation
		/// </summary>
		private DateTime _lastActionDate;
		private DateTime? _lastPaymentDate;


		/// <summary>
		/// список обработанных installment'ов
		/// list of processed installments
		/// </summary>
		private List<LoanScheduleItem> _processed = new List<LoanScheduleItem>();

		/// <summary>
		/// последовательность событий, относящихся к кредиту
		/// events sequence, related to loan
		/// </summary>
		private List<LoanRepaymentScheduleCalculatorEvent> _events = new List<LoanRepaymentScheduleCalculatorEvent>();

		/// <summary>
		/// Доход банка за все время заема
		/// total loan interest
		/// </summary>
		private decimal _totalInterestToPay = 0;


		/// <summary>
		/// Сколько всего клиент заплатил процентов
		/// total repaid interest
		/// </summary>
		private decimal _paidInterest = 0;


		/// <summary>
		/// Штрафы, которые надо было заплатить
		/// Total fees that have to be paid
		/// </summary>
		private decimal _totalFeesToPay = 0;

		//Штрафы, уплаченные клиентом
		private decimal _paidFees = 0;

		private decimal _paidRollOvers = 0;
		private decimal _totalRollOversToPay = 0;

		private LoanRepaymentScheduleCalculatorEvent _eventDayStart;
		private LoanRepaymentScheduleCalculatorEvent _eventDayEnd;


		/// <summary>
		/// количество дней в месяце, в текущем периоде
		/// Num of days in month, for current period
		/// </summary>
		private int _daysInMonth;


		/// <summary>
		/// текущий roll over, который должен быть оплачен при платеже
		/// current roll over, that have to be paid during repayment
		/// </summary>
		private List<PaymentRollover> _currentRollover = new List<PaymentRollover>();
		private List<PaymentRollover> _rollovers;


		/// <summary>
		/// было ли расписание сдвинуто
		/// was the schedule shifted
		/// </summary>
		private bool _shifted = false;

		private List<LoanScheduleItem> _rescentLate = new List<LoanScheduleItem>();

		private List<LoanCharge> _chargesToPay = new List<LoanCharge>();

		private DateTime _prevInstallmentDate = DateTime.MinValue;

		private readonly decimal _amountToChargeFrom = 0;

		public LoanRepaymentScheduleCalculator(Loan loan, DateTime? term, int amountToChargeFrom) {
			_loan = loan;
			_schedule = loan.Schedule;
			_payments = loan.TransactionsWithPaypointSuccesefull;
			_charges = loan.Charges.OrderBy(x => x.Date).ToList();

			_term = (term ?? DateTime.UtcNow).Date;
			//CalculationDate = _term;

			_eventDayStart = new LoanRepaymentScheduleCalculatorEvent(_term);
			_eventDayEnd = new LoanRepaymentScheduleCalculatorEvent(_term.AddHours(23).AddMinutes(59).AddSeconds(59));

			_amountToChargeFrom = amountToChargeFrom;

			Init();
		}

		//public DateTime CalculationDate {
		//	get { return _term; }
		//	private set
		//}

		private void Init() {
			_shifted = false;

			_chargesToPay.Clear();
			_rescentLate.Clear();

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
			if (_schedule.All(i => i.InterestRate == 0)) {
				foreach (var installment in _schedule) {
					installment.InterestRate = _loan.InterestRate;
				}
			}

			var lateChargeEvents = _charges.Select(p => new LoanRepaymentScheduleCalculatorEvent(new DateTime(p.Date.Year, p.Date.Month, p.Date.Day, 23, 59, 57), p)).ToList();
			var paymentEvents = _payments.Select(p => new LoanRepaymentScheduleCalculatorEvent(new DateTime(p.PostDate.Year, p.PostDate.Month, p.PostDate.Day, 23, 59, 58), p)).ToList();
			var installmentEvents = _schedule.Select(i => new LoanRepaymentScheduleCalculatorEvent(new DateTime(i.Date.Year, i.Date.Month, i.Date.Day, 23, 59, 59), i));

			var rollOverEvents = _rollovers.Select(p => new LoanRepaymentScheduleCalculatorEvent(p.Created.Date, p)).ToList();

			_events = installmentEvents
								 .Union(paymentEvents)
								 .Union(lateChargeEvents)
								 .Union(rollOverEvents)
								 .Union(new[] { _eventDayStart, _eventDayEnd })
								 .OrderBy(e => e.Date)
								 .ThenBy(e => e.Priority)
								 .ToList();

			//_events.ForEach(e=> Console.WriteLine(e));
		}

		public decimal NextEarlyPayment() {
			decimal amount = 0;
			_eventDayEnd.Action = () => {
				if (_loan.Status == LoanStatus.PaidOff)
					return;

				var principal = _loan.LoanAmount - _paidPrincipal;

				//Console.WriteLine("NextEarlyPayment principal={0} _expectedPrincipal={1} InterestToPay={2}", principal, _expectedPrincipal, InterestToPay);

				//если пользователь пропустил платеж
				//if user missed payment
				if (principal > _expectedPrincipal) {
					amount = Math.Round(principal - _expectedPrincipal + InterestToPay, 2);
					return;
				}

				var next = _schedule.FirstOrDefault(s => s.Date >= _lastActionDate && s.LoanRepayment > 0) ??_schedule.LastOrDefault();

				if (next != null) {

					var expectedPrincipal = next.Balance;
					amount = Math.Round(Math.Max(0, principal - expectedPrincipal + InterestToPay + FeesToPay), 2);

					Console.WriteLine("NextEarlyPayment next={0} expectedPrincipal={1} amount={2}", next, expectedPrincipal, amount);

				}
			};
			Recalculate();
			_eventDayEnd.Action = null;
			return amount;
		}

		public decimal InterestToPay { get { return _totalInterestToPay - _paidInterest; } }
		public decimal FeesToPay { get { return _totalFeesToPay - _paidFees; } }

		public decimal InterestRate {
			get {
				var item = _schedule.FirstOrDefault(i => i.Date >= _lastActionDate);
				if (item == null) {
					item = _schedule.LastOrDefault();
				}
				if (item == null)
					return _loan.InterestRate;
				return item.InterestRate;
			}
		}


		public decimal TotalEarlyPayment() {
			decimal amount = 0;
			_eventDayEnd.Action = () => {
				var payment = Math.Max(0, InterestToPay) + FeesToPay + _loan.LoanAmount - _paidPrincipal;
				var rollovers = _rollovers.Where(r => r.Status != RolloverStatus.Expired).Sum(r => r.Payment - r.PaidPaymentAmount);
				amount = Math.Round(payment + rollovers, 2);
			};
			Recalculate();
			_eventDayEnd.Action = null;
			return amount;
		}

		public decimal RecalculateSchedule() {
			do {
				Recalculate();
			} while (_shifted);
			return 0;
		}

		public LoanScheduleItem GetState() {
			var item = new LoanScheduleItem() { Loan = _loan, Date = _term };
			_eventDayEnd.Action = () => {
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

		private void Recalculate() {
			Init();
			foreach (var e in _events) {

				//Console.WriteLine("event=={0}", e);

				UpdateState(e.Date);

				if (e.Installment != null) {
					ProcessInstallment(e.Installment);
				}
				if (e.Charge != null) {
					ProcessCharge(e.Charge);
				}
				if (e.Rollover != null) {
					ProcessRollover(e.Rollover);
				}
				if (e.Payment != null) {
					ProcessPayment(e.Payment);
				}
				if (e.Action != null) {
					e.Action();
				}
			}
			_loan.UpdateBalance();
			_loan.UpdateStatus(_term);
		}

		private void ProcessRollover(PaymentRollover rollover) {
			if (rollover.Status == RolloverStatus.Removed)
				return;

			_currentRollover.Add(rollover);
			rollover.PaidPaymentAmount = 0;
			_totalRollOversToPay += rollover.Payment;
		}

		/// <summary>
		/// el: 1. cumulate _totalFeesToPay;  2. add fee amount to first unpaid schedule item all non paid fees; 2. cumulate _chargesToPay list
		/// </summary>
		/// <param name="charge"></param>
		private void ProcessCharge(LoanCharge charge) {
			// el: if loan PaidOff, mark the charge as Expired
			if (_loan.Status == LoanStatus.PaidOff) {
				charge.State = "Expired";
				return;
			}

			// el: attach fee amount to the first unpaid schedule item. will be last item from processed schedules list
			var lastInstallment = _processed.Where(i => i.Status == LoanScheduleStatus.Late || i.Status == LoanScheduleStatus.StillToPay).LastOrDefault();
			if (lastInstallment != null) {
				if (lastInstallment.Status == LoanScheduleStatus.Late || (lastInstallment.Status == LoanScheduleStatus.StillToPay && _schedule.Count == _processed.Count)) {
					lastInstallment.AmountDue += charge.Amount;
					lastInstallment.Fees += charge.Amount;
				}
			}

			// el: add amount to totalFees 
			_totalFeesToPay += charge.Amount;

			// el: reset AmountPaid of the charge
			charge.AmountPaid = 0;
			// el: add to list of _chargesToPay - handles in RecordFeesPayment
			_chargesToPay.Add(charge);
		}

		
		// In common: calculate total interest to pay at specific event date, during looping of ordered events list
		// calculate interest rate btwn 2 nearby events in the list; add interest calculated to _totalInterestToPay; 
		// also removing expired rollovers from _currentRollover list
		private void UpdateState(DateTime date) {
			//interest for period
			var interest = _principal * GetInterestRate(_lastActionDate, date);
			_totalInterestToPay += interest;

			//Console.WriteLine("lastActionDate: {0} date: {1} interest: {2} totalInterestToPay: {3} totalPrincipalToPay: {4} principal: {5}", _lastActionDate, date, interest, _totalInterestToPay, _totalPrincipalToPay, _principal);

			//	last event datetime
			_lastActionDate = date;

			//marking expired rollovers and removing them from the list
			var expired = new List<PaymentRollover>();
			foreach (var rollover in _currentRollover.Where(r => r != null)) {
				if (rollover.ExpiryDate.Value.Date <= date.Date && date.Date <= _term) {
					_totalRollOversToPay -= rollover.Payment - rollover.PaidPaymentAmount;
					rollover.Status = RolloverStatus.Expired;
					expired.Add(rollover);
				}
			}
			_currentRollover.RemoveAll(r => expired.Contains(r));
		}

		/// <summary>
		/// Возвращает процентную ставку, для конкретного периода.
		/// Returns interest rate for specific period
		///  </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public decimal GetInterestRate(DateTime start, DateTime end) {
		
			var rate = 0m;

			for (var start2 = start.AddMonths(1); start2 <= end; ) {
				if (start2.Year == end.Year && start2.Month == end.Month) {
					start2 = end;
				}

				rate += GetInterestRateOneMonth(start, start2);
				_daysInMonth = DaysInMonth(start2);
				start = start2;
				start2 = start2.AddMonths(1);
			}

			rate += GetInterestRateOneMonth(start, end);
			return rate;
		}
		/// <summary>
		/// Returns interest rate for defined period with freeze intervals consideration
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// InterestRate - for lastActionDate
		/// <returns></returns>
		private decimal GetInterestRateOneMonth(DateTime start, DateTime end) {

			TimeSpan span = (end.Date - start.Date);
			decimal days = (decimal)Math.Floor(span.TotalDays);

	
			decimal nTotalInterest = 0;

			List<LoanInterestFreeze> freezes = this._loan.InterestFreeze.Where(f => f.DeactivationDate == null).ToList();

			if (freezes.Count == 0) {
				nTotalInterest = days * InterestRate / _daysInMonth;
				/*
				_log.DebugFormat(
					"Loan {0}: total interest {5} between {1} and {2} for {3} earning day{4}.",
					_loan.Id,
					start.ToString("MMM d yyyy HH:mm:ss", CultureInfo.InvariantCulture),
					end.ToString("MMM d yyyy HH:mm:ss", CultureInfo.InvariantCulture),
					days,
					(days == 1) ? "" : "s",
					nTotalInterest
				);
				*/

				return nTotalInterest;
			} // if

			decimal nCurrentInterestRate = InterestRate;

			decimal nStdOneDayInterest = nCurrentInterestRate / _daysInMonth;

			for (DateTime oCurrent = start.Date; oCurrent < end.Date; oCurrent = oCurrent.AddDays(1)) {
				bool bContains = false;

				foreach (LoanInterestFreeze lif in freezes) {
					if (lif.Contains(oCurrent)) {

						nTotalInterest += lif.InterestRate / _daysInMonth;

						bContains = true;

						/*
						_log.DebugFormat(
							"Loan {0}: one day frozen interest {1} for {2}",
							_loan.Id,
							lif.InterestRate,
							oCurrent.ToString("MMM d yyyy HH:mm:ss", CultureInfo.InvariantCulture)
						);
						*/

						break;
					} // if
				} // for each freeze period

				if (!bContains) {
					nTotalInterest += nStdOneDayInterest;

					/*
					_log.DebugFormat(
						"Loan {0}: one day standard interest {1} for {2}",
						_loan.Id,
						nCurrentInterestRate,
						oCurrent.ToString("MMM d yyyy HH:mm:ss", CultureInfo.InvariantCulture)
					);
					*/
				} // if
			} // for each day in the month



			return nTotalInterest;
		} // GetInterestRateOneMonth

		private static int DaysInMonth(DateTime date) {
			return DateTime.DaysInMonth(date.Year, date.Month);
		}

		private void ProcessPayment(PaypointTransaction payment) {
			payment.Rollover = 0;

			var money = payment.Amount;
			var interestToPay = Math.Max(0, _totalInterestToPay - _paidInterest);
			var feestToPay = _totalFeesToPay - _paidFees;
			_lastPaymentDate = payment.PostDate;

			//Paying Fees
			var amount = Math.Min(money, Math.Round(feestToPay, 2));
			_paidFees += amount;
			money = money - amount;
			payment.Fees = amount;
			RecordFeesPayment(amount);

			//Paying Interest
			amount = payment.InterestOnly ? money : Math.Min(money, Math.Round(interestToPay, 2));
			_paidInterest += amount;
			money = money - amount;
			payment.Interest = amount;
			RecordInterestPayment(amount);

			//Paying rollover
			foreach (var rollover in _rollovers) {
				money = PayRollover(rollover, payment, money);
			}
			//Removing payed rollovers
			_currentRollover = _currentRollover.Where(r => r.Status != RolloverStatus.Paid).ToList();

			//Paying principal
			amount = Math.Min(money, _principal);
			_principal = _principal - amount;
			money = money - amount;
			_paidPrincipal += amount;
			payment.LoanRepayment = amount;
			RecordPrincipalPayment(amount);

			UpdateInstallmentsState();
		}

		private decimal PayRollover(PaymentRollover rollover, PaypointTransaction payment, decimal money) {
			decimal amount;
			if (
				rollover != null &&
				rollover.Created.Date <= _lastActionDate.Date &&
				_lastActionDate < rollover.ExpiryDate &&
				_totalRollOversToPay > _paidRollOvers
				//_currentRollover.Status == RolloverStatus.New
				) {
				amount = Math.Min(money, rollover.Payment - rollover.PaidPaymentAmount);
				//_currentRollover.Payment = _currentRollover.Payment - amount;
				rollover.PaidPaymentAmount += amount;
				payment.Rollover += amount;
				_paidRollOvers += amount;
				money = money - amount;

				//if rollover is payed
				if (_paidRollOvers >= _totalRollOversToPay) {
					//than shifting schedule in case of "first" repayment
					if (rollover.Status == RolloverStatus.New) {
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

		private void ShiftEvents(DateTime date, int months) {
			foreach (var e in _events.Where(e => e.Date.Date <= date.Date)) {
				e.Date = e.Date.AddMonths(months);
				_shifted = true;
			}
		}

		// in case of late payment, need update all not closed installments
		private void RecordFeesPayment(decimal amount) {
			var oldAmount = amount;
			foreach (var installment in _processed.Where(i => i.Fees > 0)) {
				var money = Math.Min(installment.Fees, amount);
				installment.Fees -= money;
				installment.FeesPaid += money;
				installment.RepaymentAmount += money;
				installment.AmountDue -= money;
				amount = amount - money;
			}

			amount = oldAmount;
			foreach (var charge in _chargesToPay) {
				var money = Math.Min(charge.Amount, amount);
				charge.AmountPaid += money;
				if (charge.Amount == charge.AmountPaid) {
					charge.State = "Paid";
				}
				amount = amount - money;
			}
			_chargesToPay = _chargesToPay.Where(c => c.State != "Paid").ToList();
		}

		// in case of late payment, need to update all not closed installments
		private void RecordInterestPayment(decimal amount) {
			foreach (var installment in _processed.Where(i => i.Interest > 0)) {
				var money = Math.Min(installment.Interest, amount);
				installment.Interest -= money;
				installment.InterestPaid += money;
				installment.RepaymentAmount += money;
				installment.AmountDue -= money;
			}
		}

		// in case of late payment, need to update all not closed installments
		private void RecordPrincipalPayment(decimal amount) {
			foreach (LoanScheduleItem installment in _processed.Where(i => i.LoanRepayment > 0)) {
				var money = Math.Min(installment.LoanRepayment, amount);
				installment.LoanRepayment -= money;
				installment.AmountDue -= money;
				installment.RepaymentAmount += money;
				installment.Principal += money;
				_totalPrincipalToPay -= money;
				amount = amount - money;
			}
		}

		private void UpdateInstallmentsState() {
			foreach (var installment in _processed) {
				if (installment.AmountDue == 0) {
					if (installment.Status == LoanScheduleStatus.Late) {
						installment.Status = LoanScheduleStatus.Paid;
					} else if (installment.Status == LoanScheduleStatus.StillToPay || installment.Status == LoanScheduleStatus.AlmostPaid) {
						installment.Status = LoanScheduleStatus.PaidOnTime;
					}

					installment.DatePaid = _lastPaymentDate;
				} else if (installment.AmountDue <= _amountToChargeFrom && _schedule.Last().Date != installment.Date) {
					installment.Status = LoanScheduleStatus.AlmostPaid;
				}
			}
		}

		// set LoanRepayment (p), AmountDue (f+i+p), change Status if need; add the item to _processed list etc.
		private void ProcessInstallment(LoanScheduleItem installment) {

			// el: set by previous schedule item date or DateTime.MinVaue
			installment.PrevInstallmentDate = _prevInstallmentDate;
			// el: swap
			_prevInstallmentDate = installment.Date;
			// el: current open principal
			installment.BalanceBeforeRepayment = _principal;
			installment.LoanRepayment = _expectedPrincipal - installment.Balance;

			_expectedPrincipal = installment.Balance;

			_daysInMonth = DaysInMonth(installment.Date);

			//how much have to pay principal in current installment
			var principalToPay = _principal - _expectedPrincipal - _rescentLate.Where(x => x.Status == LoanScheduleStatus.Late).Sum(x => x.LoanRepayment);
			var interestToPay = _totalInterestToPay - _paidInterest - _processed.Sum(x => x.Interest);
			var feesToPay = _totalFeesToPay - _paidFees - _processed.Sum(x => x.Fees);
			installment.Interest = Math.Max(0, Math.Round(interestToPay, 2));
			installment.Fees = Math.Round(feesToPay, 2);

			installment.Status = LoanScheduleStatus.StillToPay;

			//if the moment of installment client have less money than had to be, than the payment considered as paid.

			var diff = _principal - _expectedPrincipal;

			if (diff <= 0) {
				_loan.LoanType.BalanceReachedExpected(installment);
				CloseInstallment(installment);
			}

			if (installment.Date < _term && (installment.Status == LoanScheduleStatus.StillToPay || installment.Status == LoanScheduleStatus.AlmostPaid)) {
				if (diff >= _amountToChargeFrom || _schedule.Last().Date == installment.Date) {
					installment.Status = LoanScheduleStatus.Late;
					_rescentLate.Add(installment);
				} else {
					installment.Status = LoanScheduleStatus.AlmostPaid;
				}
			} else {
				_rescentLate.Clear();
			}

			installment.LoanRepayment = Math.Round(Math.Max(0, principalToPay), 2);
			installment.AmountDue = installment.LoanRepayment + installment.Interest + installment.Fees;

			//for future installments, that have to be paid, will consider that client pays exactly on schedule 
			if (_term <= installment.Date && _principal > installment.Balance && installment.Status == LoanScheduleStatus.StillToPay) {
				_principal = installment.Balance;
			}

			_totalPrincipalToPay += installment.LoanRepayment;

			_processed.Add(installment);

		}

		private void CloseInstallment(LoanScheduleItem installment) {
			if (installment.Interest != 0)
				return;

			if (!_lastPaymentDate.HasValue) {
				throw new Exception("Trying to close installment without payments");
			}

			var insDate = installment.Date.Date;
			var paymentDate = _lastPaymentDate.Value.Date;

			if (paymentDate < insDate) {
				installment.Status = LoanScheduleStatus.PaidEarly;
			} else if (paymentDate == insDate) {
				installment.Status = LoanScheduleStatus.PaidOnTime;
			} else {
				installment.Status = LoanScheduleStatus.Paid;
			}

			installment.Interest = 0;
			installment.LoanRepayment = 0;
			installment.AmountDue = 0;

			installment.DatePaid = paymentDate;
		}
	}
}
