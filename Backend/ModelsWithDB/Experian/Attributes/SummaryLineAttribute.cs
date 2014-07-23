namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class SummaryLineAttribute : ASrcAttribute {
		public SummaryLineAttribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL27/SUMMARYLINE", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Credit Summary"; } }

		public override int TargetDisplayPosition { get { return 130; } }
	} // class SummaryLineAttribute
} // namespace
