namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL42Attribute : ASrcAttribute {
		public DL42Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL42", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Directorship Summary"; } }

		public override int TargetDisplayPosition { get { return 110; } }
	} // class DL42Attribute
} // namespace
