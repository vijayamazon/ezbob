namespace EZBob.DatabaseLib
{
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.ExperianParser;
	using Model.Database;

	#region class ExperianParserOutput

	public class ExperianParserOutput
	{
		public Dictionary<string, ParsedData> Dataset { get; private set; } // Dataset
		public ParsingResult ParsingResult { get; private set; } // ParsingResult
		public string ErrorMsg { get; private set; } // ErrorMsg
		public string CompanyRefNum { get; private set; } // CompanyRefNum
		public string CompanyName { get; private set; } // CompanyName
		public TypeOfBusinessReduced? TypeOfBusinessReduced { get; private set; } // TypeOfBusinessReduced
		public int? MaxScore { get; private set; }
		public ExperianParserOutput(Dictionary<string, ParsedData> dataset, TypeOfBusinessReduced type)
		{
			Dataset = dataset;
			TypeOfBusinessReduced = type;
		}
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


		public string GetValue(string section, string key)
		{
			if (Dataset.ContainsKey(section))
			{
				if (Dataset[section].Data.Any())
				{
					if (Dataset[section].Data[0].Values.ContainsKey(key))
					{
						return Dataset[section].Data[0].Values[key];
					}
				}
			}
			return null;
		}
	} // class ExperianPar
	#endregion class ExperianParserOutput
} // namespace EZBob.DatabaseLib
