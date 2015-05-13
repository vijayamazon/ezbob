namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.Models.NewLoan;
    using Ezbob.Backend.ModelsWithDB.NewLoan;

    public class CreateLoan : AStrategy {
        public CreateLoan(NL_LoanFullModel model) {
            this.model = model;
        }

        public override string Name { get { return "CreateLoan"; } }

        public override void Execute() {
            //TODO

            //AddLoan
            var addLoan = new AddLoan(new NL_Loans {
                /*
                OfferID = ,
                LoanTypeID =,
                RepaymentIntervalTypeID =,
                LoanStatusID =,
                EzbobBankAccountID =,
                LoanSourceID =,
                Position =,
                InitialLoanAmount =,
                CreationTime =,
                IssuedTime =,
                RepaymentCount=, 
                Refnum =,
                DateClosed =,
                InterestRate =,
                InterestOnlyRepaymentCount =,
                OldLoanID =
                */
            });
            addLoan.Execute();
            var loanID = addLoan.LoanID;

            //UpdateLoanLegals
            //todo
            
            //AddLoanOptions
            //var addLoanOptions = new AddLoanOptions(new NL_LoanOptions)

            //AddLoanAgreements
            //todo
            
            //AddFundTransfer
            //todo
            
            //AddFee (setupfee) + broker commission
            //todo
            
            //AddHistory
            //todo
            
            //AddSchedules
            //todo
            
            //AddPayment (of 5 pounds if needed)
            //todo
        }

        private NL_LoanFullModel model;
    }

}
