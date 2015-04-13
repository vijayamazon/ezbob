using System;
using EzBobIntegration.Web_References.Consumer;

namespace ExperianLib
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB.Experian;

	public class ConsumerExperianModelBuilder
	{
		private string Errors { get; set; }
		private bool HasParsingError { get; set; }

		public ExperianConsumerData Build(OutputRoot outputRoot, int? customerId = null, int? directorId = null, DateTime? insertDate = null, long serviceLogId = 0)
		{
			var data = BuildMain(outputRoot, customerId, directorId, insertDate, serviceLogId);
			data.Cais = GetCais(outputRoot);
			data.Locations = GetLocations(outputRoot);
			data.Residencies = GetResidencies(outputRoot);
			data.Nocs = GetNocs(outputRoot);
			data.Error = Errors;
			data.HasParsingError = HasParsingError;
			return data;

		}

		private ExperianConsumerData BuildMain(OutputRoot outputRoot, int? customerId, int? directorId, DateTime? insertDate, long serviceLogId) {
			var data = new ExperianConsumerData {
				Applicants = new List<ExperianConsumerDataApplicant>(),
				Cais = new List<ExperianConsumerDataCais>(),
				ServiceLogId = serviceLogId,
				CustomerId = customerId,
				DirectorId = directorId,
				InsertDate = insertDate.HasValue ? insertDate.Value : DateTime.UtcNow
			};

			if (outputRoot == null || outputRoot.Output == null || outputRoot.Output.Error != null) {
				if (outputRoot != null && outputRoot.Output != null) {
					Errors = string.Format("Error from service with code: {0}, severity: {1}, message: {2} \n",
										   outputRoot.Output.Error.ErrorCode, outputRoot.Output.Error.Severity,
										   outputRoot.Output.Error.Message);
				} else {
					Errors = "OutputRoot is null";
				}
				data.HasExperianError = true;
				data.Error = Errors;
				return data;
			}

			TryRead(() => {
						foreach (var applicant in outputRoot.Output.Applicant) {
							TryRead(() => {
									var app = new ExperianConsumerDataApplicant();
									TryRead(() => app.ApplicantIdentifier = Convert.ToInt32(applicant.ApplicantIdentifier), "ApplicantIdentifier");
									TryRead(() => app.Title = applicant.Name.Title, "Title");
									TryRead(() => app.Forename = applicant.Name.Forename, "Forename");
									TryRead(() => app.MiddleName = applicant.Name.MiddleName, "MiddleName");
									TryRead(() => app.Surname = applicant.Name.Surname, "Surname");
									TryRead(() => app.Suffix = applicant.Name.Suffix, "Suffix");
									TryRead(() => app.DateOfBirth = new DateTime(
																		int.Parse(applicant.DateOfBirth.CCYY.ToString(CultureInfo.InvariantCulture)),
																		int.Parse(applicant.DateOfBirth.MM.ToString(CultureInfo.InvariantCulture)),
																		int.Parse(applicant.DateOfBirth.DD.ToString(CultureInfo.InvariantCulture)), 0,
																		0, 0, DateTimeKind.Utc),
											"DateOfBirth", false);
									TryRead(() => app.Gender = applicant.Gender, "Gender");
									data.Applicants.Add(app);
								}, "Applicant");
						}
					}, "Applicants", false);

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
			TryRead(() => {
					if (null != outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04) {
						data.CaisDOB = new DateTime(1900 + outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04.YY,
													outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04.MM,
													outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04.DD, 0, 0, 0, DateTimeKind.Utc);
					} else if (null != outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB) {
						data.CaisDOB = new DateTime(1900 + outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB.YY,
													outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB.MM,
													outputRoot.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB.DD, 0, 0, 0, DateTimeKind.Utc);
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

			return data;
		}

		private List<ExperianConsumerDataNoc> GetNocs(OutputRoot outputRoot)
		{
			var nocs = new List<ExperianConsumerDataNoc>();
			TryRead(() =>
				{
					if (outputRoot.Output.FullConsumerData.ConsumerData.NOC != null)
						foreach (var nocInfo in outputRoot.Output.FullConsumerData.ConsumerData.NOC)
						{
							nocs.AddRange(
								nocInfo.NoCDetails.Select(
									nocDetails => new ExperianConsumerDataNoc {Reference = nocDetails.Reference, TextLine = nocDetails.TextLine }));
						}
				},"Noc");
			return nocs;

		}

		private List<ExperianConsumerDataLocation> GetLocations(OutputRoot outputRoot)
		{
			var locations = new List<ExperianConsumerDataLocation>();
			TryRead(() =>
				{
					for (int i = 0; i < outputRoot.Output.LocationDetails.Length; ++i)
					{
						var location = new ExperianConsumerDataLocation();
						TryRead(() => location.LocationIdentifier = outputRoot.Output.LocationDetails[i].LocationIdentifier, "LocationIdentifier");
						TryRead(() => location.Flat = outputRoot.Output.LocationDetails[i].UKLocation.Flat, "Flat");
						TryRead(() => location.HouseName = outputRoot.Output.LocationDetails[i].UKLocation.HouseName, "HouseName");
						TryRead(() => location.HouseNumber = outputRoot.Output.LocationDetails[i].UKLocation.HouseNumber, "HouseNumber");
						TryRead(() => location.Street = outputRoot.Output.LocationDetails[i].UKLocation.Street, "Street");
						TryRead(() => location.Street2 = outputRoot.Output.LocationDetails[i].UKLocation.Street2, "Street2");
						TryRead(() => location.District = outputRoot.Output.LocationDetails[i].UKLocation.District, "District");
						TryRead(() => location.District2 = outputRoot.Output.LocationDetails[i].UKLocation.District2, "District2");
						TryRead(() => location.PostTown = outputRoot.Output.LocationDetails[i].UKLocation.PostTown, "PostTown");
						TryRead(() => location.County = outputRoot.Output.LocationDetails[i].UKLocation.County, "County");
						TryRead(() => location.Country = outputRoot.Output.LocationDetails[i].UKLocation.Country, "Country");
						TryRead(() => location.Postcode = outputRoot.Output.LocationDetails[i].UKLocation.Postcode, "Postcode");
						TryRead(() => location.POBox = outputRoot.Output.LocationDetails[i].UKLocation.POBox, "POBox");
						TryRead(() => location.SharedLetterbox = outputRoot.Output.LocationDetails[i].UKLocation.SharedLetterbox, "SharedLetterbox");
						TryRead(() => location.FormattedLocation = outputRoot.Output.LocationDetails[i].FormattedLocation, "FormattedLocation");
						locations.Add(location);

					}
				},
				"Location");
			return locations;
		}

		private List<ExperianConsumerDataResidency> GetResidencies(OutputRoot outputRoot)
		{
			var residencies = new List<ExperianConsumerDataResidency>();
			TryRead(() =>
			{
				for (int i = 0; i < outputRoot.Output.Residency.Length; ++i)
				{
					var residency = new ExperianConsumerDataResidency();
					TryRead(() => residency.LocationIdentifier = Convert.ToInt32(outputRoot.Output.Residency[i].LocationIdentifier), "LocationIdentifier");
					TryRead(() => residency.ApplicantIdentifier = Convert.ToInt32(outputRoot.Output.Residency[i].ApplicantIdentifier), "LocationIdentifier");
					TryRead(() => residency.LocationCode = outputRoot.Output.Residency[i].LocationCode, "LocationCode");
					TryRead(() => residency.TimeAtYears = outputRoot.Output.Residency[i].TimeAt.Years, "TimeAtYears");
					TryRead(() => residency.TimeAtMonths = outputRoot.Output.Residency[i].TimeAt.Months, "TimeAtMonths");
					TryRead(() => residency.ResidencyDateFrom = new DateTime(
															int.Parse(outputRoot.Output.Residency[i].ResidencyDateFrom.CCYY.ToString(CultureInfo.InvariantCulture)),
															int.Parse(outputRoot.Output.Residency[i].ResidencyDateFrom.MM.ToString(CultureInfo.InvariantCulture)),
															int.Parse(outputRoot.Output.Residency[i].ResidencyDateFrom.DD.ToString(CultureInfo.InvariantCulture)), 0, 0, 0, DateTimeKind.Utc),
								"ResidencyDateFrom", false);

					TryRead(() => residency.ResidencyDateTo = new DateTime(
															int.Parse(outputRoot.Output.Residency[i].ResidencyDateTo.CCYY.ToString(CultureInfo.InvariantCulture)),
															int.Parse(outputRoot.Output.Residency[i].ResidencyDateTo.MM.ToString(CultureInfo.InvariantCulture)),
															int.Parse(outputRoot.Output.Residency[i].ResidencyDateTo.DD.ToString(CultureInfo.InvariantCulture)), 0, 0, 0, DateTimeKind.Utc),
								"ResidencyDateTo", false);
					residencies.Add(residency);
				}
			},
				"Residency");
			return residencies;
		}

		private List<ExperianConsumerDataCais> GetCais(OutputRoot outputRoot)
		{
			var caisAccounts = new List<ExperianConsumerDataCais>();
			TryRead(() =>
			{
				foreach (var caisData in outputRoot.Output.FullConsumerData.ConsumerData.CAIS)
				{
					for (int j = 0; j < caisData.CAISDetails.Length; ++j)
					{
						var account = new ExperianConsumerDataCais
						{
							AccountBalances = new List<ExperianConsumerDataCaisBalance>(),
							CardHistories = new List<ExperianConsumerDataCaisCardHistory>()
						};

						var caisDetails = caisData.CAISDetails[j];
						TryRead(() => account.CAISAccStartDate = new DateTime(caisDetails.CAISAccStartDate.CCYY,
																			  caisDetails.CAISAccStartDate.MM,
																			  caisDetails.CAISAccStartDate.DD, 0, 0, 0, DateTimeKind.Utc), "CAISAccStartDate", false);
						TryRead(() => account.LastUpdatedDate = new DateTime(caisDetails.LastUpdatedDate.CCYY,
																			 caisDetails.LastUpdatedDate.MM,
																			 caisDetails.LastUpdatedDate.DD, 0, 0, 0, DateTimeKind.Utc), "LastUpdatedDate", false);
						TryRead(() => account.SettlementDate = new DateTime(caisDetails.SettlementDate.CCYY,
																			caisDetails.SettlementDate.MM,
																			caisDetails.SettlementDate.DD, 0, 0, 0, DateTimeKind.Utc), "SettlementDate", false);
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
}
