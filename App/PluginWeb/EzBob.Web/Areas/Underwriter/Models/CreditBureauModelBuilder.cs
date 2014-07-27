namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Xml.Serialization;
	using ConfigManager;
	using ExperianLib;
	using ExperianLib.Dictionaries;
	using ExperianLib.Ebusiness;
	using ExperianLib.IdIdentityHub;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using Infrastructure;
	using MoreLinq;
	using NHibernate;
	using NHibernate.Linq;
	using System.Text;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;
	using Web.Models;
	using log4net;
	using EZBob.DatabaseLib.Model.Experian;

	public class CreditBureauModelBuilder
	{
		private readonly ISession _session;
		private readonly ICustomerRepository _customers;
		private static readonly ILog Log = LogManager.GetLogger(typeof(CreditBureauModelBuilder));
		private readonly IExperianHistoryRepository _experianHistoryRepository;
		private const int ConsumerScoreMax = 1400;
		private const int ConsumerScoreMin = 120;
		private const int CompanyScoreMax = 100;
		private const int CompanyScoreMin = 0;
		private readonly ServiceClient serviceClient;
		private readonly IWorkplaceContext context = ObjectFactory.GetInstance<IWorkplaceContext>();
		private readonly AConnection m_oDB;

		public CreditBureauModelBuilder(ISession session,
			ICustomerRepository customers,
			IExperianHistoryRepository experianHistoryRepository)
		{
			_session = session;
			_customers = customers;
			_experianHistoryRepository = experianHistoryRepository;
			Errors = new List<string>();
			serviceClient = new ServiceClient();
			m_oDB = DbConnectionGenerator.Get(new SafeILog(Log));
		}

		public CreditBureauModel Create(Customer customer, bool getFromLog = false, long? logId = null)
		{
			Log.DebugFormat("CreditBureauModel Create customerid: {0} hist: {1} histId: {2}", customer.Id, getFromLog, logId);
			var model = new CreditBureauModel { ErrorList = new List<string>() };
			var customerMainAddress = customer.AddressInfo.PersonalAddress.ToList().FirstOrDefault();

			//registered customer
			if (customerMainAddress == null)
			{
				model.CheckStatus = "Error";
				model.CheckValidity = "Registered customer no address";
				return model;
			}

			try
			{
				ConsumerServiceResult result;
				GetConsumerInfo(customer, getFromLog, logId, customerMainAddress, out result);
				GenerateConsumerModel(model, customer.Id, result);
				CreatePersonalDataModel(model, customer);
				AppendAmlInfo(model.AmlInfo, customer, customerMainAddress);
				AppendBavInfo(model.BavInfo, customer, customerMainAddress);
				BuildEBusinessModel(customer, model, getFromLog, logId);
				BuildSummaryModel(model);
				BuildHistoryModel(model, customer);
			}
			catch (Exception e)
			{
				Log.DebugFormat("CreditBureauModel Create Exception {0} ", e);
				model.ErrorList.Add(e.Message);
			}

			model.ErrorList.AddRange(Errors);
			return model;
		}

		public void GetConsumerInfo(Customer customer,
			bool getFromLog, long? logId, CustomerAddress customerMainAddress, out ConsumerServiceResult result)
		{

			if (getFromLog && logId.HasValue)
			{
				var response =
					_session.Query<MP_ServiceLog>().FirstOrDefault(x => x.Id == logId.Value);

				if (response == null || string.IsNullOrEmpty(response.ResponseData))
				{
					Log.DebugFormat("GetConsumerInfo no service log is found for customer: {0} id: {1}", customer.Id, logId.Value);
					result = null;
					return;
				}
				var serializer = new XmlSerializer(typeof(OutputRoot));
				using (TextReader sr = new StringReader(response.ResponseData))
				{
					var output = (OutputRoot)serializer.Deserialize(sr);
					result = new ConsumerServiceResult(output);
				}
			}
			else
			{
				var loc = MultiLineLocationFromCustomerAddress(customerMainAddress);
				var consumerSrv = new ConsumerService();
				result = consumerSrv.GetConsumerInfo(customer.PersonalInfo.FirstName, customer.PersonalInfo.Surname,
					customer.PersonalInfo.Gender.ToString(), // should be Gender
					customer.PersonalInfo.DateOfBirth, null, loc, "PL", customer.Id, 0, true, false, false);
			}
		}

		private void BuildHistoryModel(CreditBureauModel model, Customer customer)
		{
			var consumerHistory = _experianHistoryRepository.GetConsumerHistory(customer).ToList();
			if (consumerHistory.Any())
			{
				model.ConsumerHistory = consumerHistory.Select(x => new CheckHistoryModel
				{
					Date = x.Date.ToUniversalTime(),
					Id = x.Id,
					Score = x.Score,
					CII = x.CII.HasValue ? x.CII.Value : -1,
					Balance = x.CaisBalance.HasValue ? x.CaisBalance.Value : -1
				}).OrderByDescending(h => h.Date);
			}
			else
			{
				var consumerResponses = (from s in _session.Query<MP_ServiceLog>()
										 where s.Director == null
										 where s.Customer.Id == customer.Id
										 where s.ServiceType == ExperianServiceType.Consumer.DescriptionAttr()
										 select new { Id = s.Id, InsertDate = s.InsertDate });
				var checkConsumerHistoryModels = new List<CheckHistoryModel>();
				foreach (var res in consumerResponses)
				{
					ConsumerServiceResult result;
					GetConsumerInfo(customer, true, res.Id, null, out result);
					var consumerModel = model;
					int cii = consumerModel.CII;
					checkConsumerHistoryModels.Add(new CheckHistoryModel
						{
							Id = res.Id,
							Score = consumerModel.Score,
							CII = cii,
							Balance = consumerModel.ConsumerAccountsOverview.Balance_Total,
							Date = res.InsertDate
						});
				}

				model.ConsumerHistory = checkConsumerHistoryModels.OrderByDescending(h => h.Date);

				foreach (var cModel in checkConsumerHistoryModels)
				{
					_experianHistoryRepository.Save(new MP_ExperianHistory
					{
						Customer = customer,
						ServiceLogId = cModel.Id,
						Type = ExperianServiceType.Consumer.DescriptionAttr(),
						Date = cModel.Date,
						Score = cModel.Score,
						CII = cModel.CII,
						CaisBalance = cModel.Balance
					});
				}
			}

			var isLimited = customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited;
			Log.DebugFormat("BuildHistoryModel company type: {0}", isLimited ? "Limited" : "NonLimited");
			var companyHistory = _experianHistoryRepository.GetCompanyHistory(customer, isLimited).ToList();
			if (companyHistory.Any())
			{
				Log.Debug("BuildHistoryModel company from history table");
				model.CompanyHistory = companyHistory.Select(x => new CheckHistoryModel
				{
					Date = x.Date.ToUniversalTime(),
					Id = x.Id,
					Score = x.Score,
					Balance = x.CaisBalance
				}).OrderByDescending(h => h.Date);
			}
			else
			{
				Log.Debug("BuildHistoryModel company from mp_servicelog table");
				var type = (isLimited ? ExperianServiceType.LimitedData.DescriptionAttr() : ExperianServiceType.NonLimitedData.DescriptionAttr());
				var checkCompanyHistoryModels = (
					from s in _session.Query<MP_ServiceLog>()
					where s.Director == null
					where s.Customer.Id == customer.Id
					where s.ServiceType == type
					select isLimited ? GetLimitedHistory(s.InsertDate, s.Id) : new CheckHistoryModel {
						Date = s.InsertDate.ToUniversalTime(),
						Id = s.Id,
						Score = GetNonLimitedScore(customer.Id, customer.Company.ExperianRefNum),
						Balance = (decimal?)null
					}
				).ToList();

				model.CompanyHistory = checkCompanyHistoryModels.Where(h => h != null).OrderByDescending(h => h.Date);
				foreach (var cModel in checkCompanyHistoryModels)
				{
					if (cModel != null)
					{
						_experianHistoryRepository.Save(new MP_ExperianHistory
							{
								Customer = customer,
								ServiceLogId = cModel.Id,
								Type = type,
								Date = cModel.Date,
								Score = cModel.Score,
							});
					}
				}
			}
		}

		private CheckHistoryModel GetLimitedHistory(DateTime date, long id) {
			ComapanyDashboardModel m = new CompanyScoreModelBuilder().BuildLimitedDashboardModel(id);

			return new CheckHistoryModel {
				Date = date,
				Id = id,
				Balance = m.CaisBalance,
				Score = m.Score,
			};
		} // GetLimitedHistory

		private void CreatePersonalDataModel(CreditBureauModel model, Customer customer)
		{
			model.Name = customer.PersonalInfo.FirstName;
			model.MiddleName = customer.PersonalInfo.MiddleInitial;
			model.Surname = customer.PersonalInfo.Surname;
			model.FullName = customer.PersonalInfo.Fullname;

			model.BorrowerType = customer.PersonalInfo.TypeOfBusiness.ToString();
			model.ConsumerSummaryCharacteristics.DSRandOwnershipType = customer.PersonalInfo.ResidentialStatus;

			model.AmlInfo = new AMLInfo
			{
				AMLResult = string.IsNullOrEmpty(customer.AMLResult)
					? "Verification was not performed"
					: customer.AMLResult
			};

			model.BavInfo = new BankAccountVerificationInfo
			{
				BankAccountVerificationResult = string.IsNullOrEmpty(customer.BWAResult)
					? "Verification was not performed"
					: customer.BWAResult
			};
			customer.FinancialAccounts = model.AccountsInformation == null ? 0 : model.AccountsInformation.Length;
			_customers.Update(customer);
		}

		public void GenerateConsumerModel(CreditBureauModel model, int id, ConsumerServiceResult eInfo)
		{
			model.ErrorList = model.ErrorList ?? new List<string>();
			if (eInfo == null || eInfo.Data == null)
			{
				model.HasExperianError = true;
				model.ErrorList.Add("No data");
				return;
			}

			var scorePosColor = GetScorePositionAndColor(eInfo.Data.BureauScore, ConsumerScoreMax, ConsumerScoreMin);
			var checkStatus = (eInfo.Data.HasExperianError) ? "Error" : eInfo.ExperianResult;

			var checkIcon = "icon-white icon-remove-sign";
			var buttonStyle = "btn-danger";
			switch (checkStatus)
			{
				case "Passed":
					checkIcon = "icon-white icon-ok-sign";
					buttonStyle = "btn-success";
					break;
				case "Referred":
					checkIcon = "icon-white icon-question-sign";
					buttonStyle = "btn-warning";
					break;
				case "Rejected":
					checkIcon = "icon-white icon-remove-sign";
					buttonStyle = "btn-danger";
					break;
			}

			var checkDate = eInfo.LastUpdateDate;
			var checkValidity = checkDate.AddMonths(3);

			Errors = new List<string>();

			model.Id = id;
			model.HasExperianError = eInfo.Data.HasExperianError;
			model.ModelType = "Consumer";
			model.CheckStatus = checkStatus;
			model.CheckIcon = checkIcon;
			model.ButtonStyle = buttonStyle;
			model.CheckDate = checkDate.ToShortDateString();
			model.CheckValidity = checkValidity.ToShortDateString();
			model.BorrowerType = "Consumer";
			model.Score = eInfo.Data.BureauScore;
			model.Odds = Math.Pow(2, ((double)eInfo.Data.BureauScore - 600) / 80);
			model.ScorePosition = scorePosColor.Position;
			model.ScoreAlign = scorePosColor.Align;
			model.ScoreValuePosition = scorePosColor.ValPosition;
			model.ScoreColor = scorePosColor.Color;
			model.ApplicantFullName = eInfo.Data.Applicants.Select(applicant =>
				string.Format("{0} {1} {2} {3}", applicant.Forename, applicant.MiddleName, applicant.Surname, applicant.DateOfBirth.HasValue ?
				(DateTime.UtcNow.Year - applicant.DateOfBirth.Value.Year).ToString(CultureInfo.InvariantCulture) : "")).Aggregate((x, y) => x + "," + y);

			model.ConsumerAccountsOverview = new ConsumerAccountsOverview();
			model.CII = eInfo.Data.CII;
			if (!string.IsNullOrEmpty(eInfo.Data.Error))
			{
				model.ErrorList.AddRange(eInfo.Data.Error.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
			}

			model.ConsumerSummaryCharacteristics = new ConsumerSummaryCharacteristics
			{
				NumberOfAccounts = 0,
				NumberOfAccounts3M = 0,
				WorstCurrentStatus = AccountStatusDictionary.GetDetailedAccountStatusString(eInfo.Data.WorstCurrentStatus),
				WorstCurrentStatus3M = AccountStatusDictionary.GetDetailedAccountStatusString(eInfo.Data.WorstHistoricalStatus),
				EnquiriesLast3M = eInfo.Data.EnquiriesLast3Months,
				EnquiriesLast6M = eInfo.Data.EnquiriesLast6Months,
				NumberOfDefaults = 0,
				NumberOfCCOverLimit = eInfo.Data.CreditCardOverLimit,
				CreditCardUtilization = eInfo.Data.CreditLimitUtilisation,
				DSRandOwnershipType = string.Empty,
				NOCsOnCCJ = eInfo.Data.NOCsOnCCJ,
				NOCsOnCAIS = eInfo.Data.NOCsOnCAIS,
				NumberOfCCJs = eInfo.Data.NumCCJs,
				SatisfiedJudgements = eInfo.Data.SatisfiedJudgement,
				AgeOfMostRecentCCJ = eInfo.Data.CCJLast2Years,
				CAISSpecialInstructionFlag = eInfo.Data.CAISSpecialInstructionFlag,


			};
			model.ConsumerAccountsOverview = new ConsumerAccountsOverview();


			var accList = new List<AccountInfo>();

			var years = new List<AccountDisplayedYear>();
			var quarters = new List<AccountDisplayedQuarter>();
			var monthsList = new List<string>();

			var displayedMonths = new List<DateTime>();
			var mthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

			for (int i = 1 - StatusHistoryMonths; i <= 0; i++)
			{
				var date = mthStart.AddMonths(i);
				displayedMonths.Add(date);
				var monthYear = date.Year;
				var monthQ = (date.Month - 1) / 3 + 1;
				var monthQStr = string.Format("Q{0}", monthQ);
				var month = date.Month;
				if ((years.Count > 0) && (years[years.Count - 1].Year == monthYear))
					years[years.Count - 1].Span++;
				else
					years.Add(new AccountDisplayedYear { Year = date.Year, Span = 1 });

				if ((quarters.Count > 0) && (quarters[quarters.Count - 1].Quarter == monthQStr))
					quarters[quarters.Count - 1].Span++;
				else
					quarters.Add(new AccountDisplayedQuarter { Quarter = monthQStr, Span = 1 });
				monthsList.Add(month.ToString("00"));
			}

			var numberOfAccounts = 0;
			var numberOfAcc3M = 0;

			// 0 - CC, 1 - Mortgage, 2 - PL, 3 - other
			var accounts = new[] { 0, 0, 0, 0 };
			var worstStatus = new[] { "0", "0", "0", "0" };
			var limits = new[] { 0, 0, 0, 0 };
			var balances = new[] { 0, 0, 0, 0 };
			int numberOfDefaults = 0;
			int defaultAmount = 0;
			int numberOfLates = 0;
			string lateStatus = "0";

			foreach (var caisDetails in eInfo.Data.Cais)
			{
				var accountInfo = new AccountInfo();
				//check which acccount type show
				Variables var = map[caisDetails.MatchTo];

				var isShowThisFinancinalAccount = CurrentValues.Instance[var];
				if (isShowThisFinancinalAccount == null || !isShowThisFinancinalAccount)
				{
					continue;
				}

				accountInfo.MatchTo = var.DescriptionAttr();
				accountInfo.OpenDate = caisDetails.CAISAccStartDate;
				accountInfo.Account = AccountTypeDictionary.GetAccountType(caisDetails.AccountType);
				var accStatus = caisDetails.AccountStatus;
				string dateType;
				accountInfo.AccountStatus = GetAccountStatusString(accStatus, out dateType);
				accountInfo.DateType = dateType;
				if (accStatus == DefaultCaisStatusName)
				{
					numberOfDefaults++;
					defaultAmount += caisDetails.CurrentDefBalance.HasValue ? caisDetails.CurrentDefBalance.Value : 0;
				}

				var accType = GetAccountType(caisDetails.AccountType);
				if (accType < 0)
					continue;

				if (((accStatus == DelinquentCaisStatusName) || (accStatus == ActiveCaisStatusName)) && var == Variables.FinancialAccounts_MainApplicant)
				{
					accounts[accType]++;
					var ws = caisDetails.WorstStatus;
					worstStatus[accType] = GetWorstStatus(worstStatus[accType], ws);
					limits[accType] += caisDetails.CreditLimit.HasValue ? caisDetails.CreditLimit.Value : 0;
					balances[accType] += caisDetails.Balance.HasValue ? caisDetails.Balance.Value : 0;

					numberOfAccounts++;
					if ((accountInfo.OpenDate.HasValue) && (accountInfo.OpenDate.Value >= DateTime.Today.AddMonths(-3)))
						numberOfAcc3M++;

					if (accStatus == DelinquentCaisStatusName)
					{
						numberOfLates++;
						lateStatus = GetWorstStatus(lateStatus, ws);
					}
				}

				string statuses = caisDetails.AccountStatusCodes ?? string.Empty;
				var sList = new List<AccountStatus>();
				for (int i = 0; i < StatusHistoryMonths; i++)
				{
					sList.Add(new AccountStatus { Status = "", StatusColor = "white" });
				}

				int relevantYear, relevantMonth, relevantDay;
				if (caisDetails.SettlementDate != null)
				{
					relevantYear = caisDetails.SettlementDate.Value.Year;
					relevantMonth = caisDetails.SettlementDate.Value.Month;
					relevantDay = caisDetails.SettlementDate.Value.Day;
				}
				else
				{
					relevantYear = caisDetails.LastUpdatedDate.Value.Year;
					relevantMonth = caisDetails.LastUpdatedDate.Value.Month;
					relevantDay = caisDetails.LastUpdatedDate.Value.Day;
				}

				var histStart = new DateTime(relevantYear, relevantMonth, 1);
				accountInfo.SettlementDate = new DateTime(relevantYear, relevantMonth, relevantDay);

				for (int i = 0; i < caisDetails.NumOfMonthsHistory; i++)
				{
					var histDate = histStart.AddMonths(-i);
					string indicator = (statuses.Length > i) ? statuses.Substring(i, 1) : string.Empty;
					var idx = displayedMonths.IndexOf(histDate);
					if (idx >= 0)
					{
						sList[idx].Status = AccountStatusDictionary.GetAccountStatusString(indicator);
						sList[idx].StatusColor = AccountStatusDictionary.GetAccountStatusColor(indicator);
					}
				}

				accountInfo.LatestStatuses = sList.ToArray();
				accountInfo.TermAndfreq = GetRepaymentPeriodString(caisDetails.RepaymentPeriod);
				accountInfo.Limit = caisDetails.CreditLimit;
				accountInfo.AccBalance = caisDetails.Balance;

				foreach (var cardHistory in caisDetails.CardHistories)
				{
					accountInfo.CashWithdrawals = string.Format("{0} ({1})", cardHistory.NumCashAdvances, cardHistory.CashAdvanceAmount);
					accountInfo.MinimumPayment = cardHistory.PaymentCode ?? string.Empty;
					break;
				}

				accountInfo.Years = years.ToArray();
				accountInfo.Quarters = quarters.ToArray();
				accountInfo.MonthsDisplayed = monthsList.ToArray();
				accList.Add(accountInfo);
			}

			model.ConsumerSummaryCharacteristics.NumberOfAccounts = numberOfAccounts;
			model.ConsumerSummaryCharacteristics.NumberOfAccounts3M = numberOfAcc3M;
			model.ConsumerSummaryCharacteristics.NumberOfDefaults = numberOfDefaults;
			model.ConsumerSummaryCharacteristics.NumberOfLates = numberOfLates;
			model.ConsumerSummaryCharacteristics.LateStatus = AccountStatusDictionary.GetDetailedAccountStatusString(lateStatus);
			model.ConsumerSummaryCharacteristics.DefaultAmount = defaultAmount;

			Log.DebugFormat("Accounts List length: {0}", accList.Count);
			accList.Sort(new AccountInfoComparer());

			model.AccountsInformation = accList.ToArray();

			model.ConsumerAccountsOverview.OpenAccounts_CC = accounts[0];
			model.ConsumerAccountsOverview.OpenAccounts_Mtg = accounts[1];
			model.ConsumerAccountsOverview.OpenAccounts_PL = accounts[2];
			model.ConsumerAccountsOverview.OpenAccounts_Other = accounts[3];
			model.ConsumerAccountsOverview.OpenAccounts_Total = accounts.Sum();

			model.ConsumerAccountsOverview.WorstArrears_CC = AccountStatusDictionary.GetDetailedAccountStatusString(worstStatus[0]);
			model.ConsumerAccountsOverview.WorstArrears_Mtg = AccountStatusDictionary.GetDetailedAccountStatusString(worstStatus[1]);
			model.ConsumerAccountsOverview.WorstArrears_PL = AccountStatusDictionary.GetDetailedAccountStatusString(worstStatus[2]);
			model.ConsumerAccountsOverview.WorstArrears_Other = AccountStatusDictionary.GetDetailedAccountStatusString(worstStatus[3]);
			model.ConsumerAccountsOverview.WorstArrears_Total = AccountStatusDictionary.GetDetailedAccountStatusString(GetWorstStatus(worstStatus));

			model.ConsumerAccountsOverview.TotalCurLimits_CC = limits[0];
			model.ConsumerAccountsOverview.TotalCurLimits_Mtg = limits[1];
			model.ConsumerAccountsOverview.TotalCurLimits_PL = limits[2];
			model.ConsumerAccountsOverview.TotalCurLimits_Other = limits[3];
			model.ConsumerAccountsOverview.TotalCurLimits_Total = limits.Sum();

			model.ConsumerAccountsOverview.Balance_CC = balances[0];
			model.ConsumerAccountsOverview.Balance_Mtg = balances[1];
			model.ConsumerAccountsOverview.Balance_PL = balances[2];
			model.ConsumerAccountsOverview.Balance_Other = balances[3];
			model.ConsumerAccountsOverview.Balance_Total = balances.Sum();
			model.NOCs = eInfo.Data.Nocs.Select(nocDetails => new NOCInfo { NOCReference = nocDetails.Reference, NOCLines = nocDetails.TextLine }).ToArray();

			Log.DebugFormat("Error List: {0}", PrintErrorList(model.ErrorList));
		}

		private static string PrintErrorList(List<string> errorList)
		{
			var sb = new StringBuilder();
			if (errorList != null)
			{
				for (int i = 0; i < errorList.Count; i++)
				{
					if (i != 0)
					{
						sb.Append(" ");
					}
					sb.Append(i + 1).Append(") ");
					sb.Append(errorList[i]);
				}
			}

			return sb.ToString();
		}

		protected void AppendAmlInfo(AMLInfo data, Customer customer, CustomerAddress customerAddress)
		{
			try
			{
				var srv = new IdHubService();
				var result = srv.Authenticate(customer.PersonalInfo.FirstName, string.Empty, customer.PersonalInfo.Surname,
											  customer.PersonalInfo.Gender.ToString(),
											  customer.PersonalInfo.DateOfBirth.HasValue
												  ? customer.PersonalInfo.DateOfBirth.Value
												  : DateTime.Now,
											  customerAddress.Line1, customerAddress.Line2, customerAddress.Line3,
											  customerAddress.Town, customerAddress.County, customerAddress.Postcode,
											  customer.Id, true);
				if (null == result)
					return;

				Log.DebugFormat("AML data for building model: {0} - {1}", result.AuthenticationIndex, result.AuthIndexText);
				data.HasAML = true;
				data.AuthIndexText = customer.AmlDescription;
				data.AuthenticationIndex = customer.AmlScore;
				data.NumPrimDataItems = result.NumPrimDataItems;
				data.NumPrimDataSources = result.NumPrimDataSources;
				data.NumSecDataItems = result.NumSecDataItems;
				data.ReturnedHRPCount = result.ReturnedHRPCount;
				data.StartDateOldestPrim = result.StartDateOldestPrim;
				data.StartDateOldestSec = result.StartDateOldestSec;
			}
			catch (Exception ex)
			{
				Log.Warn("AppendAmlInfo failed", ex);
				Errors.Add("Failed to retrieve AML info");
			}
		}

		protected void AppendBavInfo(BankAccountVerificationInfo data, Customer customer, CustomerAddress customerAddress)
		{
			try
			{
				var srv = new IdHubService();

				var bankAccount = customer.BankAccount;
				var result = srv.AccountVerification(customer.PersonalInfo.FirstName, string.Empty,
													 customer.PersonalInfo.Surname,
													 customer.PersonalInfo.Gender.ToString(),
													 customer.PersonalInfo.DateOfBirth.HasValue
														 ? customer.PersonalInfo.DateOfBirth.Value
														 : DateTime.Now,
													 customerAddress.Line1, customerAddress.Line2, customerAddress.Line3,
													 customerAddress.Town, customerAddress.County, customerAddress.Postcode,
													 bankAccount != null ? bankAccount.SortCode : "",
													 bankAccount != null ? bankAccount.AccountNumber : "",
													 customer.Id, true);
				if (null == result)
					return;
				data.HasBWA = true;
				data.AddressScore = result.AddressScore;
				data.NameScore = result.NameScore;
				data.AuthenticationText = result.AuthenticationText;
				data.AccountStatus = result.AccountStatus;
			}
			catch (Exception ex)
			{
				Log.Warn("AppendBavInfo failed", ex);
				Errors.Add("Failed to retrieve BWA info");
			}
		}


		private void BuildEBusinessModel(Customer customer, CreditBureauModel model,
										 bool getFromLog = false, long? logId = null)
		{
			var company = customer.Company;
			if (company == null)
				return;

			int updateCompanyDataPeriodDays;

			switch (company.TypeOfBusiness.Reduce()) {
				case TypeOfBusinessReduced.Limited:
					var srv = new EBusinessService(m_oDB);
					var limitedBusinessData = srv.GetLimitedBusinessData(company.ExperianRefNum, customer.Id, true, false);
					updateCompanyDataPeriodDays = CurrentValues.Instance.UpdateCompanyDataPeriodDays;

					if (limitedBusinessData != null && limitedBusinessData.LastCheckDate.HasValue &&
						(DateTime.UtcNow - limitedBusinessData.LastCheckDate.Value).TotalDays >= updateCompanyDataPeriodDays)
					{
						limitedBusinessData.IsDataExpired = true;
					}
					AppendLimitedInfo(model, limitedBusinessData);
					model.BorrowerType = company.TypeOfBusiness.ToString();
					model.CompanyName = company.CompanyName;
					model.directorsModels = GenerateDirectorsModels(customer, company.Directors, getFromLog, logId);
					break;

				case TypeOfBusinessReduced.NonLimited:
					CompanyDataForCreditBureauActionResult notLimitedBusinessData = serviceClient.Instance.GetCompanyDataForCreditBureau(context.UserId, customer.Id, company.ExperianRefNum);

					bool isDataExpired = false;
					updateCompanyDataPeriodDays = CurrentValues.Instance.UpdateCompanyDataPeriodDays;
					if (notLimitedBusinessData != null && notLimitedBusinessData.LastUpdate.HasValue &&
						(DateTime.UtcNow - notLimitedBusinessData.LastUpdate.Value).TotalDays >= updateCompanyDataPeriodDays)
					{
						isDataExpired = true;
					}
					if (notLimitedBusinessData != null)
					{
						AppendNonLimitedInfo(model, notLimitedBusinessData.LastUpdate, notLimitedBusinessData.Score, notLimitedBusinessData.Errors, isDataExpired);
					}
					model.BorrowerType = company.TypeOfBusiness.ToString();
					model.CompanyName = company.CompanyName;
					model.directorsModels = GenerateDirectorsModels(customer, company.Directors, getFromLog, logId);
					break;
			}
		}

		protected void AppendLimitedInfo(CreditBureauModel model, LimitedResults eInfo)
		{
			if (eInfo == null)
				return;
			var spc = GetScorePositionAndColor((double)eInfo.BureauScore, CompanyScoreMax, CompanyScoreMin);
			model.ModelType = "Limited";
			model.LimitedInfo = new ExperianLimitedInfo
			{
				BureauScore = eInfo.BureauScore,
				ScoreColor = spc.Color,
				RiskLevel = (eInfo.BureauScore > 90) ? "Low Risk" : ((eInfo.BureauScore < 40) ? "High Risk" : "Medium Risk"),
				ExistingBusinessLoans = eInfo.ExistingBusinessLoans,
				Error = eInfo.Error,
				IsDataExpired = eInfo.IsDataExpired,
				IsError = eInfo.IsError,
				LastCheckDate = eInfo.LastCheckDate
			};

			if (!string.IsNullOrEmpty(eInfo.Error))
			{
				model.ErrorList.AddRange(eInfo.Error.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
			}
		}

		protected void AppendNonLimitedInfo(CreditBureauModel model, DateTime? lastUpdate, int score, string errors, bool isDataExpired)
		{
			if (!lastUpdate.HasValue)
				return;
			model.ModelType = "NonLimited";
			var spc = GetScorePositionAndColor(score, CompanyScoreMax, CompanyScoreMin);
			model.NonLimitedInfo = new ExperianNonLimitedInfo
			{
				BureauScore = score,
				ScoreColor = spc.Color,
				CompanyNotFoundOnBureau = !string.IsNullOrEmpty(errors),
				Error = errors,
				IsDataExpired = isDataExpired,
				IsError = !string.IsNullOrEmpty(errors),
				LastCheckDate = lastUpdate
			};
			if (!string.IsNullOrEmpty(errors))
			{
				model.ErrorList.AddRange(errors.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
			}
		}

		private static void BuildSummaryModel(CreditBureauModel model)
		{
			model.Summary = new Summary
			{
				Score = model.Score,
				ConsumerIndebtednessIndex = model.CII,
				CheckDate = model.CheckDate,
				Validtill = model.CheckValidity,
				WorstCurrentstatus = model.ConsumerSummaryCharacteristics.WorstCurrentStatus,
				WorstHistoricalstatus = model.ConsumerSummaryCharacteristics.WorstCurrentStatus3M,
				Numberofdefaults = model.ConsumerSummaryCharacteristics.NumberOfDefaults,
				Accounts = model.ConsumerSummaryCharacteristics.NumberOfAccounts,
				CCJs = model.ConsumerSummaryCharacteristics.NumberOfCCJs,
				MostrecentCCJ = model.ConsumerSummaryCharacteristics.AgeOfMostRecentCCJ,
				DSRandownershiptype = model.ConsumerSummaryCharacteristics.DSRandOwnershipType,
				Creditcardutilization = model.ConsumerSummaryCharacteristics.CreditCardUtilization,
				Enquiriesinlast6months = model.ConsumerSummaryCharacteristics.EnquiriesLast6M,
				Enquiriesinlast3months = model.ConsumerSummaryCharacteristics.EnquiriesLast3M,
				Totalbalance = model.ConsumerAccountsOverview.Balance_Total,
				AML = model.AmlInfo.AMLResult,
				AMLnum = model.AmlInfo.AuthenticationIndex.ToString(CultureInfo.InvariantCulture),
				BWA = model.BavInfo.BankAccountVerificationResult,
				BWAnum = GetBwaScoreInfo(model.BavInfo),
				Businesstype = model.BorrowerType,
				BusinessScore = GetBusinessScore(model),
				RiskLevel = model.LimitedInfo != null ? model.LimitedInfo.RiskLevel : "-",
				Existingbusinessloans = GetExistingBusinessLoans(model),
				ThinFile = model.AccountsInformation == null || model.AccountsInformation.Length == 0,
				ConsumerAccountsOverview = model.ConsumerAccountsOverview
			};
		}

		private static string GetBwaScoreInfo(BankAccountVerificationInfo info)
		{
			return
				string.Format("{0}, {1}, {2}",
				info.NameScore,
				info.AddressScore,
				string.IsNullOrEmpty(info.AccountStatus) ? "-" : info.AccountStatus);
		}

		private static string GetBusinessScore(CreditBureauModel creditBureauModel)
		{
			if (creditBureauModel.LimitedInfo != null)
			{
				return creditBureauModel.LimitedInfo.BureauScore.ToString(CultureInfo.InvariantCulture);
			}
			if (creditBureauModel.NonLimitedInfo != null)
			{
				return creditBureauModel.NonLimitedInfo.BureauScore.ToString(CultureInfo.InvariantCulture);
			}
			return "-";
		}

		private static string GetExistingBusinessLoans(CreditBureauModel model)
		{
			if (model.LimitedInfo != null)
			{
				return model.LimitedInfo.ExistingBusinessLoans.ToString(CultureInfo.InvariantCulture);
			}
			if (model.NonLimitedInfo != null)
			{
				return model.NonLimitedInfo.CompanyNotFoundOnBureau.ToString(CultureInfo.InvariantCulture);
			}
			return "-";
		}

		public CreditBureauModel[] GenerateDirectorsModels(Customer customer, IEnumerable<Director> directors, bool getFromLog = false, long? logId = null)
		{
			var consumerSrv = new ConsumerService();
			var dirModelList = new List<CreditBureauModel>();
			foreach (var director in directors)
			{
				ConsumerServiceResult result = null;
				if (getFromLog && logId.HasValue)
				{
					var directorCopy = director;
					var logs =
						_session.Query<MP_ServiceLog>()
							.Where(x => x.Director.Id == directorCopy.Id && x.ServiceType == ExperianServiceType.Consumer.DescriptionAttr())
							.ToList();
					var date = _session.Query<MP_ServiceLog>().First(x => x.Id == logId).InsertDate.ToUniversalTime().Date;
					var response = logs.FirstOrDefault(x => x.InsertDate >= date && x.InsertDate < date.AddDays(1));

					var serializer = new XmlSerializer(typeof(OutputRoot));

					if (response != null)
					{
						Log.DebugFormat("No directors consumer requests where found in DB for director {1} {2} for date {0}", date, director.Name, director.Surname);

						using (TextReader sr = new StringReader(response.ResponseData))
						{
							var output = (OutputRoot)serializer.Deserialize(sr);
							result = new ConsumerServiceResult(output);
						}
					}
				}
				else
				{
					var directorAddresses = director.DirectorAddressInfo != null
											? director.DirectorAddressInfo.AllAddresses
											: null;
					var directorMainAddress = directorAddresses != null && directorAddresses.Any()
												  ? directorAddresses.First()
												  : null;
					var dirLoc = new InputLocationDetailsMultiLineLocation();
					if (directorMainAddress != null)
					{
						dirLoc.LocationLine1 = directorMainAddress.Line1;
						dirLoc.LocationLine2 = directorMainAddress.Line2;
						dirLoc.LocationLine3 = directorMainAddress.Line3;
						dirLoc.LocationLine4 = directorMainAddress.Town;
						dirLoc.LocationLine5 = directorMainAddress.County;
						dirLoc.LocationLine6 = directorMainAddress.Postcode;
					}
					result = consumerSrv.GetConsumerInfo(director.Name, director.Surname,
												director.Gender.ToString(),
												director.DateOfBirth, null, dirLoc, "PL", customer.Id, director.Id, true, true, false);
				}

				var dirModel = new CreditBureauModel();
				GenerateConsumerModel(dirModel, -1, result);
				dirModel.Name = director.Name;
				dirModel.MiddleName = director.Middle;
				dirModel.Surname = director.Surname;
				dirModel.FullName = string.Format("{0} {1} {2}", director.Name, director.Middle, director.Surname);
				dirModel.Id = director.Id;
				dirModelList.Add(dirModel);
			}
			return dirModelList.ToArray();
		}

		private static InputLocationDetailsMultiLineLocation MultiLineLocationFromCustomerAddress(
			CustomerAddress customerMainAddress)
		{
			var loc = new InputLocationDetailsMultiLineLocation();
			if (customerMainAddress != null)
			{
				loc.LocationLine1 = customerMainAddress.Line1;
				loc.LocationLine2 = customerMainAddress.Line2;
				loc.LocationLine3 = customerMainAddress.Line3;
				loc.LocationLine4 = customerMainAddress.Town;
				loc.LocationLine5 = customerMainAddress.County;
				loc.LocationLine6 = customerMainAddress.Postcode;
			}
			return loc;
		}

		private int GetNonLimitedScore(int customerId, string experianRefNumber)
		{
			try
			{
				CompanyDataForCreditBureauActionResult notLimitedBusinessData = serviceClient.Instance.GetCompanyDataForCreditBureau(context.UserId, customerId, experianRefNumber);
				return notLimitedBusinessData.Score;
			}
			catch (Exception ex)
			{
				Log.WarnFormat("Failed to retrieve nonlimited score from db {0}", ex);
				return 0;
			}
		}

		public static DelphiModel GetScorePositionAndColor(double score, int scoreMax, int scoreMin)
		{
			const int w = 640;

			const int barWidth = 8;
			const int spaceWidth = 1;
			const int barCount = w / barWidth;
			const int redWidth = 80;
			const int yellowX = 240;
			const int greenWidth = 240;


			var s = (int)((score - scoreMin) / (scoreMax - scoreMin) * barCount);
			s = (s < 0) ? 0 : s;
			s = (s >= barCount) ? barCount - 1 : s;

			var virtualX = (s * barWidth + barWidth / 2);
			var c = Color.White;
			if (virtualX < redWidth)
				c = Color.FromArgb(255, 0, 0);
			else if (virtualX < yellowX)
			{
				var t = Cup(1.0 - 1.0 * (virtualX - redWidth) / (yellowX - redWidth));
				c = Color.FromArgb(255, (int)(255 * t), 0);
			}
			else if (virtualX < w - greenWidth)
			{
				var t = Cup(1.0 - 1.0 * (w - greenWidth - virtualX) / (w - greenWidth - yellowX));
				c = Color.FromArgb((int)(255 * t), 255, 0);
			}
			else if (virtualX >= w - greenWidth)
				c = Color.FromArgb(0, 255, 0);

			int barPos = s * (barWidth + spaceWidth);
			int scorePos = (s < barCount / 2) ? barPos : barPos - 120;
			int valPos = (s < barCount / 2) ? barPos : barPos - 120;

			return new DelphiModel
				{
					Align = (s < barCount / 2) ? "none" : "left",
					Position = string.Format("{0}px;", scorePos),
					ValPosition = string.Format("{0}px;", valPos),
					Color = string.Format("#{0:X2}{1:X2}{2:X2};", c.R, c.G, c.B),
				};
		}

		private static double Cup(double x)
		{
			return ((x <= -1) || (x >= 1)) ? 0.0 : Math.Exp(1.0 / (x * x - 1)) * Math.E;
		}

		protected CreditBureauModel GenerateRandomModel(int id)
		{
			var r = new Random();
			var score = r.Next(-200, 1600);
			var spc = GetScorePositionAndColor(score, ConsumerScoreMax, ConsumerScoreMin);

			return new CreditBureauModel
			{
				Id = id,
				Score = score,
				ScorePosition = spc.Position,
				ScoreAlign = spc.Align,
				ScoreValuePosition = spc.ValPosition,
				ScoreColor = spc.Color,
				CheckStatus = "Passed",
				CheckIcon = "icon-white icon-ok",
				ButtonStyle = "btn-success"
			};
		}

		protected CreditBureauModel GenerateNotQualifiedModel(int id)
		{
			return new CreditBureauModel
			{
				Id = id,
				Score = 0,
				CheckStatus = "Not Qualified",
				CheckIcon = "icon-white icon-remove-sign",
				ButtonStyle = "btn-danger",
				CheckDate = string.Empty,
				CheckValidity = string.Empty
			};
		}

		protected static List<string> StatusScale = new List<string> { "D", "U", "S", "?", "0", "1", "2", "3", "4", "5", "6", "8", "9" };
		protected List<string> Errors { get; set; }
		public static string GetWorstStatus(params string[] s)
		{
			var scales = s.Select(str => new { str, status = StatusScale.IndexOf(str) });
			return scales.MaxBy(x => x.status).str;
		}

		protected const int StatusHistoryMonths = 24;

		public static string GetAccountStatusString(string status, out string dateType)
		{
			switch (status)
			{
				case DelinquentCaisStatusName:
					dateType = DelinquentDateType;
					return DelinquentStatusName;
				case ActiveCaisStatusName:
					dateType = ActiveDateType;
					return ActiveStatusName;
				case DefaultCaisStatusName:
					dateType = DefaultDateType;
					return DefaultStatusName;
				case SettledCaisStatusName:
					dateType = SettledDateType;
					return SettledStatusName;
				default:
					dateType = UnknownDateType;
					return status;
			}
		}

		private const string DefaultCaisStatusName = "F";
		private const string DefaultStatusName = "Default";
		private const string DefaultDateType = "Default Date";

		private const string DelinquentCaisStatusName = "D";
		private const string DelinquentStatusName = "Delinquent";
		private const string DelinquentDateType = "Delinquent Date";

		private const string ActiveCaisStatusName = "A";
		private const string ActiveStatusName = "Active";
		private const string ActiveDateType = "Last Update Date";

		private const string SettledCaisStatusName = "S";
		private const string SettledStatusName = "Settled";
		private const string SettledDateType = "Settlement Date";

		private const string UnknownDateType = "Unknown Date Type";

		protected int GetAccountType(string accType)
		{
			if (CardsAccounts.IndexOf(accType, StringComparison.Ordinal) >= 0)
				return 0;
			if (MortgageAccounts.IndexOf(accType, StringComparison.Ordinal) >= 0)
				return 1;
			if (LoanAccounts.IndexOf(accType, StringComparison.Ordinal) >= 0)
				return 2;
			if (OtherAccounts.IndexOf(accType, StringComparison.Ordinal) >= 0)
				return 3;
			return -1;
		}

		// 0 - CC, 1 - Mortgage, 2 - PL, 3 - other
		protected static string CardsAccounts = "04,05,06,37,38";
		protected static string MortgageAccounts = "03,16,25,30,31,32,33,34,35,69";
		protected static string LoanAccounts = "00,01,02,17,19,22,26,27,28,29";
		protected static string OtherAccounts = "07,08,12,13,14,15,18,20,21,23,24,36,39,40,41,42,43,44,45,46,47,48,49,50,51,53,54,55,56,57,58,59,60,61,62,63,64,70";

		protected string GetRepaymentPeriodString(int months)
		{
			if (months == 999)
			{
				return "-";
			}
			int repaymentYears = months / 12;
			int repaymentMonths = months % 12;
			if (repaymentYears > 0)
			{
				if (repaymentMonths > 0)
					return string.Format("{0} year(s) {1} month(s)", repaymentYears, repaymentMonths);
				return string.Format("{0} year(s)", repaymentYears);
			}
			return string.Format("{0} months", repaymentMonths);
		}

		public class AccountInfoComparer : IComparer<AccountInfo>
		{
			private static readonly Dictionary<string, int> SortedValues = new Dictionary<string, int> { { DefaultStatusName, 1 }, { DelinquentStatusName, 2 }, { ActiveStatusName, 3 }, { SettledStatusName, 4 } };

			public int Compare(AccountInfo x, AccountInfo y)
			{
				if (SortedValues.ContainsKey(x.AccountStatus) && SortedValues.ContainsKey(y.AccountStatus))
				{
					return SortedValues[x.AccountStatus].CompareTo(SortedValues[y.AccountStatus]);
				}

				if (SortedValues.ContainsKey(x.AccountStatus))
				{
					return 1;
				}

				if (SortedValues.ContainsKey(y.AccountStatus))
				{
					return -1;
				}

				return 0;
			}
		}
		
		private static readonly Dictionary<int, Variables> map = new Dictionary<int, Variables>
			{
				{1,Variables.FinancialAccounts_MainApplicant},
				{2,Variables.FinancialAccounts_AliasOfMainApplicant},
				{3,Variables.FinancialAccounts_AssociationOfMainApplicant},
				{5,Variables.FinancialAccounts_JointApplicant},
				{6, Variables.FinancialAccounts_AliasOfJointApplicant},
				{7,Variables.FinancialAccounts_AssociationOfJointApplicant},
				{9,Variables.FinancialAccounts_No_Match},
				{4,Variables.FinancialAccounts_Spare},//Spare
				{8,Variables.FinancialAccounts_Spare},//Spare
			};
	}


}