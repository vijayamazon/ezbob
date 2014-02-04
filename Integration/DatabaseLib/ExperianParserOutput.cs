namespace EZBob.DatabaseLib {
	using System;
	using System.Collections.Generic;
	using Ezbob.ExperianParser;
	using Model.Database;

	#region class ExperianParserOutput

	public class ExperianParserOutput : Tuple<Dictionary<string, ParsedData>, ParsingResult, string, string, string, TypeOfBusinessReduced?> {
		public Dictionary<string, ParsedData> Dataset { get { return Item1; } } // Dataset
		public ParsingResult ParsingResult { get { return Item2; } } // ParsingResult
		public string ErrorMsg { get { return Item3; } } // ErrorMsg
		public string CompanyRefNum { get { return Item4; } } // CompanyRefNum
		public string CompanyName { get { return Item5; } } // CompanyName
		public TypeOfBusinessReduced? TypeOfBusinessReduced { get { return Item6; } } // TypeOfBusinessReduced

		public ExperianParserOutput(
			Dictionary<string, ParsedData> dataset,
			ParsingResult parsingResult,
			string errorMsg,
			string companyRefNum,
			string companyName,
			TypeOfBusinessReduced? nTypeOfBusiness
		)
			: base(dataset, parsingResult, errorMsg, companyRefNum, companyName, nTypeOfBusiness) { } // constructor
	} // class ExperianParserOutput

	#endregion class ExperianParserOutput
} // namespace EZBob.DatabaseLib
