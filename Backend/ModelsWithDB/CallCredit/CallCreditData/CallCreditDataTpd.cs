namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataTpd {		
		public CallCreditDataTpd() {
			DecisionAlertIndividuals = new List<CallCreditDataTpdDecisionAlertIndividuals>();			
			DecisionCreditScores = new List<CallCreditDataTpdDecisionCreditScores>();
			HhoCreditScores = new List<CallCreditDataTpdHhoCreditScores>();
			ReviewAlertIndividuals = new List<CallCreditDataTpdReviewAlertIndividuals>();			
					}
		[PK]
		[NonTraversable]
		public long CallCreditDataTpdID { get; set; }
		[FK("CallCreditData", "CallCreditDataID")]
		public long? CallCreditDataID { get; set; }

		public int? TotalD { get; set; }
		public int? TotalR { get; set; }
		public int? Total36mJudgmesntsD { get; set; }
		public int? TotalJudgmesntsD { get; set; }
		public int? TotalActiveAmountJudgmesntsD { get; set; }
		public bool? CurrentlyInsolventD { get; set; }
		public bool? RestrictedD { get; set; }
		[Length(2)]
		public string WorsePayStatus12mD { get; set; }
		[Length(2)]
		public string WorsePayStatus24mD { get; set; }
		public int? TotalDefaultsD { get; set; }
		public int? TotalDefaults12mD { get; set; }
		public int? TotalSettledDefaultsD { get; set; }
		public int? TotalDefaultsAmountD { get; set; }
		public int? TotalWriteoffsD { get; set; }
		public int? TotalWriteoffsAmountD { get; set; }
		public int? TotalDelinqsD { get; set; }
		public int? TotalDelinqsAmountD { get; set; }
		public int? Total36mJudgmesntsR { get; set; }
		public int? TotalJudgmesntsR { get; set; }
		public int? TotalActiveAmountJudgmesntsR { get; set; }
		public bool? CurrentlyInsolventR { get; set; }
		public bool? RestrictedR { get; set; }
		[Length(2)]
		public string WorsePayStatus12mR { get; set; }
		[Length(2)]
		public string WorsePayStatus24mR { get; set; }
		public int? TotalDefaultsR { get; set; }
		public int? TotalDefaults12mR { get; set; }
		public int? TotalSettledDefaultsR { get; set; }
		public int? TotalDefaultsAmountR { get; set; }
		public int? TotalWriteoffsR { get; set; }
		public int? TotalWriteoffsAmountR { get; set; }
		public int? TotalDelinqsR { get; set; }
		public int? TotalDelinqsAmountR { get; set; }
		public bool? ThinFile { get; set; }
		public int? TotalH { get; set; }
		public int? Total36mJudgmentsH { get; set; }
		public int? TotalJudgmentsH { get; set; }
		public int? TotalSatisfiedJudgmesntsH { get; set; }
		public int? TotalActiveAmountJudgmesntsH { get; set; }
		public int? TotalSatisfiedAmountJudgmesntsH { get; set; }
		public bool? CurrentlyInsolventH { get; set; }
		public bool? RestrictedH { get; set; }
		public int? TotalAccountsH { get; set; }
		public int? TotalActiveAccountsH { get; set; }
		public int? TotalActiveAccountsAmountH { get; set; }
		public int? TotalAccountsZerobalH { get; set; }
		public int? TotalSettledAccountsAmountH { get; set; }
		[Length(2)]
		public string WorsePayStatus12mH { get; set; }
		[Length(2)]
		public string WorsePayStatus24mH { get; set; }
		public int? TotalDefaultsH { get; set; }
		public int? TotalDefaults12mH { get; set; }
		public int? TotalDefaultsAmountH { get; set; }
		public int? TotalWriteoffsH { get; set; }
		public int? TotalWriteoffsAmountH { get; set; }
		public int? TotalDelinqsH { get; set; }
		public int? TotalDelinqsAmountH { get; set; }

		public List<CallCreditDataTpdDecisionAlertIndividuals> DecisionAlertIndividuals { get; set; }		
		public List<CallCreditDataTpdDecisionCreditScores> DecisionCreditScores { get; set; }
		public List<CallCreditDataTpdHhoCreditScores> HhoCreditScores { get; set; }
		public List<CallCreditDataTpdReviewAlertIndividuals> ReviewAlertIndividuals { get; set; }		
	}
}
