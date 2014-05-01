namespace EzBob.Web.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using Backend.Models;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EzBob.Models;
	using Ezbob.Backend.Models;

	[DataContract]
    public class LoanOfferDetails
    {
		[DataMember]
		public decimal OfferedCreditLine { get; set; }

		[DataMember]
		public int RepaymentPeriod { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }

		[DataMember]
		public string LoanType { get; set; }

		[DataMember]
		public bool IsModified { get; set; }

		[DataMember]
		public DateTime Date { get; set; }
    }

	[DataContract]
    public class LoanOffer
    {
		[DataMember]
		public LoanScheduleItemModel[] Schedule { get; set; }

		[DataMember]
		public double Apr { get; set; }

		[DataMember]
		public decimal Total { get; set; }

		[DataMember]
		public decimal SetupFee { get; set; }
		
		[DataMember]
		public decimal TotalPrincipal { get; set; }

		[DataMember]
		public decimal TotalInterest { get; set; }

		[DataMember]
		public decimal RealInterestCost { get; set; }

		[DataMember]
		public long TimeStamp { get; set; }

		[DataMember]
		public decimal LoanAmount { get; set; }

		[DataMember]
		public AgreementModel Agreement { get; set; }

		[DataMember]
		public LoanOfferDetails Details { get; set; }

		[DataMember]
		public decimal? MaxInterestForSource { get; set; }

		[DataMember]
		public string LoanSourceName { get; set; }

		[DataMember]
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

	        var offer = new LoanOffer();

			List<LoanScheduleItemModel> l = new List<LoanScheduleItemModel>();
			foreach (var s in loan.Schedule)
			{
				LoanScheduleItemModel n = new LoanScheduleItemModel
				{
					Id = s.Id,
					AmountDue = s.AmountDue,
					Date = s.Date,
					PrevInstallmentDate = s.PrevInstallmentDate,
					Interest = s.Interest,
					InterestPaid = s.InterestPaid,
					LateCharges = s.LateCharges,
					RepaymentAmount = s.RepaymentAmount,
					Status = s.Status.ToString(),
					StatusDescription = s.Status.ToDescription(),
					LoanRepayment = s.LoanRepayment,
					Balance = s.Balance,
					BalanceBeforeRepayment = s.BalanceBeforeRepayment,
					Fees = s.Fees,
					InterestRate = s.InterestRate
				};
				l.Add(n);
			}

			offer.Schedule = l.ToArray();
			offer.Apr = apr;
			offer.SetupFee = loan.SetupFee;
			offer.Total = total;
			offer.RealInterestCost = realInterestCost;
			offer.LoanAmount = loan.LoanAmount;
			offer.TimeStamp = timestamp;
			offer.TotalInterest = totalInterest;
			offer.TotalPrincipal = totalPrincipal;
			offer.Agreement = agreement;
			offer.Details = new LoanOfferDetails
				{
					InterestRate = cr.InterestRate,
					RepaymentPeriod = _repaymentCalculator.ReCalculateRepaymentPeriod(cr),
					OfferedCreditLine = totalPrincipal,
					LoanType = cr.LoanType.Name,
					IsModified = !string.IsNullOrEmpty(cr.LoanTemplate),
					Date = loan.Date
				};
			offer.MaxInterestForSource = loan.LoanSource == null ? null : loan.LoanSource.MaxInterest;
			offer.LoanSourceName = loan.LoanSource == null ? "" : loan.LoanSource.Name;
			offer.ManualAddressWarning = cr.Customer.ManualAddressWarning();

            return offer;
        }
    }
  }

