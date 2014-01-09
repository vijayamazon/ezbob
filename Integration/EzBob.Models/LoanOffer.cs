namespace EzBob.Models
{
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EzBob.Web.Areas.Customer.Models;

    public class LoanOfferDetails
    {
        public decimal OfferedCreditLine { get; set; }
        public int RepaymentPeriod { get; set; }
        public decimal InterestRate { get; set; }
        public string LoanType { get; set; }
        public bool IsModified { get; set; }
        public DateTime Date { get; set; }
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
		public decimal? MaxInterestForSource { get; set; }
		public string LoanSourceName { get; set; }
	    public string ManualAddressWarning { get; set; }

	    private static readonly RepaymentCalculator _repaymentCalculator = new RepaymentCalculator();

        public static LoanOffer InitFromLoan(Loan loan, double calculateApr,  AgreementModel agreement, CashRequest cr)
        {
            var apr = loan.LoanAmount == 0 ? 0 : calculateApr;
            var total = loan.Schedule.Sum(s => s.AmountDue) + loan.SetupFee;
            var totalPrincipal = loan.Schedule.Sum(s => s.LoanRepayment);
            var totalInterest = loan.Schedule.Sum(s => s.Interest) + loan.Charges.Sum(x => x.Amount) + loan.SetupFee;
            var realInterestCost = loan.LoanAmount == 0 ? 0 : totalInterest / loan.LoanAmount;
            var timestamp = DateTime.UtcNow.Ticks;

            var offer = new LoanOffer()
                {
                    Schedule = loan.Schedule.Select(s => LoanScheduleItemModel.FromLoanScheduleItem(s)).ToArray(),
                    Apr = apr,
                    SetupFee = loan.SetupFee,
                    Total = total,
                    RealInterestCost = realInterestCost,
                    LoanAmount = loan.LoanAmount,
                    TimeStamp = timestamp,
                    TotalInterest = totalInterest,
                    TotalPrincipal = totalPrincipal,
                    Agreement = agreement,
                    Details = new LoanOfferDetails
                        {
                            InterestRate = cr.InterestRate,
                            RepaymentPeriod = _repaymentCalculator.ReCalculateRepaymentPeriod(cr),
                            OfferedCreditLine = totalPrincipal,
                            LoanType = cr.LoanType.Name,
                            IsModified = !string.IsNullOrEmpty(cr.LoanTemplate),
                            Date = loan.Date
                        },
                    MaxInterestForSource = loan.LoanSource == null ? null : loan.LoanSource.MaxInterest,
					LoanSourceName = loan.LoanSource == null ? "" : loan.LoanSource.Name,
					ManualAddressWarning = loan.Customer.ManualAddressWarning(),
                };

            return offer;
        }
    }
  }

