namespace EzBob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetCompanySeniority : AStrategy {
		#region public

		#region constructor

		public GetCompanySeniority(int nCustomerID, bool isLimited, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpGetCompanySeniority(nCustomerID,isLimited, DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Get company seniority"; }
		} // Name

		#endregion property Name

		#region property CompanyIncorporationDate

		public DateTime? CompanyIncorporationDate { get; private set; }

		#endregion property CompanyIncorporationDate

		#region method Execute

		public override void Execute() {
			CompanyIncorporationDate = m_oSp.ExecuteScalar<DateTime?>();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly SpGetCompanySeniority m_oSp;

		#region class SpGetCompanySeniority

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

		#endregion class SpGetCompanySeniority

		#endregion private
	} // class GetCompanySeniority
} // namespace
