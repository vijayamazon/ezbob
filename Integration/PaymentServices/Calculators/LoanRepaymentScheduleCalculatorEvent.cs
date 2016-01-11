using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;

namespace PaymentServices.Calculators
{
    internal class LoanRepaymentScheduleCalculatorEvent
    {
        private int _priority = 0;

        public int Priority
        {
            get
            {
                if (_priority != 0) return _priority;

                if (Payment != null)
                {
                    return 1;
                }

                if (Installment != null)
                {
                    return 2;
                }

                if (Action != null)
                {
                    return 3;
                }

                return 0;    
            }
        }

        //installment всегда считается в конце дня
		public LoanRepaymentScheduleCalculatorEvent(DateTime date, LoanScheduleItem loanScheduleItem, int priority = 0)
            : this(new DateTime(date.Year, date.Month, date.Day, 23, 59, 59), priority)
        {
            Installment = loanScheduleItem;
        }

        public LoanRepaymentScheduleCalculatorEvent(DateTime date, PaypointTransaction paypointTransaction, int priority = 0)
            : this(date, priority)
        {
            Payment = paypointTransaction;
        }

        public LoanRepaymentScheduleCalculatorEvent(DateTime date, Action action, int priority = 0)
            : this(date, priority)
        {
	        Console.WriteLine("Action:  {0}==={1}; {2}", date, action, priority);
            Action = action;
        }

        public LoanRepaymentScheduleCalculatorEvent(DateTime date, int priority = 0)
        {
            Date = date;
            _priority = priority;
        }

        public LoanRepaymentScheduleCalculatorEvent(DateTime date, LoanCharge loanCharge) :this(date)
        {
            Charge = loanCharge;
        }

        public LoanRepaymentScheduleCalculatorEvent(DateTime date, PaymentRollover rollover) :this(date)
        {
            Rollover = rollover;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Date, GetTypeString());
        }

        public string GetTypeString()
        {

	      if (Installment != null)
            {
                return "Installment";
            }
            if (Payment != null)
            {
                return "Payment";
            }
            if (Charge != null)
            {
                return "Charge";
            }
            if (Rollover != null)
            {
                return "Rollover";
            }
            if (Action != null)
            {
                return "Action";
            }
            return "Unknown";
        }

        public DateTime Date { get; set; }
        public LoanScheduleItem Installment { get; set; }
        public PaypointTransaction Payment { get; set; }
        public LoanCharge Charge { get; set; }
        public Action Action { get; set; }
        public PaymentRollover Rollover { get; set; }
    }
}