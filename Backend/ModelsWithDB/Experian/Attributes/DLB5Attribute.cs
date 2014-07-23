namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DLB5Attribute : ASrcAttribute {
		public DLB5Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DLB5", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = GroupName,
					UnlimitedWidth = true,
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Shareholders Details"; } }

		public override int TargetDisplayPosition { get { return 90; } }
	} // class DLB5Attribute
} // namespace
