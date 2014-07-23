namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL97Attribute : ASrcAttribute {
		public DL97Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL97", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
					ID = GroupName,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Instalment CAIS Details"; } }

		public override int TargetDisplayPosition { get { return 180; } }
	} // class DL97Attribute
} // namespace
