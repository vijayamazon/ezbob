namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL76Attribute : ASrcAttribute {
		public DL76Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL76", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Commercial Delphi Score"; } }

		public override int TargetDisplayPosition { get { return 20; } }
	} // class DL76Attribute
} // namespace
