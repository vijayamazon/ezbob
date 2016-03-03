namespace Ezbob.Backend.Strategies.NewLoan.Migration {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using StructureMap;

	public class LoanLegalsAgreement : AStrategy {

		public LoanLegalsAgreement() {
			legalRep = ObjectFactory.GetInstance<LoanLegalRepository>();
		}

		public override string Name { get { return "LoanLegalsAgreement"; } }
		public string Error { get; private set; }
		public LoanLegalRepository legalRep { get; private set; }
		//public LoanRepository loanRep { get; private set; }


		public override void Execute() {
			
			NL_AddLog(LogType.Info, "Started", "NL_MigrationLoanLegals", null, null, null);
			try {
				List<MigrationModels.CashReqModel> legalsList = DB.Fill<MigrationModels.CashReqModel>("NL_MigrationLoanLegals", CommandSpecies.StoredProcedure);
				int copiedRecords = 0;
				
				foreach (var cashReqModel in legalsList) {
					
					LoanLegal legals = legalRep.Get(cashReqModel.LegalID);

					NL_LoanLegals nlLegals = new NL_LoanLegals() {
						//Amount = legals.
						//RepaymentPeriod = legals.
						OfferID = cashReqModel.OfferID,
						SignatureTime = legals.Created,
						CreditActAgreementAgreed = legals.CreditActAgreementAgreed,
						PreContractAgreementAgreed = legals.PreContractAgreementAgreed,
						PrivateCompanyLoanAgreementAgreed = legals.PrivateCompanyLoanAgreementAgreed,
						GuarantyAgreementAgreed = legals.GuarantyAgreementAgreed,
						EUAgreementAgreed = legals.EUAgreementAgreed,
						COSMEAgreementAgreed = legals.COSMEAgreementAgreed,
						NotInBankruptcy = legals.NotInBankruptcy,
						SignedName = legals.SignedName,
						SignedLegalDocs = legals.SignedLegalDocs,
						BrokerSetupFeePercent = legals.BrokerSetupFeePercent
					};

					copiedRecords++;
				}
				
				NL_AddLog(LogType.Info, "Ended", null, "copied records: " + copiedRecords, null, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception exc) {

				Error = exc.Message;
				Log.Alert(exc);
				NL_AddLog(LogType.Error, "Strategy failed", null, Error, exc.ToString(), exc.StackTrace);
			}

		} //Execute


		


	
	}
}//ns