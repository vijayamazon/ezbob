namespace EzMailChimpCampaigner {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	class SpSelectCustomers : AStoredProcedure {

		public SpSelectCustomers(Constants.CampaignsType nCampaignType, DateTime oDateStart, DateTime oDateEnd, bool bIncludeTest, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			DateStart = oDateStart;
			DateEnd = oDateEnd;
			IncludeTest = bIncludeTest;

			switch (nCampaignType) {
			case Constants.CampaignsType.OnlyRegisteredEmail:
				m_sSpName = Constants.GetFirstStepCustomersSp;
				break;

			case Constants.CampaignsType.OnlyRegisteredStore:
				m_sSpName = Constants.GetSecondStepCustomersSp;
				break;

			case Constants.CampaignsType.DidntTakeLoan:
				m_sSpName = Constants.GetLastStepCustomersSp;
				DateStart = oDateStart.AddDays(-1);
				DateEnd = oDateEnd.AddDays(-1);
				break;

			case Constants.CampaignsType.DidntTakeLoanAlibaba:
				m_sSpName = Constants.GetLastStepCustomersAlibabaSp;
				DateStart = oDateStart.AddDays(-1);
				DateEnd = oDateEnd.AddDays(-1);
				break;

			default:
				throw new NotImplementedException("Not implemented for campaign type " + nCampaignType);
			} // switch
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters

		public DateTime DateStart { get; set; }

		public DateTime DateEnd { get; set; }

		public bool IncludeTest { get; set; }

		protected override string GetName() {
			return m_sSpName;
		} // GetName

		private readonly string m_sSpName;

	} // class SpSelectCustomers

} // namespace EzMailChimpCampaigner
