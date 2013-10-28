﻿namespace EzBob.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using EZBob.DatabaseLib.Model.Loans;
	using EzBob.Web.Areas.Customer.Models;

    [Serializable]
    public class LoanModel
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DateClosed { get; set; }
        public decimal LoanAmount { get; set; }
        public string Status { get; set; }
        public string StatusDescription { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal NextRepayment { get; set; }
        public decimal Fees { get; set; }
        public decimal Interest { get; set; }
        public string RefNumber { get; set; }
        public decimal InterestRate { get; set; }
        public decimal NextEarlyPayment { get; set; }
        public decimal TotalEarlyPayment { get; set; }
        public decimal APR { get; set; }
        public decimal SetupFee { get; set; }
        public decimal Repayments { get; set; }
        public decimal Principal { get; set; }
        public decimal AmountDue { get; set; }
        public decimal NextInterestPayment { get; set; }

        public List<LoanAgreementModel> Agreements { get; set; }

        public List<LoanScheduleItemModel> Schedule { get; set; }
        public LoanCharge Charge { get; set; }
        public decimal Late { get; set; }

        public string LastReportedCaisStatus { get; set; }
        public DateTime? LastReportedCaisStatusDate { get; set; }

        public string LoanType { get; set; }
        public bool Modified { get; set; }
        public string DiscountPlan { get; set; }

        public decimal? InterestDue { get; set; }

        public List<string> InterestFreeze { get; set; }

		public bool IsEarly { get; set; }

		public string LoanSourceName { get; set; }

        public static LoanModel FromLoan(Loan loan, ILoanRepaymentScheduleCalculator calculator, ILoanRepaymentScheduleCalculator calculatorForNow = null)
        {
            var nowState = calculatorForNow != null ? calculatorForNow.GetState() : new LoanScheduleItem();

            var loanModel = new LoanModel
                {
                    Date = loan.Date,
                    Id = loan.Id,
                    Position = loan.Position + 1,
                    RefNumber = loan.RefNumber,
                    LoanAmount = loan.LoanAmount,
                    Status = loan.Status.ToString(),
                    StatusDescription = loan.Status.ToDescription(),
                    Balance = loan.Schedule.Sum(s => s.LoanRepayment),
                    TotalBalance = loan.Schedule.Sum(s => s.AmountDue),
                    Principal = loan.Principal,
                    NextRepayment = loan.NextRepayment,
                    NextInterestPayment = loan.NextInterestPayment,
                    Repayments = loan.Repayments,
                    Fees = loan.Schedule.Sum(i => i.Fees),
                    APR = loan.APR,
                    SetupFee = loan.SetupFee,
                    Interest = loan.Interest,
                    InterestRate = loan.InterestRate,
                    NextEarlyPayment = calculator.NextEarlyPayment(),
                    TotalEarlyPayment = calculator.TotalEarlyPayment(),
                    DateClosed = loan.DateClosed,
                    Agreements = loan.Agreements.Select(a => new LoanAgreementModel(a.Id, a.Name)).ToList(),
                    Charge = LoanChargesModel.FromCharges(loan.Charges.FirstOrDefault(l => l.Date == loan.Charges.Max(a => a.Date))),
                    AmountDue = nowState != null ? nowState.AmountDue : -1,
                    Late = loan.Schedule.Where(s => s.Status == LoanScheduleStatus.Late).Sum(s => s.LoanRepayment) + nowState.Interest + nowState.Fees + nowState.LateCharges,
                    LastReportedCaisStatus = loan.LastReportedCaisStatus,
                    LastReportedCaisStatusDate = loan.LastReportedCaisStatusDate,
                    LoanType = loan.LoanType.Name,
                    Modified = loan.Modified || (loan.CashRequest != null && !string.IsNullOrEmpty(loan.CashRequest.LoanTemplate)),
                    InterestDue = loan.InterestDue,
                    InterestFreeze = loan.InterestFreeze.OrderBy(f => f.StartDate).Select(f => f.ToString()).ToList()
                };

			if (loan.Schedule != null)
			{
				var scheduledPayments = loan.Schedule.Where(x => x.Status == LoanScheduleStatus.StillToPay ||
				                                                 x.Status == LoanScheduleStatus.Late ||
				                                                 x.Status == LoanScheduleStatus.AlmostPaid);

				if (scheduledPayments.Any())
				{
					DateTime earliestSchedule = scheduledPayments.Min(x => x.Date);
					if (earliestSchedule.Date >= DateTime.UtcNow && (earliestSchedule.Date.Year != DateTime.UtcNow.Year || earliestSchedule.Date.Month != DateTime.UtcNow.Month || earliestSchedule.Date.Day != DateTime.UtcNow.Day))
					{
						loanModel.IsEarly = true;
					}
				}
			}

            if (loan.CashRequest!= null)
            {
                loanModel.DiscountPlan = loan.CashRequest.DiscountPlan.Name;
	            loanModel.LoanSourceName = loan.CashRequest.LoanSource.Name;
            }
            return loanModel;
        }
    }
}