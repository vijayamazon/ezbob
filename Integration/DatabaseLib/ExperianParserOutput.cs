namespace EZBob.DatabaseLib {
	using System.Collections.Generic;
	using Ezbob.ExperianParser;
	using Model.Database;

	#region class ExperianParserOutput

	public class ExperianParserOutput {
		public Dictionary<string, ParsedData> Dataset { get; private set;} // Dataset
		public ParsingResult ParsingResult { get; private set; } // ParsingResult
		public string ErrorMsg { get; private set; } // ErrorMsg
		public string CompanyRefNum { get; private set; } // CompanyRefNum
		public string CompanyName { get; private set; } // CompanyName
		public TypeOfBusinessReduced? TypeOfBusinessReduced { get; private set; } // TypeOfBusinessReduced
		public int? MaxScore { get; private set; }
		public ExperianParserOutput(
			Dictionary<string, ParsedData> dataset,
			ParsingResult parsingResult,
			string errorMsg,
			string companyRefNum,
			string companyName,
			TypeOfBusinessReduced? nTypeOfBusiness,
			int? maxScore = null)
		{
			Dataset = dataset;
			ParsingResult = parsingResult;
			ErrorMsg = errorMsg;
			CompanyRefNum = companyRefNum;
			CompanyName = companyName;
			TypeOfBusinessReduced = nTypeOfBusiness;
			MaxScore = maxScore;
		} // constructor
	} // class ExperianParserOutput

	#endregion class ExperianParserOutput
} // namespace EZBob.DatabaseLib
