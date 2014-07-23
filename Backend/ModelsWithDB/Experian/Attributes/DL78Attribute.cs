namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL78Attribute : ASrcAttribute {
		public DL78Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL78", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Commercial Delphi Credit Limit"; } }

		public override int TargetDisplayPosition { get { return 30; } }
	} // class DL78Attribute
} // namespace
