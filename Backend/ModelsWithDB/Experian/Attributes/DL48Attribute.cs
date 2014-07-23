namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL48Attribute : ASrcAttribute {
		public DL48Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL48", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company CIFAS Details"; } }

		public override int TargetDisplayPosition { get { return 150; } }
	} // class DL48Attribute
} // namespace
