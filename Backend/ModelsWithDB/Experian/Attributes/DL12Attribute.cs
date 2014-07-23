namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL12Attribute : ASrcAttribute {
		public DL12Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL12", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = GroupName,
					UnlimitedWidth = true,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Identification"; } }

		public override int TargetDisplayPosition { get { return 10; } }
	} // class DL12Attribute
} // namespace
