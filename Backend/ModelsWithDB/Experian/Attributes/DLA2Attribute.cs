namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DLA2Attribute : ASrcAttribute {
		public DLA2Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DLA2", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Standard Ratios Details"; } }

		public override int TargetDisplayPosition { get { return 200; } }
	} // class DLA2Attribute
} // namespace
