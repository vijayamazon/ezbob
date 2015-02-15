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
	using System.Text;
	using ServiceClientProxy;
	using log4net;
	using EZBob.DatabaseLib.Model.Experian;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using EzBob.Web.Infrastructure;

	public class CreditBureauModelBuilder
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(CreditBureauModelBuilder));
		private readonly IExperianHistoryRepository _experianHistoryRepository;
		private const int ConsumerScoreMax = 1400;
		private const int ConsumerScoreMin = 120;

		private readonly ServiceClient _serviceClient;
		private readonly IEzbobWorkplaceContext _context;

		public CreditBureauModelBuilder(IExperianHistoryRepository experianHistoryRepository, IEzbobWorkplaceContext context)
		{
			_experianHistoryRepository = experianHistoryRepository;
			_context = context;
			Errors = new List<string>();
			_serviceClient = new ServiceClient();
		}

		public CreditBureauModel Create(Customer customer, bool getFromLog = false, long? logId = null)
		{
			Log.DebugFormat("CreditBureauModel Create customerid: {0} hist: {1} histId: {2}", customer.Id, getFromLog, logId);
			var model = new CreditBureauModel { Id = customer.Id, Directors = new List<ExperianConsumerModel>()};
			try
			{
				model.Consumer = GetConsumerInfo(customer, null, logId, customer.PersonalInfo != null ? customer.PersonalInfo.Fullname : "");
				if (customer.Company != null && customer.Company.Directors.Any())
				{
					foreach (var director in customer.Company.Directors)
					{
						model.Directors.Add(GetConsumerInfo(customer, director, null, 
							string.Format("{0} {1} {2}", director.Name, director.Middle, director.Surname)));
					}
				}

				model.AmlInfo = GetAmlInfo(customer);
				model.BavInfo = GetBavInfo(customer);
				model.Summary = GetSummaryModel(model);
			}
			catch (Exception e)
			{
				Log.DebugFormat("CreditBureauModel Create Exception {0} ", e);
				if (model.Consumer != null)
					model.Consumer.ErrorList.Add(e.Message);
			}

			if (model.Consumer != null)
				model.Consumer.ErrorList.AddRange(Errors);

			return model;
		}

		public ExperianConsumerModel GetConsumerInfo(Customer customer, Director director, long? logId, string name)
		{
			var data = _serviceClient.Instance.LoadExperianConsumer(_context.UserId, customer.Id, director != null ? director.Id : (int?)null, logId);

			if (director != null && director.ExperianConsumerScore == null) {
				director.ExperianConsumerScore = data.Value.BureauScore;
			}

			if (director == null && customer.ExperianConsumerScore == null) {
				customer.ExperianConsumerScore = data.Value.BureauScore;
			}

			return GenerateConsumerModel(data.Value, customer, director, logId, name);
		}

		private IOrderedEnumerable<CheckHistoryModel> GetConsumerHistoryModel(Customer customer, Director director)
		{
			List<MP_ExperianHistory> consumerHistory = director != null ?
				_experianHistoryRepository.GetDirectorConsumerHistory(director.Id).ToList() :
				_experianHistoryRepository.GetCustomerConsumerHistory(customer.Id).ToList();

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

		public ExperianConsumerModel GenerateConsumerModel(ExperianConsumerData eInfo, Customer customer, Director director, long? logId, string name)
		{
			var model = new ExperianConsumerModel { ErrorList = new List<string>(),
				Id = director != null ? director.Id : customer.Id };
			if (eInfo == null || eInfo.ServiceLogId == null)
			{
				model.HasExperianError = true;
				model.ErrorList.Add("No data");
				model.ApplicantFullNameAge = name;
				return model;
			}

			var scorePosColor = GetScorePositionAndColor(eInfo.BureauScore.HasValue ? eInfo.BureauScore.Value : 0, ConsumerScoreMax, ConsumerScoreMin);

			model.ServiceLogId = eInfo.ServiceLogId;
			model.HasExperianError = eInfo.HasExperianError;
			model.ModelType = "Consumer";
			model.CheckDate = eInfo.InsertDate.ToShortDateString();
			model.IsDataRelevant = (DateTime.UtcNow - eInfo.InsertDate).TotalDays < CurrentValues.Instance.UpdateConsumerDataPeriodDays;
			model.CheckValidity = eInfo.InsertDate.AddDays(CurrentValues.Instance.UpdateConsumerDataPeriodDays).ToShortDateString();
			model.BorrowerType = "Consumer";
			model.Score = eInfo.BureauScore;
			model.Odds = Math.Pow(2, (((double)(eInfo.BureauScore ?? 0)) - 600) / 80);
			model.ScorePosition = scorePosColor.Position;
			model.ScoreAlign = scorePosColor.Align;
			model.ScoreValuePosition = scorePosColor.ValPosition;
			model.ScoreColor = scorePosColor.Color;
			model.Applicant = eInfo.Applicants.FirstOrDefault();
			model.Location = eInfo.Locations.FirstOrDefault();
			model.TotalAccountBalances = eInfo.TotalAccountBalances;
			model.TotalMonthlyRepayments = eInfo.CreditCommitmentsRevolving + eInfo.CreditCommitmentsNonRevolving +
			                               eInfo.MortgagePayments;
			model.CreditCardBalances = eInfo.CreditCardBalances;

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

			if (eInfo.Locations.Count > 1) {
				Errors.Add("More than one locations specified");
			}

			model.CII = eInfo.CII;
			if (!string.IsNullOrEmpty(eInfo.Error))
			{
				model.ErrorList.AddRange(eInfo.Error.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
			}

			model.NumberOfAccounts = 0;
			model.NumberOfAccounts3M = 0;
			model.WorstCurrentStatus = AccountStatusDictionary.GetAccountStatus(eInfo.WorstCurrentStatus).LongDescription;
			model.WorstCurrentStatus3M = AccountStatusDictionary.GetAccountStatus(eInfo.WorstHistoricalStatus).LongDescription;
			model.EnquiriesLast3M = eInfo.EnquiriesLast3Months;
			model.EnquiriesLast6M = eInfo.EnquiriesLast6Months;
			model.NumberOfDefaults = 0;
			model.NumberOfCCOverLimit = eInfo.CreditCardOverLimit;
			model.CreditCardUtilization = eInfo.CreditLimitUtilisation;
			model.NOCsOnCCJ = eInfo.NOCsOnCCJ;
			model.NOCsOnCAIS = eInfo.NOCsOnCAIS;
			model.NumberOfCCJs = eInfo.NumCCJs;
			model.TotalCCJValueStr = GetClass1String(eInfo.TotalCCJValue1);
			model.TotalCCJValue = (eInfo.TotalCCJValue1.HasValue && eInfo.TotalCCJValue1.Value > 0) ? eInfo.TotalCCJValue1.Value * 100 : (int?)null;
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
				if (accStatus == DefaultCaisStatusName && var == Variables.FinancialAccounts_MainApplicant) 
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
					var status = AccountStatusDictionary.GetAccountStatus(ws);
					worstStatus[accType] = GetWorstStatus(worstStatus[accType], ws);
					limits[accType] += caisDetails.CreditLimit.HasValue ? caisDetails.CreditLimit.Value : 0;
					balances[accType] += caisDetails.Balance.HasValue ? caisDetails.Balance.Value : 0;

					numberOfAccounts++;
					if ((accountInfo.OpenDate.HasValue) && (accountInfo.OpenDate.Value >= DateTime.Today.AddMonths(-3)))
						numberOfAcc3M++;

					if (status.IsLate)
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

				accountInfo.SettlementDate = caisDetails.SettlementDate ?? caisDetails.LastUpdatedDate;

				if (accountInfo.SettlementDate.HasValue) {
					var histStart = new DateTime(accountInfo.SettlementDate.Value.Year, accountInfo.SettlementDate.Value.Month, 1);

					for (int i = 0; i < caisDetails.AccountStatusCodes.Length; i++)
					{
						var histDate = histStart.AddMonths(-i);
						string indicator = (statuses.Length > i) ? statuses.Substring(i, 1) : string.Empty;
						var idx = displayedMonths.IndexOf(histDate);
						if (idx >= 0) {
							var status = AccountStatusDictionary.GetAccountStatus(indicator);
							sList[idx].Status = status.ShortDescription;
							sList[idx].StatusColor = status.Color;
						}
					}
				}
				accountInfo.LatestStatuses = sList.ToArray();
				accountInfo.TermAndfreq = GetRepaymentPeriodString(caisDetails.RepaymentPeriod);
				accountInfo.Limit = caisDetails.CreditLimit;
				accountInfo.AccBalance = caisDetails.Balance;
				accountInfo.CurrentDefBalance = caisDetails.CurrentDefBalance;

				foreach (var cardHistory in caisDetails.CardHistories)
				{
					accountInfo.CashWithdrawals = string.Format("{0} ({1})", cardHistory.NumCashAdvances, cardHistory.CashAdvanceAmount);
					accountInfo.MinimumPayment = cardHistory.PaymentCode ?? string.Empty;
					break;
				}

				accountInfo.Years = years.ToArray();
				accountInfo.Quarters = quarters.ToArray();
				accountInfo.MonthsDisplayed = monthsList.ToArray();

				if (caisDetails.AccountBalances.Any(x => x.AccountBalance.HasValue)) {
					accountInfo.BalanceHistory = caisDetails.AccountBalances
						.Where(x => x.AccountBalance.HasValue)
						.Select(x => x.AccountBalance.Value.ToString(CultureInfo.InvariantCulture))
						.Reverse()
						.Aggregate((a, b) => a + "," + b);
				}
				accList.Add(accountInfo);
			}

			model.NumberOfAccounts = numberOfAccounts;
			model.NumberOfAccounts3M = numberOfAcc3M;
			model.NumberOfDefaults = numberOfDefaults;
			model.NumberOfLates = numberOfLates;
			model.LateStatus = AccountStatusDictionary.GetAccountStatus(lateStatus).LongDescription;
			model.DefaultAmount = defaultAmount;

			Log.DebugFormat("Accounts List length: {0}", accList.Count);
			accList.Sort(new AccountInfoComparer());

			model.AccountsInformation = accList.ToArray();

			model.ConsumerAccountsOverview.OpenAccounts_CC = accounts[0];
			model.ConsumerAccountsOverview.OpenAccounts_Mtg = accounts[1];
			model.ConsumerAccountsOverview.OpenAccounts_PL = accounts[2];
			model.ConsumerAccountsOverview.OpenAccounts_Other = accounts[3];
			model.ConsumerAccountsOverview.OpenAccounts_Total = accounts.Sum();

			model.ConsumerAccountsOverview.WorstArrears_CC = AccountStatusDictionary.GetAccountStatus(worstStatus[0]).LongDescription;
			model.ConsumerAccountsOverview.WorstArrears_Mtg = AccountStatusDictionary.GetAccountStatus(worstStatus[1]).LongDescription;
			model.ConsumerAccountsOverview.WorstArrears_PL = AccountStatusDictionary.GetAccountStatus(worstStatus[2]).LongDescription;
			model.ConsumerAccountsOverview.WorstArrears_Other = AccountStatusDictionary.GetAccountStatus(worstStatus[3]).LongDescription;
			model.ConsumerAccountsOverview.WorstArrears_Total = AccountStatusDictionary.GetAccountStatus(GetWorstStatus(worstStatus)).LongDescription;

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

			model.ConsumerHistory = GetConsumerHistoryModel(customer, director);
			return model;
		}

		private string GetClass1String(int? totalCcjValue1) {
			if (!totalCcjValue1.HasValue) {
				return "";
			}
			switch (totalCcjValue1.Value) {
			case -1:
				return "No trace block";
			case 0:
				return "No relevant CAIS";
			case 999:
				return "£99,801+";
			default:
				return string.Format("£{0:N0} - £{1:N0}", ((totalCcjValue1.Value - 1) * 100), (totalCcjValue1.Value * 100));
			}
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
			var summary = new Summary();
			summary.Score = model.Consumer.Score;
			summary.ConsumerIndebtednessIndex = model.Consumer.CII;
			summary.CheckDate = model.Consumer.CheckDate;
			summary.Validtill = model.Consumer.CheckValidity;
			summary.IsDataRelevant = model.Consumer.IsDataRelevant;
			summary.WorstCurrentstatus = model.Consumer.WorstCurrentStatus;
			summary.WorstHistoricalstatus = model.Consumer.WorstCurrentStatus3M;
			summary.Numberofdefaults = model.Consumer.NumberOfDefaults;
			summary.Accounts = model.Consumer.NumberOfAccounts;
			summary.CCJs = model.Consumer.NumberOfCCJs;
			summary.MostrecentCCJ = model.Consumer.AgeOfMostRecentCCJ;
			summary.TotalCCJValue = model.Consumer.TotalCCJValueStr;
			summary.Creditcardutilization = model.Consumer.CreditCardUtilization;
			summary.Enquiriesinlast6months = model.Consumer.EnquiriesLast6M;
			summary.Enquiriesinlast3months = model.Consumer.EnquiriesLast3M;
			if (model.Consumer.ConsumerAccountsOverview != null)
			{
				summary.Totalbalance = model.Consumer.ConsumerAccountsOverview.Balance_Total;
			}
			summary.AML = model.AmlInfo.AMLResult;
			summary.AMLnum = model.AmlInfo.AuthenticationIndex.ToString(CultureInfo.InvariantCulture);
			summary.BWA = model.BavInfo.BankAccountVerificationResult;
			summary.BWAnum = GetBwaScoreInfo(model.BavInfo);
			summary.ThinFile = model.Consumer.AccountsInformation == null || model.Consumer.AccountsInformation.Length == 0;
			return summary;
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

		public static string GetAccountStatusString(string status, out string dateType, bool shouldAddDaysSuffix = false)
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
					if (shouldAddDaysSuffix)
					{
						return status + " days";
					}

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
