namespace EzBob.Backend.Strategies {
	using System;
	using System.Xml;
	using Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.XmlUtils;

	public class GetCompanySeniority : AStrategy {
		#region public

		#region constructor

		public GetCompanySeniority(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
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
			string sCompanyDataXml = DB.ExecuteScalar<string>(
				"GetCompanySeniority",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", m_nCustomerID)
			);

			if (!string.IsNullOrWhiteSpace(sCompanyDataXml)) {
				XmlNode companyInfo = Xml.ParseRoot(sCompanyDataXml);

				if (companyInfo != null)
					CompanyIncorporationDate = new ExperianUtils(Log).DetectIncorporationDate(companyInfo);
			} // if
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;

		#endregion private
	} // class GetCompanySeniority
} // namespace
