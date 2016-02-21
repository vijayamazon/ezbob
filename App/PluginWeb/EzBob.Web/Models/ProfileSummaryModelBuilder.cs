namespace EzBob.Web.Models {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Extensions;
	using EzBob.Backend.Models;
	using EzBob.Models;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Infrastructure;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using MoreLinq;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;

	public class ProfileSummaryModelBuilder {
		public ProfileSummaryModelBuilder(CreditBureauModelBuilder creditBureauModelBuilder, IEzbobWorkplaceContext context, ServiceClient serviceClient) {
			this.creditBureauModelBuilder = creditBureauModelBuilder;
			this.serviceClient = serviceClient;
			this.context = context;
		}

		public ProfileSummaryModel CreateProfile(Customer customer, CreditBureauModel creditBureau, CompanyScoreModel companyScore) {
			TimeCounter tc = new TimeCounter("ProfileSummaryModel 1 building time for customer " + customer.Id);
			var summary = new ProfileSummaryModel();
			using (tc.AddStep("BuildCustomerSummary Time taken")) {
				BuildCustomerSummary(summary, customer);
			}
			using (tc.AddStep("BuildCreditBureau Time taken")) {
				BuildCreditBureau(customer, summary, creditBureau);
			}
			using (tc.AddStep("AddDecisionHistory Time taken")) {
				AddDecisionHistory(summary, customer);
			}
			using (tc.AddStep("BuildRequestedLoan Time taken")) {
				BuildRequestedLoan(summary, customer);
			}
			using (tc.AddStep("BuildAlerts Time taken")) {
				BuildAlerts(summary, customer);
			}
			using (tc.AddStep("BuildCompaniesHouseAlerts Time taken")) {
				BuildCompaniesHouseAlerts(summary, companyScore);
			}
			log.Info(tc.ToString());
			return summary;
		}

		public ProfileSummaryModel CreateProfile(Customer customer) {
			TimeCounter tc = new TimeCounter("ProfileSummaryModel 2 building time for customer " + customer.Id);
			var summary = new ProfileSummaryModel();
			using (tc.AddStep("BuildCustomerSummary Time taken")) {
				BuildCustomerSummary(summary, customer);
			}
			using (tc.AddStep("BuildCreditBureau Time taken")) {
				BuildCreditBureau(customer, summary);
			}
			using (tc.AddStep("AddDecisionHistory Time taken")) {
				AddDecisionHistory(summary, customer);
			}
			using (tc.AddStep("BuildRequestedLoan Time taken")) {
				BuildRequestedLoan(summary, customer);
			}
			using (tc.AddStep("BuildAlerts Time taken")) {
				BuildAlerts(summary, customer);
			}
			log.Info(tc.ToString());
			return summary;
		}

		private static void BuildRequestedLoan(ProfileSummaryModel summary, Customer customer) {
			var rl = new CustomerRequestedLoanModel();
			var requestedLoan = customer.CustomerRequestedLoan.OrderByDescending(x => x.Created).FirstOrDefault();
			if (requestedLoan != null) {
				rl.Amount = requestedLoan.Amount;
				rl.Created = requestedLoan.Created;
				rl.Term = requestedLoan.Term;
			}
			summary.RequestedLoan = rl;
		}

		private static string Money<T>(T amount) where T : IFormattable {
			return String.Format("{0:0.#}", amount);
		}

		private void AddDecisionHistory(ProfileSummaryModel summary, Customer customer) {
			var decisionHistories = this.serviceClient.Instance.LoadDecisionHistory(customer.Id, this.context.UserId);
			summary.DecisionHistory = decisionHistories.Model
				.Select(DecisionHistoryModel.Create)
				.OrderBy(x => x.Date)
				.ToList();

			//Dashboard
			summary.Decisions = new DecisionsModel {
				TotalDecisionsCount = summary.DecisionHistory.Count,
				TotalApprovedAmount = summary.DecisionHistory.Where(dh => dh.Action == "Approve")
					.Sum(dh => dh.ApprovedSum),
				RejectsCount = summary.DecisionHistory.Count(dh => dh.Action == "Reject")
			};
			var lastApprove = summary.DecisionHistory.OrderByDescending(dh => dh.Date)
				.FirstOrDefault(dh => dh.Action == "Approve");
			if (lastApprove != null) {
				summary.Decisions.LastInterestRate = lastApprove.InterestRate;
				summary.Decisions.LastDecisionDate = lastApprove.Date;
			}
		}

		private void AppendLi(StringBuilder sb, string error) {
			sb.AppendLine("<li>");
			sb.Append(error);
			sb.Append("</li>");
		}

		private void BuildMultiBrandAlert(int customerID, AlertsModel target) {
			try {
				MultiBrandLoanSummaryActionResult ar = this.serviceClient.Instance.BuildMultiBrandLoanSummary(customerID);

				bool isMulti = ar.Summary.OriginCount > 1;

				(isMulti ? target.Errors : target.Infos).Add(new AlertModel {
					Abbreviation = "BRND",
					Alert = (isMulti ? "Multi-" : "Single ") + "branded customer",
					AlertType = (isMulti ? AlertType.Error : AlertType.Info).DescriptionAttr(),
					Tooltip = "<br>" + string.Join("<br>", ar.Summary.Loans),
					Tab = ProfileTab.Dashboard.DescriptionAttr()
				});
			} catch (Exception e) {
				log.Alert(e, "Failed to build multi brand alert.");
			} // try
		} // BuildMultiBrandAlert

		private void BuildAlerts(ProfileSummaryModel summary, Customer customer) {
			TimeCounter tc = new TimeCounter("BuildAlerts building time for customer " + customer.Id);

			summary.Alerts = new AlertsModel {
				Errors = new List<AlertModel>(),
				Warnings = new List<AlertModel>(),
				Infos = new List<AlertModel>(),
			};

			using (tc.AddStep("BuildMultiBrandAlert Time taken")) {
				BuildMultiBrandAlert(customer.Id, summary.Alerts);
			}

			using (tc.AddStep("CustomerAlerts Time taken")) {
				var isBrokerRegulated = (customer.Broker != null) && customer.Broker.FCARegistered;

				var IsWizardComplete = (customer.WizardStep != null) && customer.WizardStep.TheLastOne;

				if (!isBrokerRegulated && customer.PersonalInfo.IsRegulated && IsWizardComplete) {
					summary.Alerts.Infos.Add(new AlertModel {
						Abbreviation = "NRB",
						Alert = "Regulated customer for a Non-Regulated broker",
						AlertType = AlertType.Warning.DescriptionAttr(),
						Tab = ProfileTab.Dashboard.DescriptionAttr()
					});
				} // if

				if (customer.IsTest) {
					summary.Alerts.Infos.Add(new AlertModel {
						Abbreviation = "Test",
						Alert = "Is test",
						AlertType = AlertType.Info.DescriptionAttr(),
						Tab = ProfileTab.Dashboard.DescriptionAttr()
					});
				} // if

				if (customer.IsAlibaba) {
					summary.Alerts.Infos.Add(new AlertModel {
						Abbreviation = "Ali",
						Alert = "Is alibaba customer",
						AlertType = AlertType.Info.DescriptionAttr(),
						Tab = ProfileTab.Dashboard.DescriptionAttr()
					});
				} // if

				if (customer.CciMark) {
					summary.Alerts.Errors.Add(new AlertModel {
						Abbreviation = "CCI",
						Alert = "CCI Mark",
						AlertType = AlertType.Error.DescriptionAttr(),
						Tab = ProfileTab.Dashboard.DescriptionAttr()
					});
				} // if

				if (customer.CollectionStatus.IsDefault || customer.CollectionStatus.Name == "Bad") {
					summary.Alerts.Errors.Add(new AlertModel {
						Abbreviation = "Bad",
						Alert = string.Format("Customer Status : {0}", customer.CollectionStatus.Name),
						AlertType = AlertType.Error.DescriptionAttr(),
						Tab = ProfileTab.Dashboard.DescriptionAttr()
					});
				} else if (customer.CollectionStatus.Name == "Risky") {
					summary.Alerts.Warnings.Add(new AlertModel {
						Abbreviation = "Risky",
						Alert = string.Format("Customer Status : {0}", customer.CollectionStatus.Name),
						AlertType = AlertType.Warning.DescriptionAttr(),
						Tab = ProfileTab.Dashboard.DescriptionAttr()
					});
				} // if

				if (customer.FraudStatus != FraudStatus.Ok) {
					summary.Alerts.Errors.Add(new AlertModel {
						Abbreviation = "F",
						Alert = string.Format("Fraud Status : {0}", customer.FraudStatus.DescriptionAttr()),
						AlertType = AlertType.Error.DescriptionAttr(),
						Tab = ProfileTab.FraudDetection.DescriptionAttr()
					});
				} // if

				if (customer.CreditResult == CreditResultStatus.PendingInvestor) {
					summary.Alerts.Warnings.Add(new AlertModel {
						Abbreviation = "PI",
						Alert = string.Format("Credit Result : {0}", customer.CreditResult.DescriptionAttr()),
						AlertType = AlertType.Warning.DescriptionAttr(),
						Tab = ProfileTab.Dashboard.DescriptionAttr()
					});
				} // if

				switch (customer.AMLResult) {
					case "Rejected":
						summary.Alerts.Errors.Add(new AlertModel {
							Abbreviation = "AML",
							Alert = string.Format("AML Status : {0}", customer.AMLResult),
							AlertType = AlertType.Error.DescriptionAttr(),
							Tab = ProfileTab.CreditBureau.DescriptionAttr()
						});
						break;
					case "Not performed":
					case "Warning":
						summary.Alerts.Warnings.Add(new AlertModel {
							Abbreviation = "AML",
							Alert = string.Format("AML Status : {0}", customer.AMLResult),
							AlertType = AlertType.Warning.DescriptionAttr(),
							Tab = ProfileTab.CreditBureau.DescriptionAttr()
						});
						break;
				} // switch

				switch (summary.CreditBureau.ThinFile) {
					case "Yes":
						summary.Alerts.Errors.Add(new AlertModel {
							Abbreviation = "TF",
							Alert = "Thin file",
							AlertType = AlertType.Error.DescriptionAttr(),
							Tab = ProfileTab.CreditBureau.DescriptionAttr()
						});
						break;
					case "N/A":
						summary.Alerts.Warnings.Add(new AlertModel {
							Abbreviation = "N/A",
							Alert = "Couldn't get financial accounts",
							AlertType = AlertType.Warning.DescriptionAttr(),
							Tab = ProfileTab.Dashboard.DescriptionAttr()
						});
						break;
				} // switch

				if (summary.CreditBureau.NumDirectorThinFiles > 0) {
					summary.Alerts.Errors.Add(new AlertModel {
						Abbreviation = "TF",
						Alert =
							string.Format("{0} director{1} with thin file", summary.CreditBureau.NumDirectorThinFiles,
								summary.CreditBureau.NumDirectorThinFiles == 1 ? "" : "s"),
						AlertType = AlertType.Error.DescriptionAttr(),
						Tab = ProfileTab.Dashboard.DescriptionAttr()
					});
				} // if

				if (summary.CreditBureau.NumDirectorNA > 0) {
					summary.Alerts.Warnings.Add(new AlertModel {
						Abbreviation = "N/A",
						Alert =
							string.Format("{0} director{1} with no experian data available", summary.CreditBureau.NumDirectorNA,
								summary.CreditBureau.NumDirectorThinFiles == 1 ? "" : "s"),
						AlertType = AlertType.Warning.DescriptionAttr(),
						Tab = ProfileTab.Dashboard.DescriptionAttr()
					});
				} // if

				if (summary.CreditBureau.ApplicantDOBs != null) {
					foreach (var dob in summary.CreditBureau.ApplicantDOBs) {
						if (dob.HasValue && (dob.Value.AddYears(18) > DateTime.Today)) {
							summary.Alerts.Errors.Add(new AlertModel {
								Abbreviation = "A",
								Alert = "Age of applicant under 18",
								AlertType = AlertType.Error.DescriptionAttr(),
								Tab = ProfileTab.Dashboard.DescriptionAttr()
							});
						} // if
					} // for each
				} // if

				if (customer.CustomerRelationStates.Any()) {
					var state = customer.CustomerRelationStates.First();
					if (state.IsFollowUp.HasValue && state.IsFollowUp.Value && state.FollowUp.FollowUpDate <= DateTime.UtcNow) {
						summary.Alerts.Errors.Add(new AlertModel {
							Abbreviation = "Follow",
							Alert = "Customer relations follow up date is due " + state.FollowUp.FollowUpDate.ToString("dd/MM/yyyy"),
							AlertType = AlertType.Error.DescriptionAttr(),
							Tab = ProfileTab.CustomerRelations.DescriptionAttr()
						});
					} // if
				} // if
			}

			using (tc.AddStep("GetCompanySeniorityAlerts Time taken")) {
				try {
					if (customer.PersonalInfo != null) {
						DateTime? companySeniority =
							this.serviceClient.Instance.GetCompanySeniority(customer.Id,
								customer.PersonalInfo.TypeOfBusiness.Reduce() ==
									TypeOfBusinessReduced.Limited, this.context.UserId)
								.Value;
						if (companySeniority.HasValue && companySeniority.Value.AddYears(1) > DateTime.UtcNow &&
							(companySeniority.Value.Year != DateTime.UtcNow.Year || companySeniority.Value.Month != DateTime.UtcNow.Month ||
								companySeniority.Value.Day != DateTime.UtcNow.Day)) {
							summary.Alerts.Errors.Add(new AlertModel {
								Abbreviation = "YC",
								Alert = "Young company. Incorporation date: " + companySeniority.Value.ToString("dd/MM/yyyy"),
								AlertType = AlertType.Error.DescriptionAttr(),
								Tab = ProfileTab.CompanyScore.DescriptionAttr()
							});
						}
					}
				} catch (Exception e) {
					log.Debug(e, "Error fetching company seniority.");
				} // try
			}
			using (tc.AddStep("BuildLandRegistryAlerts Time taken")) {
				bool bResult = BuildLandRegistryAlerts(customer, summary);
				log.Debug("Just FYI: BuildLandRegistryAlerts() returned {0}", bResult ? "true" : "false");
			}
			using (tc.AddStep("BuildDataAlerts Time taken")) {
				BuildDataAlerts(customer, summary);
			}
			using (tc.AddStep("BuildCompanyCaisAlerts Time taken")) {
				BuildCompanyCaisAlerts(customer, summary, this.context.UserId);
			}
			using (tc.AddStep("LoadExperianConsumerMortgageData Time taken")) {
				bool hasMortgage = false;
				bool isHomeOwner = customer.PropertyStatus != null && (customer.PropertyStatus.IsOwnerOfMainAddress || customer.PropertyStatus.IsOwnerOfOtherProperties);
				try {
					hasMortgage = this.serviceClient.Instance.LoadExperianConsumerMortgageData(this.context.UserId, customer.Id)
						.Value.NumMortgages > 0;
				} catch (Exception e) {
					log.Debug(e, "Error fetching customer's mortgages.");
				} // try

				if (isHomeOwner && !hasMortgage) {
					summary.Alerts.Warnings.Add(new AlertModel {
						Abbreviation = "MTG",
						Alert = "Home owner and no mortgages",
						AlertType = AlertType.Warning.DescriptionAttr(),
						Tab = ProfileTab.Properties.DescriptionAttr()
					});
				} else if (!isHomeOwner && hasMortgage) {
					summary.Alerts.Warnings.Add(new AlertModel {
						Abbreviation = "MTG",
						Alert = "Has mortgages but not a home owner",
						AlertType = AlertType.Warning.DescriptionAttr(),
						Tab = ProfileTab.Properties.DescriptionAttr()
					});
				} // if
			}
			using (tc.AddStep("MedalAlerts Time taken")) {
				this.medalCalculationsRepository = ObjectFactory.GetInstance<MedalCalculationsRepository>();
				MedalCalculations medalCalculationsRecord = this.medalCalculationsRepository.GetActiveMedal(customer.Id);

				if (customer.Company != null && (customer.Company.TypeOfBusiness == EZBob.DatabaseLib.Model.Database.TypeOfBusiness.LLP || customer.Company.TypeOfBusiness == EZBob.DatabaseLib.Model.Database.TypeOfBusiness.Limited) && customer.CustomerMarketPlaces.Count(x => x.Marketplace.Name == "HMRC") < 2) {
					// The customer should have medal
					if (medalCalculationsRecord == null) {
						summary.Alerts.Errors.Add(new AlertModel {
							Abbreviation = "MDL",
							Alert = "New medal was not calculated",
							AlertType = AlertType.Error.DescriptionAttr(),
							Tab = ProfileTab.Calculator.DescriptionAttr()
						});
					} else if (!string.IsNullOrEmpty(medalCalculationsRecord.Error)) {
						summary.Alerts.Errors.Add(new AlertModel {
							Abbreviation = "MDL",
							Alert = string.Format("Error while calculating new medal: {0}", medalCalculationsRecord.Error),
							AlertType = AlertType.Error.DescriptionAttr(),
							Tab = ProfileTab.Calculator.DescriptionAttr()
						});
					} // if
				} else if (customer.Company != null && (customer.Company.TypeOfBusiness != EZBob.DatabaseLib.Model.Database.TypeOfBusiness.LLP &&
					customer.Company.TypeOfBusiness != EZBob.DatabaseLib.Model.Database.TypeOfBusiness.Limited) ||
					customer.CustomerMarketPlaces.Count(x => x.Marketplace.Name == "HMRC") > 1) {
					summary.Alerts.Infos.Add(new AlertModel {
						Abbreviation = "MDL",
						Alert = "This customer shouldn't have new medal",
						AlertType = AlertType.Info.DescriptionAttr(),
						Tab = ProfileTab.Calculator.DescriptionAttr()
					});
				} // if
			}//MedalAlerts
			log.Info(tc.ToString());
		}// BuildAlerts

		private void BuildDataAlerts(Customer customer, ProfileSummaryModel summary) {
			var currAddr = customer.AddressInfo.PersonalAddress.FirstOrDefault();
			var errors = new StringBuilder();

			if (currAddr != null && !currAddr.Zoopla.Any() && currAddr.IsOwnerAccordingToLandRegistry)
				AppendLi(errors, "No zoopla for current address");

			var otherProperties = customer.AddressInfo.OtherPropertiesAddresses.Where(x => x.IsOwnerAccordingToLandRegistry && !x.Zoopla.Any());
			if (otherProperties.Any())
				AppendLi(errors, "No zoopla for owned property");

			if (customer.CustomerMarketPlaces.Any(x => !string.IsNullOrEmpty(x.UpdateError))) {
				var mpErrors =
					customer.CustomerMarketPlaces.Where(x => !string.IsNullOrEmpty(x.UpdateError))
						.Select(x => string.Format("MP {0} : {1}", x.DisplayName, x.UpdateError));
				foreach (var mpError in mpErrors)
					AppendLi(errors, mpError);
			}

			if (!customer.ExperianConsumerScore.HasValue || customer.ExperianConsumerScore.Value == 0)
				AppendLi(errors, "No consumer score");

			if (customer.Company != null &&
				!customer.Company.Directors.Any(x => !x.ExperianConsumerScore.HasValue || x.ExperianConsumerScore.Value == 0)) {
				var dirErrors = customer.Company.Directors.Where(
					x => !x.ExperianConsumerScore.HasValue || x.ExperianConsumerScore.Value == 0)
					.Select(x => string.Format("Director {0} {1} don't have consumer score", x.Name, x.Surname));
				foreach (var dirError in dirErrors)
					AppendLi(errors, dirError);
			}

			string errorStr = errors.ToString();
			if (!string.IsNullOrEmpty(errorStr)) {
				summary.Alerts.Errors.Add(new AlertModel {
					Abbreviation = "DATA",
					Alert = string.Format("<ul class='alert-list'>{0}</ul>", errorStr),
					AlertType = AlertType.Error.DescriptionAttr(),
					Tab = ProfileTab.Dashboard.DescriptionAttr()
				});
			}
		}//BuildDataAlerts

		private bool BuildLandRegistryAlerts(Customer customer, ProfileSummaryModel summary) {
			var customersLrs = this.serviceClient.Instance.LandRegistryLoad(customer.Id, this.context.UserId).Value;
			var lrs = customersLrs.Where(x =>
				x.RequestType == LandRegistryLib.LandRegistryRequestType.Res.ToString() &&
				x.ResponseType == LandRegistryLib.LandRegistryResponseType.Success.ToString())
				.ToList();

			if (customer.PropertyStatus != null && !lrs.Any() && (customer.PropertyStatus.IsOwnerOfMainAddress || customer.PropertyStatus.IsOwnerOfOtherProperties)) {
				summary.Alerts.Warnings.Add(new AlertModel {
					Abbreviation = "LR",
					Alert = "No land registries retrieved",
					AlertType = AlertType.Warning.DescriptionAttr(),
					Tab = ProfileTab.Properties.DescriptionAttr()
				});
				return true;
			}

			if (lrs.Any()) {
				var owners = lrs.SelectMany(x => x.Owners)
					.Select(x => new {
						firstName = x.FirstName ?? string.Empty,
						lastName = x.LastName ?? string.Empty,
						company = x.CompanyName ?? string.Empty
					})
					.ToList();
				if (owners.Any() && !owners.Any(owner =>
					owner.firstName.ToLowerInvariant()
						.Contains(customer.PersonalInfo.FirstName.Trim()
							.ToLowerInvariant()) &&
					owner.lastName.ToLowerInvariant()
						.Contains(customer.PersonalInfo.Surname.Trim()
							.ToLowerInvariant()))) {
					var ownerNames = owners.Select(x => string.Format("{0} {1} {2}", x.firstName, x.lastName, x.company))
						.Aggregate((a, b) => a + ", " + b);
					summary.Alerts.Errors.Add(new AlertModel {
						Abbreviation = "LR",
						Alert = "Not a land registry owner",
						Tooltip = string.Format("Owners list: {0}", ownerNames),
						AlertType = AlertType.Error.DescriptionAttr(),
						Tab = ProfileTab.Properties.DescriptionAttr()
					});
				}
			}

			return false;
		}

		private void BuildCompanyCaisAlerts(Customer customer, ProfileSummaryModel summary, int userId) {
			var errors = new StringBuilder();
			CompanyCaisDataActionResult companyCaisData = this.serviceClient.Instance.GetCompanyCaisDataForAlerts(userId, customer.Id);

			if (companyCaisData.NumOfCurrentDefaultAccounts > 0)
				AppendLi(errors, string.Format("Company has {0} default accounts", companyCaisData.NumOfCurrentDefaultAccounts));

			if (companyCaisData.NumOfSettledDefaultAccounts > 0) {
				summary.Alerts.Warnings.Add(new AlertModel {
					Abbreviation = "BUS",
					Alert = string.Format("Company has {0} settled default accounts", companyCaisData.NumOfSettledDefaultAccounts),
					AlertType = AlertType.Warning.DescriptionAttr(),
					Tab = ProfileTab.CompanyScore.DescriptionAttr()
				});
			}

			bool hasLastAccounts = false;
			int numOfLateAccounts = 0;
			foreach (CompanyCaisAccount account in companyCaisData.Accounts) {
				if (GetIsAccountLateInLastXMonths(account, CurrentValues.Instance.CompanyCaisLateAlertLongMonths))
					numOfLateAccounts++;

				if (!hasLastAccounts)
					hasLastAccounts = GetIsAccountLateInLastXMonths(account, CurrentValues.Instance.CompanyCaisLateAlertShortMonths);
			}

			if (hasLastAccounts)
				AppendLi(errors, string.Format("Company has late accounts in last {0} months", CurrentValues.Instance.CompanyCaisLateAlertShortMonths.Value));

			if (numOfLateAccounts > CurrentValues.Instance.CompanyCaisLateAlertShortPeriodThreshold)
				AppendLi(errors, string.Format("Company has {0} late accounts in last {1} months", numOfLateAccounts, CurrentValues.Instance.CompanyCaisLateAlertLongMonths.Value));

			string errorStr = errors.ToString();
			if (!string.IsNullOrEmpty(errorStr)) {
				summary.Alerts.Errors.Add(new AlertModel {
					Abbreviation = "BUS",
					Alert = string.Format("<ul class='alert-list'>{0}</ul>", errorStr),
					AlertType = AlertType.Error.DescriptionAttr(),
					Tab = ProfileTab.CompanyScore.DescriptionAttr()
				});
			}
		}

		private void BuildCompaniesHouseAlerts(ProfileSummaryModel summary, CompanyScoreModel companyScore) {
			if (companyScore.CompaniesHouseModel == null || companyScore.CompaniesHouseModel.Officers == null) {
				return;
			}

			var customerOfficer = companyScore.CompaniesHouseModel.Officers.FirstOrDefault(x => x.IsCustomer);

			if (customerOfficer != null &&
				customerOfficer.AppointmentOrder != null &&
				customerOfficer.AppointmentOrder.Appointments != null &&
				customerOfficer.AppointmentOrder.Appointments.Count >= 5) {
				summary.Alerts.Warnings.Add(new AlertModel {
					Abbreviation = "MDC",
					Alert = string.Format("Applicant has appointments in {0} companies", customerOfficer.AppointmentOrder.Appointments.Count),
					AlertType = AlertType.Warning.DescriptionAttr(),
					Tab = ProfileTab.CompanyScore.DescriptionAttr()
				});
			}

			if (customerOfficer != null &&
			customerOfficer.AppointmentOrder != null &&
			customerOfficer.AppointmentOrder.Appointments != null &&
			customerOfficer.AppointmentOrder.Appointments.Any(x => x.CompanyStatus != "Active")) {
				summary.Alerts.Errors.Add(new AlertModel {
					Abbreviation = "DSL",
					Alert = string.Format("Applicant has appointments in companies with status {0}",
						customerOfficer
						.AppointmentOrder
						.Appointments
						.Where(x => x.CompanyStatus != "Active")
						.Select(x => x.CompanyStatus)
						.Aggregate((a, b) => a + ", " + b)),
					AlertType = AlertType.Error.DescriptionAttr(),
					Tab = ProfileTab.CompanyScore.DescriptionAttr()
				});
			}
		}

		private void BuildCreditBureau(Customer customer, ProfileSummaryModel summary, CreditBureauModel creditBureauModel = null) {
			var creditBureau = new CreditBureau();

			if (creditBureauModel == null)
				creditBureauModel = this.creditBureauModelBuilder.Create(customer);

			if (creditBureauModel.Consumer != null && creditBureauModel.Consumer.ServiceLogId != null) {
				creditBureau.CreditBureauScore = creditBureauModel.Consumer.Score;
				creditBureau.TotalDebt = creditBureauModel.Consumer.TotalAccountBalances;
				creditBureau.TotalMonthlyRepayments = creditBureauModel.Consumer.TotalMonthlyRepayments;
				creditBureau.CreditCardBalances = creditBureauModel.Consumer.CreditCardBalances;
				creditBureau.BorrowerType = customer.PersonalInfo.TypeOfBusiness.TypeOfBussinessForWeb();
				creditBureau.FinancialAccounts = creditBureauModel.Consumer.AccountsInformation.Count();
				creditBureau.ThinFile = creditBureau.FinancialAccounts == 0 ? "Yes" : "No";
				// patch to fix crashing underwriter
				creditBureau.ApplicantDOBs = new List<DateTime?> {
					creditBureauModel.Consumer.Applicant != null ? creditBureauModel.Consumer.Applicant.DateOfBirth : null
				};

				if (creditBureauModel.Directors.Any()) {
					foreach (var director in creditBureauModel.Directors) {
						if (director != null && director.ServiceLogId != null && !director.AccountsInformation.Any())
							creditBureau.NumDirectorThinFiles++;

						if (director == null || director.ServiceLogId == null)
							creditBureau.NumDirectorNA++;
					}
				}
			} else
				creditBureau.ThinFile = "N/A";

			summary.CreditBureau = creditBureau;
		}

		private void BuildCustomerSummary(ProfileSummaryModel summary, Customer customer) {
			summary.Id = customer.Id;
			summary.IsOffline = customer.IsOffline;

			summary.AffordabilityAnalysis =
				new AffordabilityAnalysis {
					CashAvailabilityOrDeficits = "Not implemented now",
					EzBobMonthlyRepayment = Money(GetRepaymentAmount(customer))
				};
			summary.LoanActivity = CreateLoanActivity(customer);
			summary.AmlBwa =
				new AmlBwa {
					Aml = customer.AMLResult,
					Bwa = customer.BWAResult,
					Lighter = new Lighter(ObtainAmlState(customer))
				};
			summary.FraudCheck = new FraudCheck {
				Status = customer.Fraud.ToString(),
			};
			summary.OverallTurnOver = customer.PersonalInfo == null ? null : customer.PersonalInfo.OverallTurnOver;
			summary.WebSiteTurnOver = customer.PersonalInfo == null ? null : customer.PersonalInfo.WebSiteTurnOver;
			summary.Comment = customer.Comment;

			summary.CompanyEmployeeCountInfo = new CompanyEmployeeCountInfo(customer.Company);
			summary.CompanyInfo = CompanyInfoMap.FromCompany(customer.Company);
			summary.IsOffline = customer.IsOffline;
		}
	
		private LoanActivity CreateLoanActivity(Customer customer) {
			var previousLoans = customer.Loans.Count(x => x.DateClosed != null);
			var currentBalance = customer.Loans.Sum(x => x.Balance);
			var latePayments = customer.Loans.Sum(x => x.PastDues);
			var interest = customer.Loans.Where(l => l.Status == LoanStatus.Late)
				.Sum(l => l.InterestDue);
			var collection = customer.Loans.Where(x => x.IsDefaulted)
				.Sum(x => x.PastDues);
			var lateStatus = customer.PaymentDemenaor.ToString();

			int currentLateDays = 0;
			DateTime now = DateTime.UtcNow;
			var lateLoans = customer.Loans.Where(l => l.Status == LoanStatus.Late);
			foreach (Loan l in lateLoans) {
				foreach (LoanScheduleItem ls in l.Schedule) {
					int lateInDays = (int)(now - ls.Date).TotalDays;
					if (lateInDays > currentLateDays)
						currentLateDays = lateInDays;
				}
			}

			var totalFees =
				(from l in customer.Loans
				 from c in l.Charges
				 where c.State != "Expired"
				 where c.Amount > 0
				 select c.Amount).Sum();
			var feesCount =
				(from l in customer.Loans
				 from c in l.Charges
				 where c.State != "Expired"
				 where c.Amount > 0
				 select c.Amount).Count();

			//Dashboard
			var totalIssues = customer.Loans.Sum(l => l.LoanAmount);
			var totalIssuesCount = customer.Loans.Count;
			var repaid = customer.Loans.Sum(l => l.Repayments);
			var repaidCount = customer.Loans.Count(l => l.DateClosed.HasValue);
			var active = customer.Loans.Sum(l => l.Balance);
			var activeCount = customer.Loans.Count(l => !l.DateClosed.HasValue);
			var earnedInterest = customer.Loans.Sum(l => l.InterestPaid);

			var activeLoans = new List<ActiveLoan>();
			int i = 0;
			var loans = customer.Loans.OrderBy(l => l.Date);
			foreach (var loan in loans) {
				i++;
				if (!loan.DateClosed.HasValue) {
					int term = 0;
					try {
						var agreement = JsonConvert.DeserializeObject<AgreementModel>(loan.AgreementModel);
						term = agreement.Term;
					} catch (Exception ex) {
						log.Alert(ex, "Failed to build current loans model.");
					}

					if (loan.AgreementModel != null) {
						activeLoans.Add(new ActiveLoan {
							Approved = loan.CashRequest.ManagerApprovedSum,
							Balance = loan.Principal,
							LoanAmount = loan.LoanAmount,
							LoanAmountPercent =
								loan.CashRequest.ManagerApprovedSum.HasValue && loan.CashRequest.ManagerApprovedSum.Value > 0
									? loan.LoanAmount / (decimal)loan.CashRequest.ManagerApprovedSum.Value : 0,
							LoanDate = loan.Date,
							InterestRate = loan.InterestRate,
							IsLate = loan.Status == LoanStatus.Late,
							IsEU = loan.LoanSource.Name == EZBob.DatabaseLib.Model.Database.Loans.LoanSourceName.EU.ToString() || loan.LoanSource.Name == EZBob.DatabaseLib.Model.Database.Loans.LoanSourceName.COSME.ToString(),
							BalanceWidthPercent = loan.CashRequest.ManagerApprovedSum.HasValue && loan.CashRequest.ManagerApprovedSum.Value > 0
								? loan.Principal / (decimal)loan.CashRequest.ManagerApprovedSum.Value : 0,
							BalancePercent = loan.LoanAmount > 0 ? loan.Principal / loan.LoanAmount : 0,
							Term = term > 0 ? term : loan.CashRequest.RepaymentPeriod,
							TermApproved = loan.CashRequest.ApprovedRepaymentPeriod ?? loan.CashRequest.RepaymentPeriod,
							TotalFee = loan.SetupFee,
							LoanNumber = i,
							Comment = loan.CashRequest.UnderwriterComment
						});
					}
				}
			}

			if (activeLoans.Any()) {
				var maxLoan = activeLoans.MaxBy(x => x.Approved);

				maxLoan.WidthPercent = 1;

				foreach (var activeLoan in activeLoans) {
					if (activeLoan.Approved.HasValue)
						activeLoan.WidthPercent = maxLoan.Approved.HasValue && maxLoan.Approved.Value > 0 ? activeLoan.Approved.Value / maxLoan.Approved.Value : 0;
					activeLoan.BalanceWidthPercent *= (decimal)activeLoan.WidthPercent;
					activeLoan.LoanAmountWidthPercent = activeLoan.LoanAmountPercent * (decimal)activeLoan.WidthPercent;
				}
			}

			return new LoanActivity {
				PreviousLoans = Money(previousLoans),
				CurrentBalance = Money(currentBalance),
				LatePaymentsSum = Money(latePayments),
				Collection = Money(collection),
				LateInterest = Money(interest ?? 0),
				Lighter = new Lighter(ObtainLoanActivityState(latePayments, collection)),
				CurrentLateDays = currentLateDays.ToString(CultureInfo.InvariantCulture),
				PaymentDemeanor = lateStatus,
				TotalFees = Money(totalFees),
				FeesCount = Money(feesCount),

				//Dashboard

				TotalIssuesSum = totalIssues,
				TotalIssuesCount = totalIssuesCount,
				RepaidSum = repaid,
				RepaidCount = repaidCount,
				ActiveSum = active,
				ActiveCount = activeCount,
				EarnedInterest = earnedInterest,
				ActiveLoans = activeLoans.OrderByDescending(x => x.LoanNumber)
			};
		}

		private bool GetIsAccountLateInLastXMonths(CompanyCaisAccount account, int numberOfMonths) {
			DateTime tmp = account.LastUpdateDate.AddMonths(numberOfMonths);
			int numOfMonthsBackWeHaveDataFor = 0;
			while (tmp > DateTime.UtcNow) {
				numOfMonthsBackWeHaveDataFor++;
				tmp = tmp.AddMonths(-1);
			}

			if (numOfMonthsBackWeHaveDataFor > 0) {
				string effectiveLateStatuses = account.Statuses.Substring(0, numOfMonthsBackWeHaveDataFor)
					.Replace("0", string.Empty);
				if (effectiveLateStatuses.Length > 0)
					return true;
			}

			return false;
		}

		private decimal GetRepaymentAmount(Customer customer) {
			var customerSchedule = customer.Loans
				.Where(x => x.Status == LoanStatus.Late || x.Status == LoanStatus.Live)
				.Select(x => x.Schedule)
				.ToList();

			var monthlyRepaymentSum = customerSchedule.Sum(x => x.Sum(y => y.AmountDue));
			var count = customerSchedule.Count;
			var repaymentAmount = count != 0 ? monthlyRepaymentSum / count : 0;
			return repaymentAmount;
		}

		private LightsState ObtainAmlState(Customer customer) {
			if (customer.AMLResult == "Rejected" || customer.BWAResult == "Rejected")
				return LightsState.Reject;
			if (customer.AMLResult == "Warning" || customer.BWAResult == "Warning")
				return LightsState.Warning;
			if (customer.AMLResult == "Not performed" || customer.BWAResult == "Not performed")
				return LightsState.NotPerformed;

			return LightsState.Passed;
		}

		private LightsState ObtainLoanActivityState(decimal latePayments, decimal collection) {
			if (collection > 0)
				return LightsState.Reject;
			if (latePayments > 0)
				return LightsState.Warning;
			return LightsState.Passed;
		}

		private static readonly ASafeLog log = new SafeILog(typeof(ProfileSummaryModelBuilder));
		private readonly CreditBureauModelBuilder creditBureauModelBuilder;
		private readonly ServiceClient serviceClient;
		private MedalCalculationsRepository medalCalculationsRepository;
		private readonly IEzbobWorkplaceContext context;

		public enum ProfileTab {
			[Description("dashboard")]
			Dashboard,
			[Description("profile-summary")]
			ProfileSummary,
			[Description("marketplaces")]
			Marketplaces,
			[Description("payment-accounts")]
			PaymentAccounts,
			[Description("credit-bureau")]
			CreditBureau,
			[Description("calculator")]
			Calculator,
			[Description("logical-glue-history")]
			LogicalGlueHistory,
			[Description("loanhistorys")]
			LoanHistories,
			[Description("customer-info")]
			CustomerInfo,
			[Description("company-score")]
			CompanyScore,
			[Description("messages-tab")]
			MessagesTab,
			[Description("apiChecks")]
			ApiChecks,
			[Description("customerRelations")]
			CustomerRelations,
			[Description("fraudDetection")]
			FraudDetection,
			[Description("properties")]
			Properties
		}//ProfileTab
	}
}
