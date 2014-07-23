namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL72Attribute : ASrcAttribute {
		public DL72Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL72", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Current Directorship Details"; } }

		public override int TargetDisplayPosition { get { return 100; } }
	} // class DL72Attribute
} // namespace
