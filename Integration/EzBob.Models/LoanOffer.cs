using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Models
{
    public class LoanOfferDetails
    {
        public decimal OfferedCreditLine { get; set; }
        public int RepaymentPeriod { get; set; }
        public decimal InterestRate { get; set; }
        public string LoanType { get; set; }
    }

    public class LoanOffer
    {
        public LoanScheduleItemModel[] Schedule { get; set; }
        public double Apr { get; set; }
        public decimal Total { get; set; }
        public decimal SetupFee { get; set; }
        public decimal TotalPrincipal { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal RealInterestCost { get; set; }
        public long TimeStamp { get; set; }
        public decimal LoanAmount { get; set; }
        public AgreementModel Agreement { get; set; }
        public LoanOfferDetails Details { get; set; }

        public static LoanOffer InitFromLoan(Loan loan, double calculateApr,  AgreementModel agreement )
        {
            var apr = loan.LoanAmount == 0 ? 0 : calculateApr;
            var total = loan.Schedule.Sum(s => s.AmountDue) + loan.SetupFee;
            var totalPrincipal = loan.Schedule.Sum(s => s.LoanRepayment);
            var totalInterest = loan.Schedule.Sum(s => s.Interest) + loan.Charges.Sum(x => x.Amount) + loan.SetupFee;
            var realInterestCost = loan.LoanAmount == 0 ? 0 : totalInterest / loan.LoanAmount;
            var timestamp = DateTime.UtcNow.Ticks;

            return new LoanOffer()
            {
                Schedule = loan.Schedule.Select(s => LoanScheduleItemModel.FromLoanScheduleItem(s)).ToArray(),
                Apr=apr,
                SetupFee = loan.SetupFee,
                Total = total,
                RealInterestCost = realInterestCost,
                LoanAmount = loan.LoanAmount,
                TimeStamp = timestamp,
                TotalInterest = totalInterest,
                TotalPrincipal = totalPrincipal,
                Agreement = agreement
            };
        }
    }
  }

