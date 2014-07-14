using System;
using EzBobIntegration.Web_References.Consumer;

namespace ExperianLib
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Experian;
	using Newtonsoft.Json;

	public class ConsumerExperianModelBuilder
	{
		private string Errors { get; set; }
		private bool HasParsingError { get; set; }

		public ExperianConsumerData Build(OutputRoot outputRoot, int? customerId = null, int? directorId = null, DateTime? insertDate = null, long serviceLogId = 0)
		{
			var data = new ExperianConsumerData
				{
					Applicants = new List<ExperianConsumerDataApplicant>(),
					Cais = new List<ExperianConsumerDataCais>()
				};

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


			foreach (var applicant in outputRoot.Output.Applicant)
			{
				TryRead(() =>
					{
						var app = new ExperianConsumerDataApplicant();
						TryRead(() => app.ApplicantIdentifier = applicant.ApplicantIdentifier, "ApplicantIdentifier");
						TryRead(() => app.Title = applicant.Name.Title, "Title");
						TryRead(() => app.Forename = applicant.Name.Forename, "Forename");
						TryRead(() => app.MiddleName = applicant.Name.MiddleName, "MiddleName");
						TryRead(() => app.Surname = applicant.Name.Surname, "Surname");
						TryRead(() => app.Suffix = applicant.Name.Suffix, "Suffix");
						TryRead(() => app.DateOfBirth = new DateTime(
															int.Parse(applicant.DateOfBirth.CCYY.ToString(CultureInfo.InvariantCulture)),
															int.Parse(applicant.DateOfBirth.MM.ToString(CultureInfo.InvariantCulture)),
															int.Parse(applicant.DateOfBirth.DD.ToString(CultureInfo.InvariantCulture))),
								"DateOfBirth", false);
						TryRead(() => app.Gender = applicant.Gender, "Gender");
						data.Applicants.Add(app);
					}, "Applicant");
			}

			TryRead(() => data.BureauScore = Convert.ToInt32(outputRoot.Output.ConsumerSummary.PremiumValueData.Scoring.E5S051), "BureauScore");
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
				}, "CaisDOB", false);
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

			data.Cais = GetCais(outputRoot);

			data.Error = Errors;
			data.HasParsingError = HasParsingError;
			Console.WriteLine(JsonConvert.SerializeObject(data, new JsonSerializerSettings { Formatting = Formatting.Indented }));
			return data;

		}

		private List<ExperianConsumerDataCais> GetCais(OutputRoot outputRoot)
		{
			var caisAccounts = new List<ExperianConsumerDataCais>();
			TryRead(() =>
			{
				foreach (var caisData in outputRoot.Output.FullConsumerData.ConsumerData.CAIS)
				{
					var account = new ExperianConsumerDataCais
						{
							AccountBalances = new List<ExperianConsumerDataCaisBalance>(),
							CardHistories = new List<ExperianConsumerDataCaisCardHistory>()
						};

					for (int j = 0; j < caisData.CAISDetails.Length; ++j)
					{
						var caisDetails = caisData.CAISDetails[j];
						TryRead(() => account.CAISAccStartDate = new DateTime(caisDetails.CAISAccStartDate.CCYY,
																			  caisDetails.CAISAccStartDate.MM,
																			  caisDetails.CAISAccStartDate.DD), "CAISAccStartDate", false);
						TryRead(() => account.LastUpdatedDate = new DateTime(caisDetails.LastUpdatedDate.CCYY,
																			 caisDetails.LastUpdatedDate.MM,
																			 caisDetails.LastUpdatedDate.DD), "LastUpdatedDate", false);
						TryRead(() => account.SettlementDate = new DateTime(caisDetails.SettlementDate.CCYY,
																			caisDetails.SettlementDate.MM,
																			caisDetails.SettlementDate.DD), "SettlementDate", false);
						TryRead(() => account.MatchTo = Convert.ToInt32(caisDetails.MatchDetails.MatchTo), "MatchTo");
						TryRead(() => account.CreditLimit = Convert.ToInt32(caisDetails.CreditLimit.Amount.Replace("£", "")),
								"CreditLimit", false);

						TryRead(() => account.Balance = Convert.ToInt32(caisDetails.Balance.Amount.Replace("£", "")), "Balance", false);
						TryRead(
							() => account.CurrentDefBalance = Convert.ToInt32(caisDetails.CurrentDefBalance.Amount.Replace("£", "")),
							"CurrentDefBalance", false);
						TryRead(
							() => account.DelinquentBalance = Convert.ToInt32(caisDetails.DelinquentBalance.Amount.Replace("£", "")),
							"DelinquentBalance", false);

						TryRead(() => account.AccountStatusCodes = caisDetails.AccountStatusCodes ?? string.Empty, "AccountStatusCodes");
						TryRead(() => account.Status1To2 = caisDetails.Status1To2, "Status1To2");
						TryRead(() => account.StatusTo3 = caisDetails.StatusTo3, "StatusTo3");
						TryRead(() => account.NumOfMonthsHistory = Convert.ToInt32(caisDetails.NumOfMonthsHistory), "NumOfMonthsHistory");
						TryRead(() => account.WorstStatus = caisDetails.WorstStatus, "WorstStatus");

						TryRead(() => account.AccountType = caisDetails.AccountType, "AccountType");
						TryRead(() => account.CompanyType = caisDetails.CompanyType, "CompanyType");
						TryRead(() => account.AccountStatus = caisDetails.AccountStatus, "AccountStatus");

						TryRead(() => account.RepaymentPeriod = Convert.ToInt32(caisDetails.RepaymentPeriod), "RepaymentPeriod");
						TryRead(() => account.Payment = Convert.ToInt32(caisDetails.Payment.Replace("£", "")), "Payment", false);

						TryRead(() => account.NumAccountBalances = Convert.ToInt32(caisDetails.NumAccountBalances), "NumAccountBalances");
						TryRead(() => account.NumCardHistories = Convert.ToInt32(caisDetails.NumCardHistories), "NumCardHistories");


						if (caisDetails.CardHistories != null && account.NumCardHistories > 0)
						{
							for (int k = 0; k < caisDetails.CardHistories.Length; ++k)
							{
								var cardHist = new ExperianConsumerDataCaisCardHistory();
								TryRead(() => cardHist.CashAdvanceAmount = Convert.ToInt32(caisDetails.CardHistories[k].CashAdvanceAmount),
										"CashAdvanceAmount");
								TryRead(() => cardHist.NumCashAdvances = Convert.ToInt32(caisDetails.CardHistories[k].NumCashAdvances),
										"NumCashAdvances");
								TryRead(() => cardHist.PaymentAmount = Convert.ToInt32(caisDetails.CardHistories[k].PaymentAmount),
										"PaymentAmount");
								TryRead(() => cardHist.PaymentCode = caisDetails.CardHistories[k].PaymentCode, "PaymentCode");
								TryRead(() => cardHist.PrevStatementBal = Convert.ToInt32(caisDetails.CardHistories[k].PrevStatementBal),
										"PrevStatementBal");
								TryRead(() => cardHist.PromotionalRate = caisDetails.CardHistories[k].PromotionalRate, "PromotionalRate");
								account.CardHistories.Add(cardHist);
							}
						}

						if (caisDetails.AccountBalances != null && account.NumAccountBalances > 0)
						{
							for (int l = 0; l < caisDetails.AccountBalances.Length; ++l)
							{
								var balance = new ExperianConsumerDataCaisBalance();
								TryRead(() => balance.AccountBalance = Convert.ToInt32(caisDetails.AccountBalances[l].AccountBalance),
										"AccountBalance");
								TryRead(() => balance.Status = caisDetails.AccountBalances[l].Status, "Status");
								account.AccountBalances.Add(balance);
							}
						}

						caisAccounts.Add(account);
					}
				}
			}, "Cais");

			return caisAccounts;

		}

		private void TryRead(Action a, string key, bool isRequered = true)
		{
			try
			{
				a();
			}
			catch
			{
				if (isRequered)
				{
					HasParsingError = true;
					Errors += "Can`t read value for: " + key + Environment.NewLine;
				}
			}
		}
	}

	public class ConsumerServiceResult
	{
		public string ExperianResult { get; set; }//todo remove
		public DateTime LastUpdateDate { get; set; }//todo remove

		public ExperianConsumerData Data { get; set; }
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
