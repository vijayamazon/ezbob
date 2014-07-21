namespace ExperianLib.Ebusiness {
	using System;

	public class NonLimitedResults : BusinessReturnData {
		public NonLimitedResults(string sError, decimal nBureauScore) : base(sError, nBureauScore) {}

		public NonLimitedResults(Exception e) : base(e) {}

		public bool CompanyNotFoundOnBureau { get { return !string.IsNullOrEmpty(Error); } }

		public override bool IsLimited { get { return false; } }
	} // class NonLimitedResults
} // namespace
