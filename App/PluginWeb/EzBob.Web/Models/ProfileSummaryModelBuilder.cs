namespace EzBob.Web.Models
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Backend.Models;
	using EZBob.DatabaseLib.Model.Loans;
	using ExperianLib;
	using CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using Areas.Underwriter.Models;
	using EzBob.Models;
	using EzBob.Models.Marketplaces;
	using Ezbob.Utils.Extensions;
	using MoreLinq;
	using NHibernate;
	using Newtonsoft.Json;
	using StructureMap;
	using log4net;

	public class ProfileSummaryModelBuilder
	{
		private readonly IDecisionHistoryRepository _decisions;
		private readonly MarketPlacesFacade _mpFacade;

		public ProfileSummaryModelBuilder(IDecisionHistoryRepository decisions, MarketPlacesFacade mpFacade)
		{
			_decisions = decisions;
			_mpFacade = mpFacade;
		}

		private static readonly ILog Log = LogManager.GetLogger(typeof(ProfileSummaryModelBuilder));

		public ProfileSummaryModel CreateProfile(Customer customer)
		{
			var summary = new ProfileSummaryModel { Id = customer.Id, IsOffline = customer.IsOffline };
			BuildMarketplaces(customer, summary);
			BuildCreditBureau(customer, summary);
			BuildPaymentAccounts(customer, summary);
			AddDecisionHistory(summary, customer);

			BuildRequestedLoan(summary, customer);

			summary.AffordabilityAnalysis =
					new AffordabilityAnalysis
					{
						CashAvailabilityOrDeficits = "Not implemented now",
						EzBobMonthlyRepayment = Money(GetRepaymentAmount(customer))
					};
			summary.LoanActivity = CreateLoanActivity(customer);
			summary.AmlBwa =
				new AmlBwa
				{
					Aml = customer.AMLResult,
					Bwa = customer.BWAResult,
					Lighter = new Lighter(ObtainAmlState(customer))
				};
			summary.FraudCheck = new FraudCheck
				{
					Status = customer.Fraud.ToString(),

				};
			summary.OverallTurnOver = customer.PersonalInfo == null ? null : customer.PersonalInfo.OverallTurnOver;
			summary.WebSiteTurnOver = customer.PersonalInfo == null ? null : customer.PersonalInfo.WebSiteTurnOver;
			summary.Comment = customer.Comment;

			summary.CompanyEmployeeCountInfo = new CompanyEmployeeCountInfo(customer.Company);
			summary.CompanyInfo = CompanyInfoMap.FromCompany(customer.Company);
			summary.IsOffline = customer.IsOffline;

			BuildAlerts(summary, customer);
			return summary;
		}

		private void BuildAlerts(ProfileSummaryModel summary, Customer customer)
		{
			summary.Alerts = new AlertsModel()
				{
					Errors = new List<AlertModel>(),
					Warnings = new List<AlertModel>(),
					Infos = new List<AlertModel>(),
				};


			if (customer.IsTest)
			{
				summary.Alerts.Infos.Add(new AlertModel{ Abbreviation = "Test", Alert = "Is test", AlertType = AlertType.Info.DescriptionAttr() });
			}

			if (customer.CciMark)
			{
				summary.Alerts.Errors.Add(new AlertModel {Abbreviation = "CCI", Alert = "CCI Mark", AlertType = AlertType.Error.DescriptionAttr() });
			}

			if (customer.CollectionStatus.CurrentStatus.IsDefault || customer.CollectionStatus.CurrentStatus.Name == "Bad")
			{
				summary.Alerts.Errors.Add(new AlertModel {Abbreviation = "Bad", Alert = string.Format("Customer Status : {0}", customer.CollectionStatus.CurrentStatus.Name), AlertType = AlertType.Error.DescriptionAttr() });
			}
			else if (customer.CollectionStatus.CurrentStatus.Name == "Risky")
			{
				summary.Alerts.Warnings.Add(new AlertModel { Abbreviation = "Risky", Alert = string.Format("Customer Status : {0}", customer.CollectionStatus.CurrentStatus.Name), AlertType = AlertType.Warning.DescriptionAttr() });
			}

			if (customer.FraudStatus != FraudStatus.Ok)
			{
				summary.Alerts.Errors.Add(new AlertModel {Abbreviation = "F", Alert = string.Format("Fraud Status : {0}", customer.FraudStatus.DescriptionAttr()), AlertType = AlertType.Error.DescriptionAttr() });
			}

			switch (customer.AMLResult)
			{
				case "Rejected":
					summary.Alerts.Errors.Add(new AlertModel {Abbreviation = "AML", Alert = string.Format("AML Status : {0}", customer.AMLResult), AlertType = AlertType.Error.DescriptionAttr() });
					break;
				case "Not performed":
				case "Warning":
					summary.Alerts.Warnings.Add(new AlertModel { Abbreviation = "AML", Alert = string.Format("AML Status : {0}", customer.AMLResult), AlertType = AlertType.Warning.DescriptionAttr() });
					break;
			}

			switch (summary.CreditBureau.ThinFile)
			{
				case "Yes":
					summary.Alerts.Errors.Add(new AlertModel {Abbreviation="TF", Alert = "Thin file", AlertType = AlertType.Error.DescriptionAttr() });
					break;
				case "N/A":
					summary.Alerts.Warnings.Add(new AlertModel {Abbreviation="N/A", Alert = "Couldn't get financial accounts", AlertType = AlertType.Warning.DescriptionAttr() });
					break;
			}

			if (customer.CustomerRelationStates.Any())
			{
				var state = customer.CustomerRelationStates.First();
				if (state.IsFollowUp.HasValue && state.IsFollowUp.Value && state.FollowUp.FollowUpDate <= DateTime.UtcNow)
				{
					summary.Alerts.Errors.Add(new AlertModel { Abbreviation = "Follow", Alert = "Customer relations follow up date is due " + state.FollowUp.FollowUpDate.ToString("dd/MM/yyyy"), AlertType = AlertType.Error.DescriptionAttr() });
				}
			}
			BuildLandRegistryAlerts(customer, summary);
		}

		private void BuildLandRegistryAlerts(Customer customer, ProfileSummaryModel summary)
		{
			var lrs = customer.LandRegistries.Where(x =>
				x.RequestType == LandRegistryLib.LandRegistryRequestType.Res &&
				x.ResponseType == LandRegistryLib.LandRegistryResponseType.Success).ToList();
			if (!lrs.Any() && customer.PersonalInfo != null && String.Equals(customer.PersonalInfo.ResidentialStatus,"home owner", StringComparison.OrdinalIgnoreCase))
			{
				summary.Alerts.Warnings.Add(new AlertModel
					{
						Abbreviation = "LR",
						Alert = "No land registries retrieved",
						AlertType = AlertType.Warning.DescriptionAttr()
					});
			}
			
			if (lrs.Any())
			{
				var owners = lrs.SelectMany(x => x.Owners).Select(x => new {firstName = x.FirstName ?? string.Empty, lastName = x.LastName ?? string.Empty, company = x.CompanyName ?? string.Empty}).ToList();
				if (owners.Any() && !owners.Any(owner =>
						owner.firstName.ToLowerInvariant().Contains(customer.PersonalInfo.FirstName.Trim().ToLowerInvariant()) &&
						owner.lastName.ToLowerInvariant().Contains(customer.PersonalInfo.Surname.Trim().ToLowerInvariant())))
				{
					var ownerNames = owners.Select(x => string.Format("{0} {1} {2}", x.firstName, x.lastName, x.company)).Aggregate((a, b) => a + ", " + b);
					summary.Alerts.Errors.Add(new AlertModel
					{
						Abbreviation = "LR",
						Alert = "Not a land registry owner",
						AlertType = AlertType.Error.DescriptionAttr(),
						Tooltip = string.Format("Owners list: {0}", ownerNames)
					});
				}
			}
		}

		private static void BuildRequestedLoan(ProfileSummaryModel summary, Customer customer)
		{
			var rl = new CustomerRequestedLoanModel();
			var requestedLoan = customer.CustomerRequestedLoan.FirstOrDefault();
			if (requestedLoan != null)
			{
				rl.Amount = requestedLoan.Amount;
				rl.Created = requestedLoan.Created;
				rl.CustomerReason = requestedLoan.CustomerReason == null ? null : requestedLoan.CustomerReason.Reason;
				rl.CustomerSourceOfRepayment = requestedLoan.CustomerSourceOfRepayment == null ? null : requestedLoan.CustomerSourceOfRepayment.SourceOfRepayment;
				rl.OtherReason = requestedLoan.OtherReason;
				rl.OtherSourceOfRepayment = requestedLoan.OtherSourceOfRepayment;
			}
			summary.RequestedLoan = rl;
		}
		private static void BuildCreditBureau(Customer customer, ProfileSummaryModel summary)
		{
			var creditBureau = new CreditBureau();
			var consumerSrv = new ConsumerService();

			try
			{
				var loc = new EzBobIntegration.Web_References.Consumer.InputLocationDetailsMultiLineLocation();
				if (customer.AddressInfo.PersonalAddress.Any())
				{
					var customerMainAddress = customer.AddressInfo.PersonalAddress.First();

					loc.LocationLine1 = customerMainAddress.Line1;
					loc.LocationLine2 = customerMainAddress.Line2;
					loc.LocationLine3 = customerMainAddress.Line3;
					loc.LocationLine4 = customerMainAddress.Town;
					loc.LocationLine5 = customerMainAddress.County;
					loc.LocationLine6 = customerMainAddress.Postcode;
				}
				ConsumerServiceResult result = null;
				if (customer.PersonalInfo != null && !string.IsNullOrEmpty(customer.PersonalInfo.FirstName))
				{
					result = consumerSrv.GetConsumerInfo(customer.PersonalInfo.FirstName, customer.PersonalInfo.Surname,
					   customer.PersonalInfo.Gender.ToString(), // should be Gender
					   customer.PersonalInfo.DateOfBirth, null, loc, "PL", customer.Id, 0, true, false, false);
				}
				if (result != null)
				{
					creditBureau.CreditBureauScore = result.BureauScore;
					creditBureau.TotalDebt = result.TotalAccountBalances;
					creditBureau.TotalMonthlyRepayments = result.SumOfRepayements;
					creditBureau.CreditCardBalances = result.CreditCardBalances;
					creditBureau.BorrowerType =
						TypeOfBusinessExtenstions.TypeOfBussinessForWeb(customer.PersonalInfo.TypeOfBusiness);

					//creditBureau.Lighter = new Lighter(ObtainCreditBureauState(result.ExperianResult));
					creditBureau.FinancialAccounts = customer.FinancialAccounts;
					var isHasFinancialAccout = false;

					try {
						isHasFinancialAccout = result.Output.Output.FullConsumerData.ConsumerData.CAIS.Any(x => x.CAISDetails.Any());
					}
					catch (Exception ex) {
						Log.InfoFormat("Can't read value for isHasFinancialAccount because of exception: {0}", ex.Message);
					} // try

					creditBureau.ThinFile = !isHasFinancialAccout ? "Yes" : "No";
				}
				else
				{
					creditBureau.ThinFile = "N/A";
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}

			summary.CreditBureau = creditBureau;
		}

		private decimal GetRepaymentAmount(Customer customer)
		{
			var customerSchedule = customer.Loans
				.Where(x => x.Status == LoanStatus.Late || x.Status == LoanStatus.Live)
				.Select(x => x.Schedule)
				.ToList();

			var monthlyRepaymentSum = customerSchedule.Sum(x => x.Sum(y => y.AmountDue));
			var count = customerSchedule.Count();
			var repaymentAmount = count != 0 ? monthlyRepaymentSum / count : 0;
			return repaymentAmount;
		}

		private void BuildMarketplaces(Customer customer, ProfileSummaryModel summary)
		{
			double? anualTurnOver = 0;

			double? totalPositiveReviews = 0;
			double? totalNegativeReviews = 0;
			double? totalNeutralReviews = 0;
			var inventory = 0d;
			var marketplacesAll = customer.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.IsPaymentAccount == false).ToList();
			var marketplaces =
				marketplacesAll.Where(mp => mp.Disabled == false && string.IsNullOrEmpty(mp.UpdateError)).ToList();
			var isNewExist = marketplacesAll.Any(mp => mp.IsNew);

			foreach (var mp in marketplaces)
			{
				var analisysFunction = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(mp.Id);
				var av = analisysFunction.Data
					.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;

				var lastAnualTurnover = av != null
					? av.LastOrDefault(
						x =>
							x.ParameterName == "Total Sum of Orders" && x.TimePeriod.TimePeriodType <= TimePeriodEnum.Year)
					: null;
				anualTurnOver += lastAnualTurnover != null
					? Double.Parse(lastAnualTurnover.Value.ToString(), CultureInfo.InvariantCulture)
					: 0;

				inventory += av != null
					? av.Where(
						x =>
							x.ParameterName == "Total Value of Inventory" &&
							x.TimePeriod.TimePeriodType == TimePeriodEnum.Lifetime)
						.Sum(x => Double.Parse(x.Value.ToString(), CultureInfo.InvariantCulture))
					: 0;

				var isAmazon = mp.Marketplace.Name == "Amazon";
				var amazonFeedback = mp.AmazonFeedback.LastOrDefault();
				var ebayFeedBack = mp.EbayFeedback.LastOrDefault();

				var feedbackByPeriodAmazon = amazonFeedback != null
					? amazonFeedback.FeedbackByPeriodItems.FirstOrDefault(x => x.TimePeriod.Id == 4)
					: null;
				var feedbackByPeriodEbay = ebayFeedBack != null
					? ebayFeedBack.FeedbackByPeriodItems.FirstOrDefault(x => x.TimePeriod.Id == 4)
					: null;

				totalNegativeReviews += isAmazon
					? (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Negative : 0)
					: (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Negative : 0);
				totalPositiveReviews += isAmazon
					? (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Positive : 0)
					: (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Positive : 0);
				totalNeutralReviews += isAmazon
					? (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Neutral : 0)
					: (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Neutral : 0);
			}

			var totalReviews = totalNegativeReviews + totalPositiveReviews + totalNeutralReviews;

			summary.MarketPlaces =
				new MarketPlaces
				{
					NumberOfStores = String.Format("{0} / {1}", marketplaces.Count, marketplacesAll.Count),
					AnualTurnOver = anualTurnOver,
					Inventory = Money(inventory),
					Seniority = Money(GetSeniority(customer, false)),
					TotalPositiveReviews =
						String.Format("{0:0.#} ({1:0.#}%)", totalPositiveReviews,
							(totalReviews != 0 ? totalPositiveReviews / totalReviews * 100 : 0)),
					Lighter = new Lighter(ObtainMarketPlacesState(marketplaces)),
					IsNew = isNewExist
				};
		}

		private void BuildPaymentAccounts(Customer customer, ProfileSummaryModel summary)
		{
			var paymentAccounts = customer.GetPaymentAccounts();
			summary.PaymentAccounts =
				new PaymentAccounts
				{
					NumberOfPayPalAccounts = String.Format("{0}", paymentAccounts.Count()),
					Balance = "Not implemented now",
					NetExpences = String.Format("{0}", paymentAccounts.Sum(x => x.TotalNetOutPayments)),
					NetIncome = paymentAccounts.Sum(x => x.TotalNetInPayments),
					Lighter = new Lighter(ObtainPaymentsAccountsState(customer)),
					Seniority = Money(GetSeniority(customer, true)),
					IsNew = paymentAccounts.Any(x => x.IsNew)
				};
		}

		private double GetSeniority(Customer customer, bool isPaymentAccountOnly)
		{
			var marketplacesSeniority = _mpFacade.MarketplacesSeniority(customer, false, isPaymentAccountOnly);
			var minAccountAge = DateTime.UtcNow - marketplacesSeniority;
			var minAccountAgeTotalMonth = minAccountAge.TotalDays / 30;
			return minAccountAgeTotalMonth;
		}

		private LoanActivity CreateLoanActivity(Customer customer)
		{
			var previousLoans = customer.Loans.Count(x => x.DateClosed != null);
			var currentBalance = customer.Loans.Sum(x => x.Balance);
			var latePayments = customer.Loans.Sum(x => x.PastDues);
			var interest = customer.Loans.Where(l => l.Status == LoanStatus.Late).Sum(l => l.InterestDue);
			var collection = customer.Loans.Where(x => x.IsDefaulted).Sum(x => x.PastDues);
			var lateStatus = customer.PaymentDemenaor.ToString();

			int currentLateDays = 0;
			DateTime now = DateTime.UtcNow;
			var lateLoans = customer.Loans.Where(l => l.Status == LoanStatus.Late);
			foreach (Loan l in lateLoans)
			{
				foreach (LoanScheduleItem ls in l.Schedule)
				{
					int lateInDays = (int)(now - ls.Date).TotalDays;
					if (lateInDays > currentLateDays)
					{
						currentLateDays = lateInDays;
					}
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
			var totalIssuesCount = customer.Loans.Count();
			var repaid = customer.Loans.Sum(l => l.Repayments);
			var repaidCount = customer.Loans.Count(l => l.DateClosed.HasValue);
			var active = customer.Loans.Sum(l => l.Balance);
			var activeCount = customer.Loans.Count(l => !l.DateClosed.HasValue);
			var earnedInterest = customer.Loans.Sum(l => l.InterestPaid);

			var activeLoans = new List<ActiveLoan>();
			int i = 0;
			var loans = customer.Loans.OrderBy(l => l.Date);
			foreach (var loan in loans)
			{
				i++;
				if (!loan.DateClosed.HasValue)
				{
					try
					{
						var agreement = JsonConvert.DeserializeObject<AgreementModel>(loan.AgreementModel);
						activeLoans.Add(new ActiveLoan
							{
								Approved = loan.CashRequest.ManagerApprovedSum,
								Balance = loan.Principal,
								LoanAmount = loan.LoanAmount,
								LoanAmountPercent =
									loan.CashRequest.ManagerApprovedSum.HasValue
										? loan.LoanAmount/(decimal) loan.CashRequest.ManagerApprovedSum.Value
										: 0,
								LoanDate = loan.Date,
								InterestRate = loan.InterestRate,
								IsLate = loan.Status == LoanStatus.Late,
								IsEU = loan.LoanSource.Name == "EU",
								BalanceWidthPercent = loan.CashRequest.ManagerApprovedSum.HasValue
										? loan.Principal / (decimal)loan.CashRequest.ManagerApprovedSum.Value
										: 0,
								BalancePercent = loan.Principal / loan.LoanAmount,
								Term = agreement.Term,
								TermApproved = loan.CashRequest.ApprovedRepaymentPeriod.HasValue 
									? loan.CashRequest.ApprovedRepaymentPeriod.Value 
									: loan.CashRequest.RepaymentPeriod,
								TotalFee = loan.SetupFee,
								LoanNumber = i,
								Comment = loan.CashRequest.UnderwriterComment
							});
					}
					catch (Exception ex)
					{
						Log.Error("Failed to build current loans model", ex);
					}
				}
			}

			if (activeLoans.Any())
			{
				var maxLoan = activeLoans.MaxBy(x => x.Approved);

				maxLoan.WidthPercent = 1;

				foreach (var activeLoan in activeLoans)
				{
					if (activeLoan.Approved.HasValue)
					{
						activeLoan.WidthPercent = maxLoan.Approved.HasValue ? activeLoan.Approved.Value/maxLoan.Approved.Value : 0;
					}
					activeLoan.BalanceWidthPercent *= (decimal) activeLoan.WidthPercent;
					activeLoan.LoanAmountWidthPercent = activeLoan.LoanAmountPercent*(decimal) activeLoan.WidthPercent;
				}
			}

			return new LoanActivity
				{
					PreviousLoans = Money(previousLoans),
					CurrentBalance = Money(currentBalance),
					LatePaymentsSum = Money(latePayments),
					Collection = Money(collection),
					LateInterest = Money((decimal)interest),
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

		private static string Money<T>(T amount) where T : IFormattable
		{
			return String.Format("{0:0.#}", amount);
		}

		private void AddDecisionHistory(ProfileSummaryModel summary, Customer customer)
		{
			summary.DecisionHistory = _decisions.ByCustomer(customer).Select(DecisionHistoryModel.Create).OrderBy(x => x.Date).ToList();

			//Dashboard
			summary.Decisions = new DecisionsModel
				{
					TotalDecisionsCount = summary.DecisionHistory.Count,
					TotalApprovedAmount = summary.DecisionHistory.Where(dh => dh.Action == "Approve").Sum(dh => dh.ApprovedSum),
					RejectsCount = summary.DecisionHistory.Count(dh => dh.Action == "Reject")
				};
			var lastApprove = summary.DecisionHistory.OrderByDescending(dh => dh.Date).FirstOrDefault(dh => dh.Action == "Approve");
			if (lastApprove != null)
			{
				summary.Decisions.LastInterestRate = lastApprove.InterestRate;
				summary.Decisions.LastDecisionDate = lastApprove.Date;
			}
		}


		private LightsState ObtainLoanActivityState(decimal latePayments, decimal collection)
		{
			if (collection > 0)
				return LightsState.Reject;
			if (latePayments > 0)
				return LightsState.Warning;
			return LightsState.Passed;
		}

		private LightsState ObtainMarketPlacesState(List<MP_CustomerMarketPlace> marketplaces)
		{
			if (marketplaces.Any(x => (!String.IsNullOrEmpty(x.UpdateError))))
			{
				return LightsState.Error;
			}
			
			var session = ObjectFactory.GetInstance<ISession>();
			foreach (MP_CustomerMarketPlace mp in marketplaces)
			{
				string currentState = (string) session.CreateSQLQuery(string.Format("EXEC GetLastMarketplaceStatus {0}, {1}", mp.Customer.Id, mp.Id)).UniqueResult();
				if (currentState == "In progress" || currentState == "BG launch")
				{
					return LightsState.InProgress;
				}
			}

			return LightsState.Passed;
		}

		private LightsState ObtainAmlState(Customer customer)
		{
			if (customer.AMLResult == "Rejected" || customer.BWAResult == "Rejected")
				return LightsState.Reject;
			if (customer.AMLResult == "Warning" || customer.BWAResult == "Warning")
				return LightsState.Warning;
			if (customer.AMLResult == "Not performed" || customer.BWAResult == "Not performed")
				return LightsState.NotPerformed;

			return LightsState.Passed;
		}

		private LightsState ObtainPaymentsAccountsState(Customer customer)
		{
			if (customer.BWAResult == "Warning")
				return LightsState.Warning;
			if (customer.BWAResult == "Rejected")
				return LightsState.Warning;

			return LightsState.Passed;
		}
	}
}