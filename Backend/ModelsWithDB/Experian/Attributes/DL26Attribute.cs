namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL26Attribute : ASrcAttribute {
		public DL26Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL26", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company CCJ Summary"; } }

		public override int TargetDisplayPosition { get { return 120; } }
	} // class DL26Attribute
} // namespace
