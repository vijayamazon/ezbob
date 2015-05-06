namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class DL79Attribute : ASrcAttribute {
		public DL79Attribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("DL79", sNodeName, sTargetDisplayName, oMap) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData

        public override string TargetDisplayGroup { get { return "Limited company Commercial Delphi credit text"; } }

		public override int TargetDisplayPosition { get { return 35; } }
	} // class DL79Attribute
} // namespace
