namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataTpdDecisionAlertIndividuals {
		public CallCreditDataTpdDecisionAlertIndividuals() {
			DecisionAlertIndividualNocs = new List<CallCreditDataTpdDecisionAlertIndividualsNocs>();
		}

		[PK]
		[NonTraversable]
		public long CallCreditDataTpdDecisionAlertIndividualsID { get; set; }
		[FK("CallCreditDataTpd", "CallCreditDataTpdID")]
		public long? CallCreditDataTpdID { get; set; }

		[Length(164)]
		public string IndividualName { get; set; }

		public List<CallCreditDataTpdDecisionAlertIndividualsNocs> DecisionAlertIndividualNocs { get; set; }

	}
}
