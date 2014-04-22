namespace EzBob.Web.Models {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.ExperianParser;
	using EZBob.DatabaseLib;

	public class CompanyScoreModel {
		public const string Ok = "ok";

		public string result { get; set; }

		public Dictionary<string, ParsedData> dataset { get; set; }

		public string company_name { get; set; }

		public string company_ref_num { get; set; }

		public CompanyScoreModel[] Owners { get { return ReferenceEquals(m_oOwners, null) ? null : m_oOwners.ToArray(); } }

		public void AddOwner(CompanyScoreModel oOwner) {
			if (ReferenceEquals(m_oSavedOwners, null)) {
				m_oSavedOwners = new SortedSet<string>();
				m_oOwners = new List<CompanyScoreModel>();
			} // if

			if (m_oSavedOwners.Contains(oOwner.company_ref_num))
				return;

			m_oSavedOwners.Add(oOwner.company_ref_num);
			m_oOwners.Add(oOwner);
		} // AddOwner

		private SortedSet<string> m_oSavedOwners;
		private List<CompanyScoreModel> m_oOwners;
	} // CompanyScoreModel

	public class CompanyScoreModelBuilder {
		public CompanyScoreModel Create(Customer customer) {
			ExperianParserOutput oOutput = customer.ParseExperian(ExperianParserFacade.Target.Company);

			CompanyScoreModel oResult = BuildFromParseResult(oOutput);

			if (oResult.result != CompanyScoreModel.Ok)
				return oResult;

			if (oOutput.TypeOfBusinessReduced == TypeOfBusinessReduced.Limited) {
				AddOwners(
					oResult,
					"Limited Company Shareholders",
					"Registered number of a limited company which is a shareholder",
					"Description of Shareholder"
					);

				AddOwners(
					oResult,
					"Limited Company Ownership Details",
					"Registered Number of the Current Ultimate Parent Company",
					"Registered Name of the Current Ultimate Parent Company"
					);
			} // if

			return oResult;
		} // Create

		private void AddOwners(CompanyScoreModel oPossession, string sGroupName, string sCompanyNumberField, string sCompanyNameField) {
			if (oPossession.dataset.ContainsKey(sGroupName)) {
				List<ParsedDataItem> aryShareholders = oPossession.dataset[sGroupName].Data;

				foreach (var oShareholder in aryShareholders) {
					if (oShareholder.ContainsKey(sCompanyNumberField)) {
						var sNumber = oShareholder[sCompanyNumberField];

						if (!string.IsNullOrWhiteSpace(sNumber)) {
							var oOwner = BuildFromParseResult(
								ExperianParserFacade.Invoke(
									sNumber,
									oShareholder[sCompanyNameField] ?? "",
									ExperianParserFacade.Target.Company,
									TypeOfBusinessReduced.Limited
								)
							);

							if (oOwner.result == CompanyScoreModel.Ok) {
								oPossession.AddOwner(oOwner);
							} // if owner was found
						} // if company number is not empty
					} // if owner has a company number
				} // for each owner
			} // if contains list of owners
		} // AddOwners

		private CompanyScoreModel BuildFromParseResult(ExperianParserOutput oResult) {
			switch (oResult.ParsingResult) {
			case ParsingResult.Ok:
				return new CompanyScoreModel {
					result = CompanyScoreModel.Ok,
					dataset = oResult.Dataset,
					company_name = oResult.CompanyName,
					company_ref_num = oResult.CompanyRefNum
				};

			case ParsingResult.Fail:
				return new CompanyScoreModel { result = "Failed to parse Experian response." };

			case ParsingResult.NotFound:
				return new CompanyScoreModel { result = "No data found." };

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // BuildFromParseResult
	} // class CompanyScoreModelBuilder
} // namespace