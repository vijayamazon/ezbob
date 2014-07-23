namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL65Attribute : ASrcAttribute {
		public DL65Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL65", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = GroupName,
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
					UnlimitedWidth = true,
					Sorting = "Alterations to the order,Total Amount of Debenture Secured,*",
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Mortgages"; } }

		public override int TargetDisplayPosition { get { return 210; } }
	} // class DL65Attribute
} // namespace
