using System;
using System.Collections.Generic;
using EzBob.Configuration;
using EZBob.DatabaseLib.Model.Database;
using Ezbob.ExperianParser;

namespace EzBob.Web.Models {
	public class CompanyScoreModel {
		public string result { get; set; }

		public Dictionary<string, ParsedData> dataset { get; set; }
	} // CompanyScoreModel

	public class CompanyScoreModelBuilder {
		public CompanyScoreModel Create(Customer customer) {
			Tuple<Dictionary<string, ParsedData>, ParsingResult, string> oResult =
				customer.ParseExperian(DBConfigurationValues.Instance.CompanyScoreParserConfiguration);

			switch (oResult.Item2) {
			case ParsingResult.Ok:
				return new CompanyScoreModel { result = "ok", dataset = oResult.Item1 };

			case ParsingResult.Fail:
				return new CompanyScoreModel { result = "Failed to parse Experian response." };

			case ParsingResult.NotFound:
				return new CompanyScoreModel { result = "No data found." };

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // Create
	} // class CompanyScoreModelBuilder
} // namespace