using System;
using EzBobIntegration.Web_References.Consumer;

namespace ExperianLib
{
	using System.Globalization;
	using EZBob.DatabaseLib.Model.Experian;
	using Newtonsoft.Json;

	public class ConsumerExperianModelBuilder
	{
		private string Errors { get; set; }
		private bool HasParsingError { get; set; }

		public ExperianConsumerData Build(OutputRoot outputRoot, int? customerId = null, int? directorId = null, DateTime? insertDate = null, long serviceLogId = 0)
		{
			var data = new ExperianConsumerData();
			data.ServiceLogId = serviceLogId;
			data.CustomerId = customerId;
			data.DirectorId = directorId;
			data.InsertDate = insertDate.HasValue ? insertDate.Value : new DateTime();

			if (outputRoot.Output.Error != null)
			{
				Errors = string.Format("Error from service with code: {0}, severity: {1}, message: {2} \n", outputRoot.Output.Error.ErrorCode, outputRoot.Output.Error.Severity, outputRoot.Output.Error.Message);
				data.HasExperianError = true;
				data.Error = Errors;
				return data;
			}

			var applicant = outputRoot.Output.Applicant != null && outputRoot.Output.Applicant.Length > 0
				? outputRoot.Output.Applicant[0] : null;

			if (applicant != null)
			{
				TryRead(() => data.ApplicantIdentifier = applicant.ApplicantIdentifier, "ApplicantIdentifier");
				TryRead(() => data.Title = applicant.Name.Title, "Title");
				TryRead(() => data.Forename = applicant.Name.Forename, "Forename");
				TryRead(() => data.MiddleName = applicant.Name.MiddleName, "MiddleName");
				TryRead(() => data.Surname = applicant.Name.Surname, "Surname");
				TryRead(() => data.Suffix = applicant.Name.Suffix, "Suffix");
				TryRead(() => data.DateOfBirth = new DateTime(
					int.Parse(applicant.DateOfBirth.CCYY.ToString(CultureInfo.InvariantCulture)),
					int.Parse(applicant.DateOfBirth.MM.ToString(CultureInfo.InvariantCulture)),
					int.Parse(applicant.DateOfBirth.DD.ToString(CultureInfo.InvariantCulture))), "DateOfBirth");
				TryRead(() => data.Gender = applicant.Gender, "Gender");
			}

			if (outputRoot.Output.Applicant != null && outputRoot.Output.Applicant.Length > 1)
			{
				Errors += "More than one applicant specified" + Environment.NewLine;
			}

			TryRead(() => data.BureauScore = Convert.ToInt32(outputRoot.Output.ConsumerSummary.PremiumValueData.Scoring.E5S051),"BureauScore");
			TryRead(() => data.CreditCardBalances = Convert.ToInt32(outputRoot.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPA04), "CreditCardBalances");
			TryRead(() => data.ActiveCaisBalanceExcMortgages = Convert.ToInt32(outputRoot.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPA02), "ActiveCaisBalanceExcMortgages");
			TryRead(() => data.NumCreditCards = Convert.ToInt32(outputRoot.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.APACSCCBehavrlData.NOMPMNPRL3M), "NumCreditCards");
			TryRead(() => data.CreditLimitUtilisation = Convert.ToInt32(outputRoot.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.APACSCCBehavrlData.CLUNPRL1M), "CreditLimitUtilisation");
			TryRead(() => data.CreditCardOverLimit = Convert.ToInt32(outputRoot.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPF131), "CreditCardOverLimit");
			TryRead(() => data.PersonalLoanStatus = outputRoot.Output.ConsumerSummary.Summary.CAIS.NDHAC05, "PersonalLoanStatus");
			TryRead(() => data.WorstStatus = outputRoot.Output.ConsumerSummary.Summary.CAIS.NDHAC09, "WorstStatus");
			TryRead(() => data.WorstCurrentStatus = outputRoot.Output.FullConsumerData.ConsumerDataSummary.SummaryDetails.CAISSummary.WorstCurrent, "WorstCurrentStatus");
			TryRead(() => data.WorstHistoricalStatus = outputRoot.Output.FullConsumerData.ConsumerDataSummary.SummaryDetails.CAISSummary.WorstHistorical, "WorstHistoricalStatus");

			TryRead(() => data.TotalAccountBalances = Convert.ToInt32(outputRoot.Output.ConsumerSummary.Summary.CAIS.E1B10), "TotalAccountBalances");
			TryRead(() => data.NumAccounts = Convert.ToInt32(outputRoot.Output.ConsumerSummary.Summary.CAIS.E1B01), "NumAccounts");
			TryRead(() => data.NumCCJs = Convert.ToInt32(outputRoot.Output.ConsumerSummary.Summary.PublicInfo.E1A01), "NumCCJs");

			TryRead(() => data.CCJLast2Years = Convert.ToInt32(outputRoot.Output.ConsumerSummary.Summary.PublicInfo.E1A03), "CCJLast2Years");
			TryRead(() => data.TotalCCJValue1 = Convert.ToInt32(outputRoot.Output.ConsumerSummary.Summary.PublicInfo.E1A02), "TotalCCJValue1");
			TryRead(() => data.TotalCCJValue2 = Convert.ToInt32(outputRoot.Output.ConsumerSummary.Summary.PublicInfo.E2G02), "TotalCCJValue2");
			TryRead(() => data.EnquiriesLast6Months = Convert.ToInt32(outputRoot.Output.ConsumerSummary.Summary.CAPS.E1E02), "EnquiriesLast6Months");
			TryRead(() => data.EnquiriesLast3Months = Convert.ToInt32(outputRoot.Output.ConsumerSummary.Summary.CAPS.E1E01), "EnquiriesLast3Months");
			TryRead(() => data.MortgageBalance = Convert.ToInt32(outputRoot.Output.ConsumerSummary.Summary.CAIS.E1B11), "MortgageBalance");
			TryRead(() =>
				{
					if (null != outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04)
					{
						data.CaisDOB = new DateTime(1900 + outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04.YY,
						                            outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04.MM,
						                            outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04.DD);
					}
					else if (null != outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB)
					{
						data.CaisDOB = new DateTime(1900 + outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB.YY,
						                            outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB.MM,
						                            outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB.DD);
					}
				} , "CaisDOB");
			TryRead(() => data.CreditCommitmentsRevolving = Convert.ToInt32(outputRoot.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPH39), "CreditCommitmentsRevolving");
			TryRead(() => data.CreditCommitmentsNonRevolving = Convert.ToInt32(outputRoot.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPH40), "CreditCommitmentsNonRevolving");
			TryRead(() => data.MortgagePayments = Convert.ToInt32(outputRoot.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPH41), "MortgagePayments");
			TryRead(() => data.Bankruptcy = (outputRoot.Output.ConsumerSummary.Summary.PublicInfo.EA1C01 == "Y"), "Bankruptcy");
			TryRead(() => data.OtherBankruptcy = (outputRoot.Output.ConsumerSummary.Summary.PublicInfo.EA2I01 == "Y"), "OtherBankruptcy");
			TryRead(() => data.CAISDefaults = Convert.ToInt32(outputRoot.Output.ConsumerSummary.Summary.CAIS.E1A05), "CAISDefaults");
			TryRead(() => data.BadDebt = outputRoot.Output.ConsumerSummary.Summary.CAIS.E1B08, "BadDebt");
			TryRead(() => data.NOCsOnCCJ = (outputRoot.Output.ConsumerSummary.Summary.NOC.EA4Q02 == "Y"), "NOCsOnCCJ");
			TryRead(() => data.NOCsOnCAIS = (outputRoot.Output.ConsumerSummary.Summary.NOC.EA4Q04 == "Y"), "NOCsOnCAIS");
			TryRead(() => data.NOCAndNOD = (outputRoot.Output.ConsumerSummary.Summary.NOC.EA4Q05 == "Y"), "NOCAndNOD");
			TryRead(() => data.SatisfiedJudgement = (outputRoot.Output.ConsumerSummary.Summary.PublicInfo.EA4Q06 == "Y"), "SatisfiedJudgement");
			TryRead(() => data.CII = Convert.ToInt32(outputRoot.Output.ConsumerSummary.PremiumValueData.CII.NDSPCII), "CII");
			TryRead(() => data.CAISSpecialInstructionFlag = outputRoot.Output.ConsumerSummary.Summary.CAIS.EA1F04, "CAISSpecialInstructionFlag");

			data.Error = Errors;
			data.HasParsingError = HasParsingError;
			Console.WriteLine(JsonConvert.SerializeObject(data, new JsonSerializerSettings{Formatting = Formatting.Indented}));
			return data;

		}

		private void TryRead(Action a, string key)
		{
			try
			{
				a();
			}
			catch
			{
				HasParsingError = true;
				Errors += "Can`t read value for: " + key + Environment.NewLine;
			}
		}

	}

	public class ConsumerServiceResult
	{
		public string ExperianResult { get; set; }//todo remove
		public DateTime LastUpdateDate { get; set; }//todo remove

		public ExperianConsumerData Data{get;set; }
		public OutputRoot Output { get; private set; }

		public double SumOfRepayements { get; set; }

		public ConsumerServiceResult()
		{
		}

		//-----------------------------------------------------------------------------------
		public ConsumerServiceResult(OutputRoot outputRoot)
		{
			Output = outputRoot;
			var builder = new ConsumerExperianModelBuilder();
			Data = builder.Build(outputRoot);
			SumOfRepayements = Data.CreditCommitmentsNonRevolving + Data.CreditCommitmentsRevolving + Data.MortgagePayments;
		}
	}
}
