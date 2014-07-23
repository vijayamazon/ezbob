namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL13Attribute : ASrcAttribute {
		public DL13Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL13", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Details"; } }

		public override int TargetDisplayPosition { get { return 50; } }
	} // class DL13Attributes
} // namespace
