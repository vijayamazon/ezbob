namespace EzBob.Web.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using Backend.Models;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EzBob.Models;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.LegalDocs;

	[DataContract]
	public class LoanOfferDetails {
		[DataMember]
		public decimal OfferedCreditLine { get; set; } // offered amount

		[DataMember]
		public int RepaymentPeriod { get; set; } // offered repayment period

		[DataMember]
		public decimal InterestRate { get; set; } // percent

		[DataMember]
		public string LoanType { get; set; } // loan type

		[DataMember]
		public bool IsModified { get; set; } // not in use (only in some report)

		[DataMember]
		public DateTime Date { get; set; } // not in use (only in some report)
	}

	[DataContract]
	public class LoanOffer {
		[DataMember]
		public decimal ManualSetupFeePercent { get; set; }

		[DataMember]
		public decimal BrokerFeePercent { get; set; }

		[DataMember]
		public LoanScheduleItemModel[] Schedule { get; set; }

		[DataMember]
		public double Apr { get; set; } //in use for personal

		[DataMember]
		public decimal Total { get; set; } // total loan amount with interest and fees

		[DataMember]
		public decimal SetupFee { get; set; } // setup fee amount

		[DataMember]
		public decimal TotalPrincipal { get; set; } // total principal

		[DataMember]
		public decimal TotalInterest { get; set; } // total interest + charges + setup fee

		[DataMember]
		public decimal RealInterestCost { get; set; } // (total interest / Loan amount) / repayment period annualized

		[DataMember]
		public long TimeStamp { get; set; } //utcnow.ticks not in use

		[DataMember]
		public decimal LoanAmount { get; set; } // loan amount 

		[DataMember]
		public AgreementModel Agreement { get; set; } // agreement model

		[DataMember]
		public List<LoanAgreementTemplate> Templates { get; set; }

		[DataMember]
		public LoanOfferDetails Details { get; set; } // offer model

		[DataMember]
		public decimal? MaxInterestForSource { get; set; } // max interest for warning in UW

		[DataMember]
		public string LoanSourceName { get; set; } // loan source for UW

		[DataMember]
		public string ManualAddressWarning { get; set; } // manual address for warning in UW

		private static readonly RepaymentCalculator _repaymentCalculator = new RepaymentCalculator();

		public static LoanOffer InitFromLoan(Loan loan, double calculateApr, AgreementModel agreement, CashRequest cr) {
			var repaymentPeriod = _repaymentCalculator.CalculateCountRepayment(loan);
			var apr = loan.LoanAmount == 0 ? 0 : calculateApr;
			var total = loan.Schedule.Sum(s => s.AmountDue) + loan.SetupFee;
			var totalPrincipal = loan.Schedule.Sum(s => s.LoanRepayment);
			var totalInterest = loan.Schedule.Sum(s => s.Interest) + loan.Charges.Sum(x => x.Amount) + loan.SetupFee;
			var realInterestCost = loan.LoanAmount == 0 ? 0 : (totalInterest / loan.LoanAmount) / (repaymentPeriod / 12.0M);
			var timestamp = DateTime.UtcNow.Ticks;

			var offer = new LoanOffer();
			offer.Schedule = loan.Schedule.Select(LoanScheduleExtention.FromLoanScheduleItem).ToArray();
			offer.Apr = apr;
			offer.SetupFee = loan.SetupFee;
			offer.Total = total;
			offer.RealInterestCost = realInterestCost;
			offer.LoanAmount = loan.LoanAmount;
			offer.TimeStamp = timestamp;
			offer.TotalInterest = totalInterest;
			offer.TotalPrincipal = totalPrincipal;
			offer.Agreement = agreement;
			offer.Details = new LoanOfferDetails {
				InterestRate = cr.InterestRate,
				RepaymentPeriod = repaymentPeriod,
				OfferedCreditLine = totalPrincipal,
				LoanType = cr.LoanType.Name,
				IsModified = !string.IsNullOrEmpty(cr.LoanTemplate),
				Date = loan.Date
			};
			offer.MaxInterestForSource = loan.LoanSource == null ? null : loan.LoanSource.MaxInterest;
			offer.LoanSourceName = loan.LoanSource == null ? "" : loan.LoanSource.Name;
			offer.ManualAddressWarning = cr.Customer == null ? "" : cr.Customer.ManualAddressWarning();

			return offer;
		}
	}
}

