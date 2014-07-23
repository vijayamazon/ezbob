namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL68Attribute : ASrcAttribute {
		public DL68Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL68", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company UK Subsidiaries"; } }

		public override int TargetDisplayPosition { get { return 170; } }
	} // class DL68Attribute
} // namespace
