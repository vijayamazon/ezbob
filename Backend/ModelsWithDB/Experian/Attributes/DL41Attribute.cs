namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL41Attribute : ASrcAttribute {
		public DL41Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL41", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = GroupName,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Payment Performance Details"; } }

		public override int TargetDisplayPosition { get { return 140; } }
	} // class DL41Attribute
} // namespace
