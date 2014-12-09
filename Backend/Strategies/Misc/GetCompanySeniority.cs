namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetCompanySeniority : AStrategy {

		public GetCompanySeniority(int nCustomerID, bool isLimited) {
			m_oSp = new SpGetCompanySeniority(nCustomerID, isLimited, DB, Log);
		} // constructor

		public override string Name {
			get { return "Get company seniority"; }
		} // Name

		public DateTime? CompanyIncorporationDate { get; private set; }

		public override void Execute() {
			CompanyIncorporationDate = m_oSp.ExecuteScalar<DateTime?>();
		} // Execute

		private readonly SpGetCompanySeniority m_oSp;

		private class SpGetCompanySeniority : AStoredProc {
			public SpGetCompanySeniority(int nCustomerID, bool isLimited, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerID = nCustomerID;
				IsLimited = isLimited;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerID > 0;
			} // HasValidParameters

			public int CustomerID { get; set; }
			public bool IsLimited { get; set; }
		} // class SpGetCompanySeniority

	} // class GetCompanySeniority
} // namespace
