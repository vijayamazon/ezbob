namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Runtime.CompilerServices;
	using ConfigManager;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using SalesForceLib.Models;

	/// <summary>
	/// If loan fully paid - send mail to customer
	/// If loan fully paid and wasn't late/default and has less than 2 (conf) loans and loan more than 5 months old - add sf opportunity - finish loan
	/// If loan 50% repaid and wasn't late/default and has less than 2 (conf) loans and loan more than 5 months old - add sf opportunity - 50% paid
	/// </summary>
	public class LoanStatusAfterPayment : AStrategy {
        public LoanStatusAfterPayment(int customerID, string customerEmail, int loanID, decimal paymentAmount, bool sendMail, decimal? balance = null, bool? isPaidOff = null)
        {
			this.customerID = customerID;
			this.customerEmail = customerEmail;
			this.loanID = loanID;
			this.paymentAmount = paymentAmount;
			this.sendMail = sendMail;
            this.balance = balance;
            this.isPaidOff = isPaidOff;
		}//constructor

		public override string Name { get { return "LoanStatusAfterPayment"; } }

		public override void Execute() {
			SafeReader sr = DB.GetFirst(
					"GetLoanStatus",
					CommandSpecies.StoredProcedure,
					new QueryParameter("LoanId", this.loanID)
				);

            //TODO in the futuere replace with this call.
            //var numOfActiveLoans = DB.ExecuteScalar<long>(
            //"NL_GetNumOfActiveLoans",
            //CommandSpecies.StoredProcedure,
            //new QueryParameter("CustomerID", this.customerID));

            //SafeReader sr = DB.GetFirst(
            //    "NL_LoanStatusGet",
            //    CommandSpecies.StoredProcedure,
            //    new QueryParameter("LoanId", this.loanID)
            //);

		    if ((this.balance == null) || (this.isPaidOff == null)) {
                this.balance = sr["Balance"];
                string loanStatus = sr["Status"];
                this.isPaidOff = (loanStatus == "PaidOff");
		    }
            
			bool wasLate = sr["WasLate"];
			decimal loanAmount = sr["LoanAmount"];
			string loanRefNum = sr["RefNum"];
			int numOfActiveLoans = sr["NumOfActiveLoans"];
			DateTime loanDate = sr["LoanDate"];
			DateTime now = DateTime.UtcNow;
			double monthsSinceLoanWasTaken = (now - loanDate).TotalDays / (365.0 / 12.0);
			this.origin = sr["Origin"];
			Log.Info("LoanStatusAfterPayment customer {0}, loan {1}, is paid off {2}, loan amount {3}, balance {4}, paid {5}, was late {6}, numOfActiveLoans {7}, monthsSinceLoanWasTaken {8}",
				this.customerID, this.loanID, this.isPaidOff, loanAmount, this.balance, this.paymentAmount, wasLate, numOfActiveLoans, monthsSinceLoanWasTaken);

			if ((bool)this.isPaidOff) {
				if (!wasLate && 
					numOfActiveLoans < CurrentValues.Instance.NumofAllowedActiveLoans && 
					monthsSinceLoanWasTaken > CurrentValues.Instance.MinLoanLifetimeMonths) {
					SalesForce.AddOpportunity addOpportunity = new AddOpportunity(this.customerID, new OpportunityModel {
						CreateDate = DateTime.UtcNow,
						Email = this.customerEmail,
						Origin = this.origin,
						Stage = OpportunityStage.s5.DescriptionAttr(),
						Type = OpportunityType.FinishLoan.DescriptionAttr(),
						Name = this.customerEmail + OpportunityType.FinishLoan.DescriptionAttr()
					});
					addOpportunity.Execute();
				}

				if (this.sendMail) {
					LoanFullyPaid loanFullyPaid = new LoanFullyPaid(this.customerID, loanRefNum);
					loanFullyPaid.Execute();
				}
			} else {
				decimal repaidPercent = (decimal)(loanAmount == 0 ? 0 : (loanAmount - this.balance) / loanAmount);
				decimal repaidPercentBeforePayment = (decimal)(loanAmount == 0 ? 0 : (loanAmount - this.balance - this.paymentAmount) / loanAmount);
				const decimal fiftyPercent = 0.5M;
				if (repaidPercent >= fiftyPercent && 
					repaidPercentBeforePayment < fiftyPercent && 
					!wasLate &&
					numOfActiveLoans < CurrentValues.Instance.NumofAllowedActiveLoans && 
					monthsSinceLoanWasTaken > CurrentValues.Instance.MinLoanLifetimeMonths) {
					AddSalesForceFiftyPercentOpportunity();
				}//if
			}//if
		}//Execute

		private void AddSalesForceFiftyPercentOpportunity() {
			SalesForce.AddOpportunity addOpportunity = new AddOpportunity(this.customerID, new OpportunityModel {
				CreateDate = DateTime.UtcNow,
				Email = this.customerEmail,
				Origin = this.origin,
				Stage = OpportunityStage.s5.DescriptionAttr(),
				Type = OpportunityType.FiftyPercentRepaid.DescriptionAttr(),
				Name = this.customerEmail + OpportunityType.FiftyPercentRepaid.DescriptionAttr()
			});
			addOpportunity.Execute();
		}//AddSalesForceFiftyPercentOpportunity

		private readonly int customerID;
		private readonly string customerEmail;
		private string origin;
		private readonly int loanID;
		private readonly decimal paymentAmount;
		private decimal? balance;
		private bool? isPaidOff;
		private readonly bool sendMail;
	}//class LoanStatusAfterPayment
}//ns
