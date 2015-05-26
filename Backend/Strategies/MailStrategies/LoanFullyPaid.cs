namespace Ezbob.Backend.Strategies.MailStrategies {
    using System;
    using System.Collections.Generic;
	using Ezbob.Backend.Strategies.SalesForce;
    using Ezbob.Utils.Extensions;
    using SalesForceLib.Models;

    public class LoanFullyPaid : AMailStrategyBase {

		public LoanFullyPaid(int customerId, string loanRefNum, bool wasLate) : base(customerId, true) {
			this.loanRefNum = loanRefNum;
			this.wasLate = wasLate;
		} // constructor

		public override string Name { get { return "Loan Fully Paid"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Loan paid in full";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"RefNum", this.loanRefNum}
			};
		} // SetTemplateAndVariables

	    protected override void ActionAtEnd() {
		    if (!this.wasLate) {
			    SalesForce.AddOpportunity addOpportunity = new AddOpportunity(CustomerData.Id, new OpportunityModel {
				    CreateDate = DateTime.UtcNow,
				    Email = CustomerData.Mail,
				    Stage = OpportunityStage.s5.DescriptionAttr(),
				    Type = OpportunityType.FinishLoan.DescriptionAttr(),
				    Name = CustomerData.FirstName + OpportunityType.FinishLoan.DescriptionAttr()
			    });
			    addOpportunity.Execute();
		    }
	    }

	    private readonly string loanRefNum;
	    private readonly bool wasLate;
    } // class LoanFullyPaid
} // namespace
