namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class ERR1Attribute : ASrcAttribute {
		public ERR1Attribute(string sNodeName = null) : base("ERR1", sNodeName, null, null) {}

		public override bool IsCompanyScoreModel {
			get { return false; }
			set { }
		} // IsCompanyScoreModel
	} // class ERR1Attribute
} // namespace
