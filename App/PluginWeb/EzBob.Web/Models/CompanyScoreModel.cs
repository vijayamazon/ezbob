using System;
using System.Collections.Generic;
using EzBob.Configuration;
using EZBob.DatabaseLib.Model.Database;
using Ezbob.ExperianParser;

namespace EzBob.Web.Models {
	public class CompanyScoreModel {
		public const string Ok = "ok";

		public string result { get; set; }

		public Dictionary<string, ParsedData> dataset { get; set; }

		public string company_name { get; set; }

		public string company_ref_num { get; set; }

		public List<CompanyScoreModel> Owners { get; set; }
	} // CompanyScoreModel

	public class CompanyScoreModelBuilder {
		public CompanyScoreModel Create(Customer customer) {
			CompanyScoreModel oResult = BuildFromParseResult(
				customer.ParseExperian(DBConfigurationValues.Instance.CompanyScoreParserConfiguration)
			);

			if (oResult.result != CompanyScoreModel.Ok)
				return oResult;

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

			return oResult;
		} // Create

		private void AddOwners(CompanyScoreModel oPossession, string sGroupName, string sCompanyNumberField, string sCompanyNameField) {
			if (oPossession.dataset.ContainsKey(sGroupName)) {
				List<SortedDictionary<string, string>> aryShareholders = oPossession.dataset[sGroupName].Data;

				foreach (var oShareholder in aryShareholders) {
					if (oShareholder.ContainsKey(sCompanyNumberField)) {
						var sNumber = oShareholder[sCompanyNumberField];

						if (!string.IsNullOrWhiteSpace(sNumber)) {
							var oOwner = BuildFromParseResult(
								Customer.ParseExperian(
									sNumber,
									oShareholder[sCompanyNameField] ?? "",
									DBConfigurationValues.Instance.CompanyScoreParserConfiguration
								)
							);

							if (oOwner.result == CompanyScoreModel.Ok) {
								if (oPossession.Owners == null)
									oPossession.Owners = new List<CompanyScoreModel>();

								oPossession.Owners.Add(oOwner);
							} // if owner was found
						} // if company number is not empty
					} // if owner has a company number
				} // for each owner
			} // if contains list of owners
		} // AddOwners

		private CompanyScoreModel BuildFromParseResult(ParseExperianResult oResult) {
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