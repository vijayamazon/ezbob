namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL52Attribute : ASrcAttribute {
		public DL52Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL52", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company 652/3 Notices"; } }

		public override int TargetDisplayPosition { get { return 160; } }
	} // class DL52Attribute
} // namespace
