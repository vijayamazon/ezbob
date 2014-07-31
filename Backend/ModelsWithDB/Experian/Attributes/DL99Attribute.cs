namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL99Attribute : ASrcAttribute {
		public DL99Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
		) : base("DL99", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = GroupName,
				};
			}
		} // MetaData

		public override string TargetDisplayGroup { get { return "Limited Company Financial Details IFRS & UK GAAP"; } }

		public override int TargetDisplayPosition { get { return 190; } }
	} // class DL99Attribute
} // namespace
