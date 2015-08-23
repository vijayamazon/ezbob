namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using Ezbob.Utils;

	public class AddLoanOptions : AStrategy {
        public AddLoanOptions(NL_LoanOptions loanOptions, int? OldLoanId) {
            this.loanOptions = loanOptions;
            oldLoanId = OldLoanId;
        }//constructor

        public override string Name { get { return "AddLoanOptions"; } }

        public override void Execute() {
	        try {
				
		        if (oldLoanId != null)
			        this.loanOptions.LoanID = DB.ExecuteScalar<long>("GetNewLoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", oldLoanId));

		        NL_LoanOptions existsOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", this.loanOptions.LoanID));

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

				existsOptions.Traverse((ignored, pi) => {
			        if (pi.GetValue(this.loanOptions) != null)
						pi.SetValue(existsOptions, pi.GetValue(this.loanOptions));
		        });

				LoanOptionsID = DB.ExecuteScalar<long>("NL_LoanOptionsSave",
					CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanOptions>("Tbl", existsOptions), 
					new QueryParameter("@LoanID", this.loanOptions.LoanID));
	
		        // ReSharper disable once CatchAllClause
	        } catch (Exception ex) {
		        Log.Alert("Failed to save NL_LoanOptions, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.loanOptions.LoanID, ex);
				Error = string.Format("Failed to save NL_LoanOptions, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.loanOptions.LoanID, ex.Message);
	        }
		}//Execute

        public long LoanOptionsID { get; set; }
		public string Error { get; set; }

		private int? oldLoanId { get; set; }
        private readonly NL_LoanOptions loanOptions;
	}//class AddLoanOptions
}//ns
