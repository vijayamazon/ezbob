namespace EzBob.Web.Areas.Customer.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database.Loans;

	/// <summary>
	/// This is currently part of customer model for profile
	/// </summary>
	public class NewLoansModel {
		public string CustomerName { get; set; }
		public string CustomerRefNum { get; set; }
		public bool IsPersonalTypeOfBusiness { get; set; }

		public decimal TotalEarlyPayment { get { return Loans.Sum(l => l.TotalEarlyPayment); } }
		public decimal TotalLatePayment { get { return Loans.Where(l => l.Status == LoanStatus.Late).Sum(l => l.Late); } }
		public decimal TotalBalance {get { return Loans.Sum(l => l.Balance);}}

		public IEnumerable<NewLoanModel> Loans { get; set; }
		public IEnumerable<NewLoanActiveRolloverModel> ActiveRollovers { get; set; }
		public IEnumerable<NewLoanActiveLoanModel> ActiveLoans {
			get { return Loans
					.Where(loan => loan.Status == LoanStatus.Late || loan.Status == LoanStatus.Live)
					.Select(loan => new NewLoanActiveLoanModel {LoanID = loan.LoanID,LoanRefNum = loan.LoanRefNum}); } }
	}//NewLoansModel

	public class NewLoanActiveRolloverModel {
		public decimal RolloverPayValue { get; set; } // rollover amount
		//todo fill the missing if needed
	}//NewLoanActiveRolloverModel

	public class NewLoanActiveLoanModel {
		public int LoanID { get; set; }
		public string LoanRefNum { get; set; }
	}//NewLoanActiveLoanModel

	public class NewLoanModel {
		public int LoanID { get; set; }
		public string LoanRefNum { get; set; }
		public DateTime Date { get; set; }
		public decimal LoanAmount { get; set; }
		public decimal InterestRate { get; set; } //percent
		public LoanStatus Status { get; set; }
		public string StatusDescription { get { return Status.DescriptionAttr(); } } //LoanStatus DescriptionAttr (Active/Overdue/Paid) 
		
		public decimal TotalInterest { get { return Schedules.Sum(s => s.Interest) + Fees.Sum(x => x.Amount) + SetupFee; } } 
		public decimal Late { get; set; } //late principal + outstanding interest + fees late charges
		public decimal TotalEarlyPayment { get { return TotalBalance; } } //total outstanding amount per loan
		public decimal NextEarlyPayment { get; set; } //next installment amount due
		public decimal NextInterestPayment { get; set; }//next installment interest
		public decimal Balance {get {return Schedules.Sum(x => x.Principal); }}
		public decimal TotalBalance { get {return Schedules.Sum(x => x.AmountDue); } }

		public decimal RealInterestCost { get {return LoanAmount == 0 ? 0 : TotalInterest / LoanAmount; } } 
		public decimal APR { get; set; } //shown only if IsPersonalTypeOfBusiness
		
		public decimal SetupFee { get; set; } //setup fee amount
		public decimal TotalRepaidAmount { get; set; } //Repayments – total repaid amount

		public bool HasRollover { get; set; } //has rollover
		public bool IsEarly { get{ return Schedules.Where(x => !x.IsPaid).All(x => x.Date > DateTime.UtcNow); }} //can pay early
		public bool IsLate { get { return Late > 0; } } //is late


		public IEnumerable<NewLoanScheduleModel> Schedules { get; set; }
		public IEnumerable<NewLoanFundTransferModel> FundTransfers { get; set; }
		public IEnumerable<NewLoanRepaymentModel> Repayments { get; set; }
		public IEnumerable<NewLoanRolloverModel> Rollovers { get; set; }
		public IEnumerable<NewLoanFeeModel> Fees { get; set; } //Charges 
		public IEnumerable<NewLoanAgreementModel> Agreements { get; set; } //used for download links

		//for UW
		public decimal OfferedAmount { get; set; } // OfferedCreditLine 
		public int RepaymentPeriod { get; set; } //offered repayment period (in months)
		
		public string LoanType { get; set; } // loan type name
		public string LoanSource { get; set; } // loan source name
	}//NewLoanModel

	public class NewLoanAgreementModel {
		public int AgreementID { get; set; }
		public string Name { get; set; } //agreement type name
	}//NewLoanAgreementModel

	public class NewLoanFeeModel {
		public DateTime Date { get; set; } //fee date
		public decimal Amount { get; set; } //fee amount
		public string StatusDescription { get; set; } //State – fee status
	}//NewLoanFeeModel

	public class NewLoanRolloverModel {
		public DateTime ExpireDate { get; set; } //expire date  of rollover
		public decimal Amount { get; set; } //Payment – rollover amount
	}//NewLoanRolloverModel

	public class NewLoanRepaymentModel {
		public DateTime PaymentDate { get; set; }
		public decimal Amount { get; set; }
		public decimal Interest { get; set; }
		public decimal Principal { get; set; } //LoanRepayment 
		public decimal Fees { get; set; } //setup fee
		public decimal Rollover { get; set; } //rollover payment
		public string StatusDescription { get; set; }
	}//NewLoanRepaymentModel

	public class NewLoanFundTransferModel {
		public DateTime FundTransferDate { get; set; }
		public decimal Amount { get; set; }
		public decimal Interest { get { return 0; } } //Don't know if needed
		public decimal Fees { get; set; } //setup fee
		public string StatusDescription { get; set; }
	}//NewLoanFundTransferModel

	public class NewLoanScheduleModel {
		public DateTime Date { get; set; }
		public decimal Principal { get; set; } //LoanRepayment  
		public decimal Interest { get; set; }
		public decimal InterestRate  { get; set; }
		public decimal SetupFee { get; set; } //setup fee amount
		public decimal Fees { get; set; }
		public decimal AmountDue { get; set; } //principal + interest + fees
		public bool IsPaid { get { return AmountDue == 0; } } //in details shown only non paid schedules
	}//NewLoanScheduleModel
}//ns