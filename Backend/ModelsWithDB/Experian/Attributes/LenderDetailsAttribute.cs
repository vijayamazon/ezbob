namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	public class LenderDetailsAttribute : ASrcAttribute {
		public LenderDetailsAttribute(
			string sNodeName = null,
			string sTargetDisplayName = null,
			string oMap = null
			) : base("LENDERDETAILS", sNodeName, sTargetDisplayName, oMap) {}

		public override bool IsTopLevel { get { return false; } }

		public override string TargetDisplayGroup { get { return "Limited Company Mortgages Details"; } }
	} // class LenderDetailsAttribute
} // namespace
