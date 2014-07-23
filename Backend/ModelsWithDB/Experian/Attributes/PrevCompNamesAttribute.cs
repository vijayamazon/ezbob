namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class PrevCompNamesAttribute : ASrcAttribute {
		public PrevCompNamesAttribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL15/PREVCOMPNAMES", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Previous Company Registered Office Address"; } }

		public override int TargetDisplayPosition { get { return 40; } }
	} // class PrevCompNamesAttribute
} // namespace
