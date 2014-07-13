namespace ExperianLib.Ebusiness {
	using System.Xml.Linq;

	public class NonLimitedResults : BusinessReturnData {

		public bool CompanyNotFoundOnBureau { get; set; }
		
		public override bool IsLimited {
			get { return false; }
		}

		protected override void Parse(XElement root) {
		} // Parse

	} // class NonLimitedResults
} // namespace
