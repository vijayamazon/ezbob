﻿namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using SalesForceLib.Models;

	/// <summary>
	/// If loan fully paid - send mail to customer
	/// If loan fully paid and wasn't late/default - add sf opportunity - finish loan
	/// If loan 50% repaid and wasn't late/default - add sf opportunity - 50% paid
	/// </summary>
	public class LoanStatusAfterPayment : AStrategy {
		public LoanStatusAfterPayment(int customerID, string customerEmail, int loanID, decimal paymentAmount, bool sendMail) {
			this.customerID = customerID;
			this.customerEmail = customerEmail;
			this.loanID = loanID;
			this.paymentAmount = paymentAmount;
			this.sendMail = sendMail;
		}

		public override string Name { get { return "LoanStatusAfterPayment"; } }

		public override void Execute() {
			SafeReader sr = DB.GetFirst(
					"GetLoanStatus",
					CommandSpecies.StoredProcedure,
					new QueryParameter("LoanId", this.loanID)
				);

			string loanStatus = sr["Status"];
			bool wasLate = sr["WasLate"];
			decimal loanAmount = sr["LoanAmount"];
			decimal balance = sr["Balance"];
			string loanRefNum = sr["RefNum"];

			if (loanStatus == "PaidOff") {
				if (!wasLate) {
					SalesForce.AddOpportunity addOpportunity = new AddOpportunity(this.customerID, new OpportunityModel {
						CreateDate = DateTime.UtcNow,
						Email = this.customerEmail,
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
				decimal repaidPercent = loanAmount == 0 ? 0 : (loanAmount - balance) / loanAmount;
				decimal repaidPercentBeforePayment = loanAmount == 0 ? 0 : (loanAmount - balance - this.paymentAmount) / loanAmount;
				const decimal fiftyPercent = 0.5M;
				if (repaidPercent > fiftyPercent && repaidPercentBeforePayment < fiftyPercent && !wasLate) {
					AddSalesForceFiftyPercentOpportunity();
				}//if
			}//if
		}

		private void AddSalesForceFiftyPercentOpportunity() {
			SalesForce.AddOpportunity addOpportunity = new AddOpportunity(this.customerID, new OpportunityModel {
				CreateDate = DateTime.UtcNow,
				Email = this.customerEmail,
				Stage = OpportunityStage.s5.DescriptionAttr(),
				Type = OpportunityType.FiftyPercentRepaid.DescriptionAttr(),
				Name = this.customerEmail + OpportunityType.FiftyPercentRepaid.DescriptionAttr()
			});
			addOpportunity.Execute();
		}//AddSalesForceFiftyPercentOpportunity

		private readonly int customerID;
		private readonly string customerEmail;
		private readonly int loanID;
		private readonly decimal paymentAmount;
		private readonly bool sendMail;
	}
}
