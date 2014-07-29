namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using ExperianLib.Dictionaries;
	using ExperianLib.IdIdentityHub;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Utils.Extensions;
	using MoreLinq;
	using NHibernate;
	using NHibernate.Linq;
	using System.Text;
	using ServiceClientProxy;
	using Web.Models;
	using log4net;
	using EZBob.DatabaseLib.Model.Experian;
	using Ezbob.Backend.ModelsWithDB.Experian;

	public class CreditBureauModelBuilder
	{
		private readonly ISession _session;
		private static readonly ILog Log = LogManager.GetLogger(typeof(CreditBureauModelBuilder));
		private readonly IExperianHistoryRepository _experianHistoryRepository;
		private const int ConsumerScoreMax = 1400;
		private const int ConsumerScoreMin = 120;

		private readonly ServiceClient _serviceClient;

		public CreditBureauModelBuilder(ISession session,
			IExperianHistoryRepository experianHistoryRepository)
		{
			_session = session;
			_experianHistoryRepository = experianHistoryRepository;
			Errors = new List<string>();
			_serviceClient = new ServiceClient();
		}

		public CreditBureauModel Create(Customer customer, bool getFromLog = false, long? logId = null)
		{
			Log.DebugFormat("CreditBureauModel Create customerid: {0} hist: {1} histId: {2}", customer.Id, getFromLog, logId);
			var model = new CreditBureauModel { Id = customer.Id };
			try
			{
				model.Consumer = GetConsumerInfo(customer.Id, null, logId, customer.PersonalInfo != null ? customer.PersonalInfo.Fullname : "");
				if (customer.Company != null && customer.Company.Directors.Any())
				{
					model.Directors = new List<ExperianConsumerModel>();
					foreach (var director in customer.Company.Directors)
					{
						model.Directors.Add(GetConsumerInfo(customer.Id, director.Id, null, 
							string.Format("{0} {1} {2}", director.Name, director.Middle, director.Surname)));
					}
				}

				model.AmlInfo = GetAmlInfo(customer);
				model.BavInfo = GetBavInfo(customer);
				model.Summary = GetSummaryModel(model);

				//todo move to CompanyScore
				model.CompanyHistory = GetCompanyHistoryModel(customer);
			}
			catch (Exception e)
			{
				Log.DebugFormat("CreditBureauModel Create Exception {0} ", e);
				model.Consumer.ErrorList.Add(e.Message);
			}

			model.Consumer.ErrorList.AddRange(Errors);
			return model;
		}

		public ExperianConsumerModel GetConsumerInfo(int customerId, int? directorId, long? logId, string name)
		{
			var data = _serviceClient.Instance.LoadExperianConsumer(customerId, directorId, logId);
			return GenerateConsumerModel(data.Value, customerId, directorId, logId, name);
		}

		private IOrderedEnumerable<CheckHistoryModel> GetConsumerHistoryModel(int customerId, int? directorId)
		{
			List<MP_ExperianHistory> consumerHistory = directorId.HasValue ? 
				_experianHistoryRepository.GetDirectorConsumerHistory(directorId.Value).ToList() : 
				_experianHistoryRepository.GetCustomerConsumerHistory(customerId).ToList();

			if (consumerHistory.Any())
			{
				return consumerHistory.Select(x => new CheckHistoryModel
					{
						Date = x.Date.ToUniversalTime(),
						Id = x.Id,
						Score = x.Score,
						CII = x.CII.HasValue ? x.CII.Value : -1,
						Balance = x.CaisBalance.HasValue ? x.CaisBalance.Value : -1,
						ServiceLogId = x.ServiceLogId
					}).OrderByDescending(h => h.Date);
			}

			return null;
		}

		//todo remove
		private IOrderedEnumerable<CheckHistoryModel> GetCompanyHistoryModel(Customer customer)
		{
			var isLimited = customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited;
			Log.DebugFormat("BuildHistoryModel company type: {0}", isLimited ? "Limited" : "NonLimited");
			var companyHistory = _experianHistoryRepository.GetCompanyHistory(customer.Company.ExperianRefNum, isLimited).ToList();
			if (companyHistory.Any())
			{
				Log.Debug("BuildHistoryModel company from history table");
				return companyHistory.Select(x => new CheckHistoryModel
				{
					Date = x.Date.ToUniversalTime(),
					Id = x.Id,
					Score = x.Score,
					Balance = x.CaisBalance
				}).OrderByDescending(h => h.Date);
			}
			
			//Not in cache
			Log.Debug("BuildHistoryModel company from mp_servicelog table");
			var type = (isLimited ? ExperianServiceType.LimitedData.DescriptionAttr() : ExperianServiceType.NonLimitedData.DescriptionAttr());

			if (isLimited)
			{
				List<CheckHistoryModel> checkCompanyHistoryModels = (from s in _session.Query<MP_ServiceLog>()
																	 where s.Director == null
																	 where s.Customer.Id == customer.Id
																	 where s.ServiceType == type
																	 select GetLimitedHistory(s.InsertDate, s.Id)
																	).ToList();

				var history =  checkCompanyHistoryModels.Where(h => h != null).OrderByDescending(h => h.Date);
				foreach (var cModel in checkCompanyHistoryModels)
				{
					if (cModel != null)
					{
						_experianHistoryRepository.Save(new MP_ExperianHistory
						{
							CustomerId = customer.Id,
							ServiceLogId = cModel.Id,
							CompanyRefNum = customer.Company.ExperianRefNum,
							Type = type,
							Date = cModel.Date,
							Score = cModel.Score,
						});
					}
				}
				return history;
			}
			return null;
		}
		//todo remove
		private CheckHistoryModel GetLimitedHistory(DateTime date, long id)
		{
			ComapanyDashboardModel m = new CompanyScoreModelBuilder().BuildLimitedDashboardModel(id);

			return new CheckHistoryModel
			{
				Date = date,
				Id = id,
				Balance = m.CaisBalance,
				Score = m.Score,
				ServiceLogId = id
			};
		} // GetLimitedHistory

		public ExperianConsumerModel GenerateConsumerModel(ExperianConsumerData eInfo, int customerId, int? directorId, long? logId, string name)
		{
			var model = new ExperianConsumerModel { ErrorList = new List<string>(),
				Id = directorId.HasValue? directorId.Value : customerId };
			if (eInfo == null || eInfo.ServiceLogId == null)
			{
				model.HasExperianError = true;
				model.ErrorList.Add("No data");
				model.ApplicantFullNameAge = name;
				return model;
			}

			var scorePosColor = GetScorePositionAndColor(eInfo.BureauScore.HasValue ? eInfo.BureauScore.Value : 0, ConsumerScoreMax, ConsumerScoreMin);

			var checkDate = eInfo.InsertDate;
			var checkValidity = checkDate.AddMonths(3);
			
			model.ServiceLogId = eInfo.ServiceLogId;
			model.HasExperianError = eInfo.HasExperianError;
			model.ModelType = "Consumer";
			model.CheckDate = checkDate.ToShortDateString();
			model.CheckValidity = checkValidity.ToShortDateString();
			model.BorrowerType = "Consumer";
			model.Score = eInfo.BureauScore;
			model.Odds = Math.Pow(2, (((double)(eInfo.BureauScore ?? 0)) - 600) / 80);
			model.ScorePosition = scorePosColor.Position;
			model.ScoreAlign = scorePosColor.Align;
			model.ScoreValuePosition = scorePosColor.ValPosition;
			model.ScoreColor = scorePosColor.Color;
			model.Applicant = eInfo.Applicants.FirstOrDefault();
			if (model.Applicant != null)
			{
				var days = model.Applicant.DateOfBirth.HasValue ? (DateTime.Now - model.Applicant.DateOfBirth.Value).TotalDays : 0;
				var age = (int)Math.Round(days/365);
				model.ApplicantFullNameAge = string.Format("{0} {1} {2} {3} {4} {5}",
				                                           model.Applicant.Title,
				                                           model.Applicant.Forename,
				                                           model.Applicant.MiddleName,
				                                           model.Applicant.Surname,
				                                           model.Applicant.Suffix,
				                                           age);
			}
			if (eInfo.Applicants.Count > 1)
			{
				Errors.Add("More than one applicant specified");
			}


			model.CII = eInfo.CII;
			if (!string.IsNullOrEmpty(eInfo.Error))
			{
				model.ErrorList.AddRange(eInfo.Error.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
			}

			model.NumberOfAccounts = 0;
			model.NumberOfAccounts3M = 0;
			model.WorstCurrentStatus = AccountStatusDictionary.GetDetailedAccountStatusString(eInfo.WorstCurrentStatus);
			model.WorstCurrentStatus3M = AccountStatusDictionary.GetDetailedAccountStatusString(eInfo.WorstHistoricalStatus);
			model.EnquiriesLast3M = eInfo.EnquiriesLast3Months;
			model.EnquiriesLast6M = eInfo.EnquiriesLast6Months;
			model.NumberOfDefaults = 0;
			model.NumberOfCCOverLimit = eInfo.CreditCardOverLimit;
			model.CreditCardUtilization = eInfo.CreditLimitUtilisation;
			model.NOCsOnCCJ = eInfo.NOCsOnCCJ;
			model.NOCsOnCAIS = eInfo.NOCsOnCAIS;
			model.NumberOfCCJs = eInfo.NumCCJs;
			model.SatisfiedJudgements = eInfo.SatisfiedJudgement;
			model.AgeOfMostRecentCCJ = eInfo.CCJLast2Years;
			model.CAISSpecialInstructionFlag = eInfo.CAISSpecialInstructionFlag;


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

			foreach (var caisDetails in eInfo.Cais)
			{
				var accountInfo = new AccountInfo();
				//check which acccount type show
				Variables? var = null;
				if (caisDetails.MatchTo.HasValue)
				{
					var = map[caisDetails.MatchTo.Value];

					var isShowThisFinancinalAccount = CurrentValues.Instance[var.Value];
					if (isShowThisFinancinalAccount == null || !isShowThisFinancinalAccount)
					{
						continue;
					}

					accountInfo.MatchTo = var.DescriptionAttr();
				}
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

			model.NumberOfAccounts = numberOfAccounts;
			model.NumberOfAccounts3M = numberOfAcc3M;
			model.NumberOfDefaults = numberOfDefaults;
			model.NumberOfLates = numberOfLates;
			model.LateStatus = AccountStatusDictionary.GetDetailedAccountStatusString(lateStatus);
			model.DefaultAmount = defaultAmount;

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
			model.NOCs = eInfo.Nocs.Select(nocDetails => new NOCInfo { NOCReference = nocDetails.Reference, NOCLines = nocDetails.TextLine }).ToArray();

			Log.DebugFormat("Error List: {0}", PrintErrorList(model.ErrorList));
			
			model.ConsumerHistory = GetConsumerHistoryModel(customerId, directorId);
			return model;
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

		protected AMLInfo GetAmlInfo(Customer customer)
		{
			var data = new AMLInfo
			{
				AMLResult = string.IsNullOrEmpty(customer.AMLResult)
					? "Verification was not performed"
					: customer.AMLResult
			};

			try
			{
				var customerAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault();
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
					return data;

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
			return data;
		}

		protected BankAccountVerificationInfo GetBavInfo(Customer customer)
		{

			var data = new BankAccountVerificationInfo
			{
				BankAccountVerificationResult = string.IsNullOrEmpty(customer.BWAResult)
					? "Verification was not performed"
					: customer.BWAResult
			};

			try
			{
				var customerAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault();
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
					return data;
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

			return data;
		}

		private static Summary GetSummaryModel(CreditBureauModel model)
		{
			return new Summary
			{
				Score = model.Consumer.Score,
				ConsumerIndebtednessIndex = model.Consumer.CII,
				CheckDate = model.Consumer.CheckDate,
				Validtill = model.Consumer.CheckValidity,
				WorstCurrentstatus = model.Consumer.WorstCurrentStatus,
				WorstHistoricalstatus = model.Consumer.WorstCurrentStatus3M,
				Numberofdefaults = model.Consumer.NumberOfDefaults,
				Accounts = model.Consumer.NumberOfAccounts,
				CCJs = model.Consumer.NumberOfCCJs,
				MostrecentCCJ = model.Consumer.AgeOfMostRecentCCJ,
				Creditcardutilization = model.Consumer.CreditCardUtilization,
				Enquiriesinlast6months = model.Consumer.EnquiriesLast6M,
				Enquiriesinlast3months = model.Consumer.EnquiriesLast3M,
				Totalbalance = model.Consumer.ConsumerAccountsOverview.Balance_Total,
				AML = model.AmlInfo.AMLResult,
				AMLnum = model.AmlInfo.AuthenticationIndex.ToString(CultureInfo.InvariantCulture),
				BWA = model.BavInfo.BankAccountVerificationResult,
				BWAnum = GetBwaScoreInfo(model.BavInfo),
				ThinFile = model.Consumer.AccountsInformation == null || model.Consumer.AccountsInformation.Length == 0,
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

		protected string GetRepaymentPeriodString(int? months)
		{
			if (months == 999 || !months.HasValue)
			{
				return "-";
			}
			int repaymentYears = months.Value / 12;
			int repaymentMonths = months.Value % 12;
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
				{1, Variables.FinancialAccounts_MainApplicant},
				{2, Variables.FinancialAccounts_AliasOfMainApplicant},
				{3, Variables.FinancialAccounts_AssociationOfMainApplicant},
				{5, Variables.FinancialAccounts_JointApplicant},
				{6, Variables.FinancialAccounts_AliasOfJointApplicant},
				{7, Variables.FinancialAccounts_AssociationOfJointApplicant},
				{9, Variables.FinancialAccounts_No_Match},
				{4, Variables.FinancialAccounts_Spare},//Spare
				{8, Variables.FinancialAccounts_Spare},//Spare
			};
	}
}