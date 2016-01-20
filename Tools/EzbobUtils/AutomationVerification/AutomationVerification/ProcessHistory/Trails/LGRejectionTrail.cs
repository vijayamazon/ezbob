namespace AutomationCalculator.ProcessHistory.Trails {
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using Ezbob.Logger;

	public class LGRejectionTrail : RejectionTrail {
		public LGRejectionTrail(
			int nCustomerID,
			long? cashRequestID,
			long? nlCashRequestID,
			ASafeLog oLog,
			string toExplanationMailAddress = null,
			string fromEmailAddress = null,
			string fromEmailName = null
		) : base(
			nCustomerID,
			cashRequestID,
			nlCashRequestID,
			oLog,
			toExplanationMailAddress,
			fromEmailAddress,
			fromEmailName
		) {
		} // constructor

		public virtual new LGRejectionInputData MyInputData {
			get { return (LGRejectionInputData)base.MyInputData; }
		} // MyInputData

		protected override RejectionInputData CreateMyInputData() {
			return new LGRejectionInputData();
		} // CreateMyInputData
	} // class LGRejectionTrail
} // namespace
