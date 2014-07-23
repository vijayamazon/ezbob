namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class CaisMonthlyAttribute : ASrcAttribute {
		public CaisMonthlyAttribute(string sNodeName = null) : base("DL95/MONTHLYDATA", sNodeName, null, null) {}

		public override bool IsCompanyScoreModel {
			get { return false; }
			set { }
		} // IsCompanyScoreModel
	} // class CaisMonthlyAttribute
} // namespace
