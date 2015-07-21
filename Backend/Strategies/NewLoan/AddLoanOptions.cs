namespace Ezbob.Backend.Strategies.NewLoan {
    using Ezbob.Backend.Models.NewLoan;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;
    using Ezbob.Utils;

    public class AddLoanOptions : AStrategy {
        public AddLoanOptions(NLLoanOptions loanOptions, int? OldLoanId) {
            this.loanOptions = loanOptions;
            this.oldLoanId = OldLoanId;
        }//constructor

        public override string Name { get { return "AddLoanOptions"; } }

        public override void Execute() {
            if (this.oldLoanId != null)
                this.loanOptions.LoanID = DB.ExecuteScalar<int>("GetNewLoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", this.oldLoanId));

            NL_LoanOptions UpdateOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", this.loanOptions.LoanID));

            //NL_LoanOptions NL_options = new NL_LoanOptions
            //{
            //    AutoCharge = options.AutoPayment,
            //    StopAutoChargeDate = options.StopAutoChargeDate,
            //    AutoLateFees = options.AutoLateFees,
            //    StopAutoLateFeesDate = options.StopLateFeeFromDate,
            //    AutoInterest = false,
            //    StopAutoInterestDate = null,
            //    ReductionFee = options.ReductionFee,
            //    LatePaymentNotification = options.LatePaymentNotification,
            //    CaisAccountStatus = options.CaisAccountStatus,
            //    ManualCaisFlag = options.ManualCaisFlag,
            //    EmailSendingAllowed = options.EmailSendingAllowed,
            //    SmsSendingAllowed = options.SmsSendingAllowed,
            //    MailSendingAllowed = options.MailSendingAllowed,
            //    UserID = this._context.UserId,
            //    InsertDate = DateTime.UtcNow,
            //    IsActive = true,
            //    Notes = null
            //};

            UpdateOptions.Traverse((ignored, pi) =>
            {
                if (pi.GetValue(this.loanOptions) != null)
                    pi.SetValue(UpdateOptions, pi.GetValue(this.loanOptions));
            });

            LoanOptionsID = DB.ExecuteScalar<int>("NL_LoanOptionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanOptions>("Tbl", UpdateOptions), new QueryParameter("@LoanID", this.loanOptions.LoanID)); 
        }//Execute

        public int LoanOptionsID { get; set; }
        private int? oldLoanId { get; set; }
        private readonly NLLoanOptions loanOptions;
    }//class AddLoan
}//ns
