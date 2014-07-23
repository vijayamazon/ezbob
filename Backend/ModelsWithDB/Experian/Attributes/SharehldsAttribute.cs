namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class SharehldsAttribute : ASrcAttribute {
		public SharehldsAttribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL23/SHAREHLDS", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = "DL23SHAREHOLDING",
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
					UnlimitedWidth = true,
					Sorting = "Description of Shareholder,Description of Shareholding,% of Shareholding",
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Shareholders"; } }

		public override int TargetDisplayPosition { get { return 80; } }
	} // class SharehldsAttribute
} // namespace
