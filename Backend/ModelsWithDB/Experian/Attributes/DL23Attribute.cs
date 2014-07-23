namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL23Attribute : ASrcAttribute {
		public DL23Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL23", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Ownership Details"; } }

		public override int TargetDisplayPosition { get { return 70; } }
	} // class DL23Attribute
} // namespace
