namespace Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	public class CallCreditDataTpdReviewAlertIndividuals {
		public CallCreditDataTpdReviewAlertIndividuals() {
			ReviewAlertIndividualNocs = new List<CallCreditDataTpdReviewAlertIndividualsNocs>();
		}
		[PK]
		[NonTraversable]
		public long CallCreditDataTpdReviewAlertIndividualsID { get; set; }
		[FK("CallCreditDataTpd", "CallCreditDataTpdID")]
		public long? CallCreditDataTpdID { get; set; }

		[Length(164)]
		public string IndividualName { get; set; }

		[NonTraversable]
		public List<CallCreditDataTpdReviewAlertIndividualsNocs> ReviewAlertIndividualNocs { get; set; }
	}
}
