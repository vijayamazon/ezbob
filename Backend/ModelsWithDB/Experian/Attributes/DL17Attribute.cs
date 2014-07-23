namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL17Attribute : ASrcAttribute {
		public DL17Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL17", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Bank Details"; } }

		public override int TargetDisplayPosition { get { return 60; } }
	} // class DL17Attribute
} // namespace
