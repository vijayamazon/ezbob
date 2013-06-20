namespace EzBob.Web.Areas.Underwriter.Controllers.ApplicationReview
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Model;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using ExperianLib;
	using ExperianLib.Dictionaries;
	using ExperianLib.Ebusiness;
	using ExperianLib.IdIdentityHub;
	using ApplicationCreator;
	using Models;
	using Code;
	using Scorto.Web;
	using Infrastructure;

    public class CreditBureauController : Controller
    {
        private readonly CustomerRepository _customers;
        private readonly IAppCreator _creator;
        private readonly IUsersRepository _users;
        private readonly IApplicationRepository _applications;
        private readonly IEzBobConfiguration _config;
        private readonly ConcentAgreementHelper _concentAgreementHelper;

        public CreditBureauController(CustomerRepository customers, IAppCreator creator,
                                        IUsersRepository users, IApplicationRepository applications, IEzBobConfiguration config)
        {
            _customers = customers;
            _creator = creator;
            _users = users;
            _applications = applications;
            _config = config;
            _concentAgreementHelper = new ConcentAgreementHelper();
        }


        [HttpPost]
        [Transactional]
        public JsonNetResult RunCheck(int Id)
        {
            var anyApps = _applications.StratagyIsRunning(Id, _config.ScoringResultStrategyName);
            if (anyApps)
                return this.JsonNet(new { Message = "The evaluation strategy is already running. Please wait..." });

            _creator.Evaluate(_users.Get(Id), NewCreditLineOption.UpdateEverythingExceptMp, true);
            return this.JsonNet(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult Index(int id)
        {
            var customer = _customers.Get(id);
            var customerMainAddress = customer.AddressInfo.PersonalAddress.ToList().FirstOrDefault();

            var loc = new EzBobIntegration.Web_References.Consumer.InputLocationDetailsMultiLineLocation();
            if (customerMainAddress != null)
            {
                loc.LocationLine1 = customerMainAddress.Line1;
                loc.LocationLine2 = customerMainAddress.Line2;
                loc.LocationLine3 = customerMainAddress.Line3;
                loc.LocationLine4 = customerMainAddress.Town;
                loc.LocationLine5 = customerMainAddress.County;
                loc.LocationLine6 = customerMainAddress.Postcode;
            }
            
            var model = new CreditBureauModel();
            try
            {
                var consumerSrv = new ConsumerService();
				var result = consumerSrv.GetConsumerInfo(customer.PersonalInfo.FirstName, customer.PersonalInfo.Surname,
                                                         customer.PersonalInfo.Gender.ToString(), // should be Gender
                                                         customer.PersonalInfo.DateOfBirth, null, loc, "PL", customer.Id, 0, true);

                model = generateConsumerModel(customer.Id, result);

                model.Name = customer.PersonalInfo.FirstName;
                model.MiddleName = customer.PersonalInfo.MiddleInitial;
                model.Surname = customer.PersonalInfo.Surname;
                model.FullName = customer.PersonalInfo.Fullname;
                model.CompanyName = string.Empty;

                model.BorrowerType = customer.PersonalInfo.TypeOfBusiness.ToString();
                model.ConsumerSummaryCharacteristics.DSRandOwnershipType = customer.PersonalInfo.ResidentialStatus;
               
                model.AmlInfo = new AMLInfo
                                    {
                                        AMLResult = string.IsNullOrEmpty(customer.AMLResult)
                                                        ? "Verification was not performed"
                                                        : customer.AMLResult
                                    };
                appendAMLInfo(model.AmlInfo, customer, customerMainAddress);

                model.BavInfo = new BankAccountVerificationInfo
                                    {
                                        BankAccountVerificationResult = string.IsNullOrEmpty(customer.BWAResult)
                                                                            ? "Verification was not performed"
                                                                            : customer.BWAResult
                                    };
                appendBAVInfo(model.BavInfo, customer, customerMainAddress);
                
                if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited) // limited
                {
                    var srv = new EBusinessService();
                    var limitedInfo = srv.GetLimitedBusinessData(customer.LimitedInfo.LimitedRefNum, customer.Id, true);

                    appendLimitedInfo(model, limitedInfo);
                    model.BorrowerType = customer.PersonalInfo.TypeOfBusiness.ToString();
                    model.CompanyName = customer.LimitedInfo.LimitedCompanyName;

                    model.directorsModels = generateDirectorsModels(id, customer.LimitedInfo.Directors);
                }
                else if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.NonLimited) // NonLimited
                {
                    var srv = new EBusinessService();
                    var nonLimitedInfo = srv.GetNotLimitedBusinessData(customer.NonLimitedInfo.NonLimitedRefNum, customer.Id, true);
                    appendNonLimitedInfo(model, nonLimitedInfo);
                    model.BorrowerType = customer.PersonalInfo.TypeOfBusiness.ToString();
                    model.CompanyName = customer.NonLimitedInfo.NonLimitedCompanyName;

                    model.directorsModels = generateDirectorsModels(id, customer.NonLimitedInfo.Directors);
                }

                model.Summary = new Summary
                    {
                        Score = model.Score.ToString(CultureInfo.InvariantCulture),
                        ConsumerIndebtednessIndex = model.CII,
                        CheckDate = model.CheckDate,
                        Validtill = model.CheckValidity,
                        WorstCurrentstatus = model.ConsumerSummaryCharacteristics.WorstCurrentStatus,
                        WorstHistoricalstatus = model.ConsumerSummaryCharacteristics.WorstCurrentStatus3M,
                        Numberofdefaults = model.ConsumerSummaryCharacteristics.NumberOfDefaults,
                        Accounts = model.ConsumerSummaryCharacteristics.NumberOfAccounts,
                        CCJs = model.ConsumerSummaryCharacteristics.NumberOfCCJs,
                        MostrecentCCJ =  model.ConsumerSummaryCharacteristics.AgeOfMostRecentCCJ,
                        DSRandownershiptype = model.ConsumerSummaryCharacteristics.DSRandOwnershipType,
                        Creditcardutilization = model.ConsumerSummaryCharacteristics.CreditCardUtilization,
                        Enquiriesinlast6months = model.ConsumerSummaryCharacteristics.EnquiriesLast6M,
                        Enquiriesinlast3months = model.ConsumerSummaryCharacteristics.EnquiriesLast3M,
                        Totalbalance = model.ConsumerAccountsOverview.Balance_Total,

                        AML = model.AmlInfo.AMLResult,
                        AMLnum = model.AmlInfo.AuthenticationIndexType.ToString(CultureInfo.InvariantCulture),
                        BWA = model.BavInfo.BankAccountVerificationResult,
                        BWAnum = GetBwaScoreInfo(model.BavInfo),
                        Businesstype = model.BorrowerType,
                        BusinessScore = GetBusinessScore(model),
                        RiskLevel = model.LimitedInfo != null ? model.LimitedInfo.RiskLevel : "-",
                        Existingbusinessloans = GetExistingBusinessLoans(model),
                        ConsumerAccountsOverview = model.ConsumerAccountsOverview
                    };

            }
            catch (Exception e)
            {
                model.ErrorList.Add(e.Message);
            }

            return this.JsonNet(model);
        }

        public CreditBureauModel[] generateDirectorsModels(int customerId, IEnumerable<Director> directors)
        {
            var customer = _customers.Get(customerId);
            var consumerSrv = new ConsumerService();
            var dirModelList = new List<CreditBureauModel>();
            foreach (var director in directors)
            {
                var directorAddresses = director.DirectorAddressInfo != null
                                            ? director.DirectorAddressInfo.AllAddresses
                                            : null;
                var directorMainAddress = directorAddresses != null && directorAddresses.Any()
                                              ? directorAddresses.First()
                                              : null;
                var dirLoc = new EzBobIntegration.Web_References.Consumer.InputLocationDetailsMultiLineLocation();
                if (directorMainAddress != null)
                {
                    dirLoc.LocationLine1 = directorMainAddress.Line1;
                    dirLoc.LocationLine2 = directorMainAddress.Line2;
                    dirLoc.LocationLine3 = directorMainAddress.Line3;
                    dirLoc.LocationLine4 = directorMainAddress.Town;
                    dirLoc.LocationLine5 = directorMainAddress.County;
                    dirLoc.LocationLine6 = directorMainAddress.Postcode;
                }

                var result = consumerSrv.GetConsumerInfo(director.Name, director.Surname,
                                                 director.Gender.ToString(),
                                                 director.DateOfBirth, null, dirLoc, "PL", customer.Id, 0, true);
                var dirModel = generateConsumerModel(-1, result);
                dirModel.Name = director.Name;
                dirModel.MiddleName = director.Middle;
                dirModel.Surname = director.Surname;
                dirModel.FullName = string.Format("{0} {1} {2}", director.Name, director.Middle, director.Surname);
                dirModel.Id = director.Id;
                dirModelList.Add(dirModel);
            }
            return dirModelList.ToArray();
        }

        protected void GetScorePositionAndColor(double score, out string position, out string align,
                                                out string valPosition, out string color)
        {
            const int w = 640;

            const int barWidth = 8;
            const int spaceWidth = 1;
            const int barCount = w/barWidth;
            const int redWidth = 80;
            const int yellowX = 240;
            const int greenWidth = 240;

            const int ScoreMax = 1400;
            const int ScoreMin = 120;

            var s = (int)((score - ScoreMin) / (ScoreMax - ScoreMin) * barCount);
            s = (s < 0) ? 0 : s;
            s = (s >= barCount) ? barCount - 1 : s;

            var virtualX = (s*barWidth + barWidth/2);
            var c = Color.White;
            if (virtualX < redWidth)
                c = Color.FromArgb(255, 0, 0);
            else if (virtualX < yellowX)
            {
                var t = cup(1.0 - 1.0*(virtualX - redWidth)/(yellowX - redWidth));
                c = Color.FromArgb(255, (int) (255*t), 0);
            }
            else if (virtualX < w - greenWidth)
            {
                var t = cup(1.0 - 1.0*(w - greenWidth - virtualX)/(w - greenWidth - yellowX));
                c = Color.FromArgb((int) (255*t), 255, 0);
            }
            else if (virtualX >= w - greenWidth)
                c = Color.FromArgb(0, 255, 0);

            int barPos = s*(barWidth + spaceWidth);
            int scorePos = (s < barCount / 2) ? barPos : barPos - 120;
            int valPos = (s < barCount / 2) ? 8 : 0;
            align = (s < barCount / 2) ? "left" : "right";

            position = string.Format("{0}px;", scorePos);
            valPosition = string.Format("{0}px;", valPos);

            color = string.Format("#{0:X2}{1:X2}{2:X2};", c.R, c.G, c.B);
        }

        private static double cup(double x)
        {
            return ((x <= -1) || (x >= 1)) ? 0.0 : Math.Exp(1.0/(x*x - 1))*Math.E;
        }

        protected CreditBureauModel generateRandomModel(int id)
        {
            var r = new Random();
            var score = r.Next(-200, 1600);
            var odds = Math.Round(Math.Pow(2, (score - 600.0)/80.0), 2);
            string pos;
            string align;
            string valPos;
            string color;
            GetScorePositionAndColor(score, out pos, out align, out valPos, out color);

            return new CreditBureauModel
                       {
                           Id = id,
                           Score = score,
                           ScorePosition = pos,
                           ScoreAlign = align,
                           ScoreValuePosition = valPos,
                           ScoreColor = color,
                           CheckStatus = "Passed",
                           CheckIcon = "icon-ok",
                           ButtonStyle = "btn-success"
                       };
        }

        protected CreditBureauModel generateNotQualifiedModel(int id)
        {
            return new CreditBureauModel
                       {
                           Id = id,
                           Score = 0,
                           CheckStatus = "Not Qualified",
                           CheckIcon = "icon-remove-sign",
                           ButtonStyle = "btn-danger",
                           CheckDate = string.Empty,
                           CheckValidity = string.Empty
                       };
        }

        protected const int StatusHistoryMonths = 24;

        protected CreditBureauModel generateConsumerModel(int id, ConsumerServiceResult eInfo)
        {
            double score = (eInfo != null) ? eInfo.BureauScore : 0.0;

            double odds = Math.Pow(2, (score - 600)/80);
            string pos;
            string align;
            string valPos;
            string color;

            GetScorePositionAndColor(score, out pos, out align, out valPos, out color);

            var checkStatus = ((eInfo == null) || !string.IsNullOrEmpty(eInfo.Error))
                                  ? "Error"
                                  : eInfo.ExperianResult;

            var checkIcon = "icon-remove-sign";
            var buttonStyle = "btn-danger";
            switch (checkStatus)
            {
                case "Passed":
                    checkIcon = "icon-ok-sign";
                    buttonStyle = "btn-success";
                    break;
                case "Referred":
                    checkIcon = "icon-question-sign";
                    buttonStyle = "btn-warning";
                    break;
                case "Rejected":
                    checkIcon = "icon-remove-sign";
                    buttonStyle = "btn-danger";
                    break;
            }

            var checkDate = (eInfo != null) ? (DateTime?) eInfo.LastUpdateDate : null;
            var checkValidity = (checkDate != null) ? (DateTime?) checkDate.Value.AddMonths(3) : null;

            Errors = new List<string>();

            var model = new CreditBureauModel
                            {
                                Id = id,

                                IsExperianError = (eInfo != null) && eInfo.IsExpirianError,
                                ModelType = "Consumer",

                                CheckStatus = checkStatus,
                                CheckIcon = checkIcon,
                                ButtonStyle = buttonStyle,
                                CheckDate = (checkDate != null) ? checkDate.Value.ToShortDateString() : string.Empty,
                                CheckValidity =
                                    (checkValidity != null) ? checkValidity.Value.ToShortDateString() : string.Empty,
                                BorrowerType = "Consumer",

                                Score = score,
                                Odds = odds,
                                ScorePosition = pos,
                                ScoreAlign = align,
                                ScoreValuePosition = valPos,
                                ScoreColor = color,

                                ConsumerSummaryCharacteristics = new ConsumerSummaryCharacteristics(),
                                ConsumerAccountsOverview = new ConsumerAccountsOverview(),
                            };
            if ((eInfo != null) && !string.IsNullOrEmpty(eInfo.Error))
            {
                model.ErrorList.AddRange(eInfo.Error.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries));
            }

            if(eInfo == null) 
                return model;

            model.ConsumerSummaryCharacteristics = new ConsumerSummaryCharacteristics
                                                       {
                                                           NumberOfAccounts = string.Empty,
                                                           NumberOfAccounts3M = string.Empty,
                                                           WorstCurrentStatus = string.Empty,
                                                           WorstCurrentStatus3M = string.Empty,

                                                           EnquiriesLast3M = eInfo.EnquiriesLast3Months.ToString(),
                                                           EnquiriesLast6M = eInfo.EnquiriesLast6Months.ToString(),

                                                           NumberOfDefaults = string.Empty,
                                                           NumberOfCCJs = string.Empty,
                                                           AgeOfMostRecentCCJ = string.Empty,
                                                           NumberOfCCOverLimit = eInfo.CreditCardOverLimit.ToString(),
                                                           CreditCardUtilization =
                                                               eInfo.CreditLimitUtilisation.ToString(),
                                                           DSRandOwnershipType = string.Empty,
                                                           NOCsOnCCJ = string.Empty,
                                                           NOCsOnCAIS = string.Empty
                                                       };
            model.ConsumerAccountsOverview =
                new ConsumerAccountsOverview
                    {
                        OpenAccounts_CC = string.Empty,
                        WorstArrears_CC = string.Empty,
                        TotalCurLimits_CC = string.Empty,
                        Balance_CC = string.Empty,

                        OpenAccounts_PL = string.Empty,
                        WorstArrears_PL = string.Empty,
                        TotalCurLimits_PL = string.Empty,
                        Balance_PL = string.Empty,

                        OpenAccounts_Mtg = string.Empty,
                        WorstArrears_Mtg = string.Empty,
                        TotalCurLimits_Mtg = string.Empty,
                        Balance_Mtg = string.Empty,

                        OpenAccounts_Other = string.Empty,
                        WorstArrears_Other = string.Empty,
                        TotalCurLimits_Other = string.Empty,
                        Balance_Other = string.Empty,

                        OpenAccounts_Total = string.Empty,
                        WorstArrears_Total = string.Empty,
                        TotalCurLimits_Total = string.Empty,
                        Balance_Total = string.Empty
                    };

            TryRead(() => model.CII = eInfo.Output.Output.ConsumerSummary.PremiumValueData.CII.NDSPCII,
                    "CII");
            TryRead(() => model.ConsumerSummaryCharacteristics.NumberOfCCJs = eInfo.Output.Output.ConsumerSummary.Summary.PublicInfo.E1A01,
                    "# of CCJ's");
            TryRead(() => model.ConsumerSummaryCharacteristics.AgeOfMostRecentCCJ = eInfo.Output.Output.ConsumerSummary.Summary.PublicInfo.E1A03,
                    "Age of most recent CCJ");

            TryRead(() => model.ConsumerSummaryCharacteristics.SatisfiedJudgements = eInfo.Output.Output.ConsumerSummary.Summary.PublicInfo.EA4Q06,
                    "Satisfied Judgements");

            TryRead(() => model.ConsumerSummaryCharacteristics.NOCsOnCCJ = eInfo.Output.Output.ConsumerSummary.Summary.NOC.EA4Q02,
                    "NOCs on CCJ");

            TryRead(() => model.ConsumerSummaryCharacteristics.NOCsOnCAIS = eInfo.Output.Output.ConsumerSummary.Summary.NOC.EA4Q04,
                    "NOCs on CAIS");

            model.ConsumerSummaryCharacteristics.CAISSpecialInstructionFlag = string.Empty;
            TryRead(() => model.ConsumerSummaryCharacteristics.CAISSpecialInstructionFlag = eInfo.Output.Output.ConsumerSummary.Summary.CAIS.EA1F04,
                    "CAIS Special Instruction Flag");
            if (string.IsNullOrEmpty(model.ConsumerSummaryCharacteristics.CAISSpecialInstructionFlag))
                model.ConsumerSummaryCharacteristics.CAISSpecialInstructionFlag = "-";

            var NDHAC05 = string.Empty;
            var NDHAC09 = string.Empty;

            TryRead(() =>
                    NDHAC05 = eInfo.Output.Output.ConsumerSummary.Summary.CAIS.NDHAC05,
                    "Current Worst status (fine) on active non revolving CAIS");
            TryRead(() =>
                    NDHAC09 = eInfo.Output.Output.ConsumerSummary.Summary.CAIS.NDHAC09,
                    "Worst status (fine) in last 6m on mortgage accounts");

            var worst = GetWorstStatus((NDHAC05 ?? string.Empty).Trim(), (NDHAC09 ?? string.Empty).Trim());
            model.ConsumerSummaryCharacteristics.WorstCurrentStatus = AccountStatusDictionary.GetDetailedAccountStatusString(worst);

            TryRead(() =>
                    model.ConsumerSummaryCharacteristics.WorstCurrentStatus3M =
                    eInfo.Output.Output.FullConsumerData.ConsumerDataSummary.SummaryDetails.CAISSummary.WorstHistorical,
                    "Worst Historical Status");
            model.ConsumerSummaryCharacteristics.WorstCurrentStatus3M =
                AccountStatusDictionary.GetDetailedAccountStatusString(model.ConsumerSummaryCharacteristics.WorstCurrentStatus3M);


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
                var monthQ = (date.Month - 1)/3 + 1;
                var monthQStr = string.Format("Q{0}", monthQ);
                var month = date.Month;
                if ((years.Count > 0) && (years[years.Count - 1].Year == monthYear))
                    years[years.Count - 1].Span++;
                else
                    years.Add(new AccountDisplayedYear {Year = date.Year, Span = 1});

                if ((quarters.Count > 0) && (quarters[quarters.Count - 1].Quarter == monthQStr))
                    quarters[quarters.Count - 1].Span++;
                else
                    quarters.Add(new AccountDisplayedQuarter {Quarter = monthQStr, Span = 1});
                monthsList.Add(month.ToString("00"));
            }

            var numberOfAccounts = 0;
            var numberOfAcc3M = 0;

            // 0 - CC, 1 - Mortgage, 2 - PL, 3 - other
            var accounts = new[] {0, 0, 0, 0};
            var worstStatus = new[] {"0", "0", "0", "0"};
            var limits = new[] {0.0, 0.0, 0.0, 0.0};
            var balances = new[] {0.0, 0.0, 0.0, 0.0};
            int numberOfDefaults = 0;

            try
            {
                if (eInfo.Output.Output.FullConsumerData.ConsumerData.CAIS != null)
                    foreach (var caisData in eInfo.Output.Output.FullConsumerData.ConsumerData.CAIS)
                    {
                        foreach (var caisDetails in caisData.CAISDetails)
                        {
                            var accountInfo = new AccountInfo();
                            DateTime? openDate;
                            try
                            {
                                openDate = new DateTime(caisDetails.CAISAccStartDate.CCYY,
                                                        caisDetails.CAISAccStartDate.MM,
                                                        caisDetails.CAISAccStartDate.DD);
                            }
                            catch
                            {
                                openDate = null;
                            }
                            accountInfo.OpenDate = (openDate == null)
                                                       ? string.Empty
                                                       : openDate.Value.ToShortDateString();
                            accountInfo.Account =
                                AccountTypeDictionary.GetAccountType(caisDetails.AccountType ?? string.Empty);
                            //accountInfo.TermAndfreq =
                            //    PaymentFrequencyDictionary.GetPaymentFrequency(caisDetails.PaymentFrequency ?? string.Empty);
                            var accStatus = caisDetails.AccountStatus;
                            string dateType;
                            accountInfo.AccountStatus = GetAccountStatusString(accStatus, out dateType);
                            accountInfo.DateType = dateType;
                            if (accStatus == "F")
                                numberOfDefaults++;

                            var accType = getAccountType(caisDetails.AccountType);
                            if (accType < 0)
                                continue;

                            if ((accStatus == "D") || (accStatus == "A"))
                            {
                                accounts[accType]++;
                                var ws = caisDetails.WorstStatus;
                                worstStatus[accType] = GetWorstStatus(worstStatus[accType], ws);

                                double limit = 0;
                                if ((caisDetails.CreditLimit != null) && (caisDetails.CreditLimit.Amount != null))
                                {
                                    string l = caisDetails.CreditLimit.Amount.Replace("£", "");
                                    double.TryParse(l, out limit);
                                }
                                limits[accType] += limit;

                                double balance = 0;
                                if ((caisDetails.Balance != null) && (caisDetails.Balance.Amount != null))
                                {
                                    string b = caisDetails.Balance.Amount.Replace("£", "");
                                    double.TryParse(b, out balance);
                                }
                                balances[accType] += balance;

                                numberOfAccounts++;
                                if ((openDate != null) && (openDate.Value >= DateTime.Today.AddMonths(-3)))
                                    numberOfAcc3M++;
                            }

                            string statuses = caisDetails.AccountStatusCodes ?? string.Empty;
                            int mthsCount = 0;
                            int.TryParse(caisDetails.NumOfMonthsHistory ?? "0", out mthsCount);

                            var sList = new List<AccountStatus>();
                            for (int i = 0; i < StatusHistoryMonths; i++)
                            {
                                sList.Add(new AccountStatus {Status = "", StatusColor = "white"});
                            }

                            int relevantYear, relevantMonth, relevantDay;
                            if (caisDetails.SettlementDate != null)
                            {
                                relevantYear = caisDetails.SettlementDate.CCYY;
                                relevantMonth = caisDetails.SettlementDate.MM;
                                relevantDay = caisDetails.SettlementDate.DD;
                            }
                            else
                            {
                                relevantYear = caisDetails.LastUpdatedDate.CCYY;
                                relevantMonth = caisDetails.LastUpdatedDate.MM;
                                relevantDay = caisDetails.LastUpdatedDate.DD;
                            }

                            var histStart = new DateTime(relevantYear, relevantMonth, 1);
                            accountInfo.SettlementDate =
                                FormattingUtils.FormatDateToString(new DateTime(relevantYear, relevantMonth, relevantDay));

                            for (int i = 0; i < mthsCount; i++)
                            {
                                var histDate = histStart.AddMonths(-i);
                                string indicator = (statuses.Length > i) ? statuses.Substring(i, 1) : string.Empty;
                                var idx = displayedMonths.IndexOf(histDate);
                                if (idx >= 0)
                                    sList[idx].Status = AccountStatusDictionary.GetAccountStatusString(indicator);
                            }

                            accountInfo.LatestStatuses = sList.ToArray();

                            int repaymentPeriod = 0;
                            int.TryParse(caisDetails.RepaymentPeriod ?? string.Empty, out repaymentPeriod);

                            accountInfo.TermAndfreq = GetRepaymentPeriodString(repaymentPeriod);

                            accountInfo.Limit = caisDetails.CreditLimit.Amount;
                            accountInfo.AccBalance = caisDetails.Balance.Amount;

                            accountInfo.CashWithdrawals = string.Empty;
                            accountInfo.MinimumPayment = string.Empty;

                            if (caisDetails.CardHistories != null)
                            {
                                foreach (var cardHistory in caisDetails.CardHistories)
                                {
                                    accountInfo.CashWithdrawals = string.Format(
                                        "{0} ({1})", cardHistory.NumCashAdvances ?? "0",
                                        cardHistory.CashAdvanceAmount ?? "0");
                                    accountInfo.MinimumPayment = cardHistory.PaymentCode ?? string.Empty;
                                    break;
                                }
                            }

                            accountInfo.Years = years.ToArray();
                            accountInfo.Quarters = quarters.ToArray();
                            accountInfo.MonthsDisplayed = monthsList.ToArray();

                            accList.Add(accountInfo);
                        }
                    }
            }
            catch (Exception)
            {
                Errors.Add("Can`t read values for Financial Accounts");
            }

            model.ConsumerSummaryCharacteristics.NumberOfAccounts = numberOfAccounts.ToString();
            model.ConsumerSummaryCharacteristics.NumberOfAccounts3M = numberOfAcc3M.ToString();
            model.ConsumerSummaryCharacteristics.NumberOfDefaults = numberOfDefaults.ToString();

	        accList.Sort(new AccountInfoComparer());
            model.AccountsInformation = accList.ToArray();

            model.ConsumerAccountsOverview.OpenAccounts_CC = accounts[0].ToString();
            model.ConsumerAccountsOverview.OpenAccounts_Mtg = accounts[1].ToString();
            model.ConsumerAccountsOverview.OpenAccounts_PL = accounts[2].ToString();
            model.ConsumerAccountsOverview.OpenAccounts_Other = accounts[3].ToString();
            model.ConsumerAccountsOverview.OpenAccounts_Total = (accounts[0] + accounts[1] + accounts[2] + accounts[3]).ToString();

            model.ConsumerAccountsOverview.WorstArrears_CC = AccountStatusDictionary.GetDetailedAccountStatusString(worstStatus[0]);
            model.ConsumerAccountsOverview.WorstArrears_Mtg = AccountStatusDictionary.GetDetailedAccountStatusString(worstStatus[1]);
            model.ConsumerAccountsOverview.WorstArrears_PL = AccountStatusDictionary.GetDetailedAccountStatusString(worstStatus[2]);
            model.ConsumerAccountsOverview.WorstArrears_Other = AccountStatusDictionary.GetDetailedAccountStatusString(worstStatus[3]);
            var totalWorst = "0";
            totalWorst = GetWorstStatus(totalWorst, worstStatus[0]);
            totalWorst = GetWorstStatus(totalWorst, worstStatus[1]);
            totalWorst = GetWorstStatus(totalWorst, worstStatus[2]);
            totalWorst = GetWorstStatus(totalWorst, worstStatus[3]);
            model.ConsumerAccountsOverview.WorstArrears_Total = AccountStatusDictionary.GetDetailedAccountStatusString(totalWorst);

            model.ConsumerAccountsOverview.TotalCurLimits_CC = limits[0].ToString();
            model.ConsumerAccountsOverview.TotalCurLimits_Mtg = limits[1].ToString();
            model.ConsumerAccountsOverview.TotalCurLimits_PL = limits[2].ToString();
            model.ConsumerAccountsOverview.TotalCurLimits_Other = limits[3].ToString();
            model.ConsumerAccountsOverview.TotalCurLimits_Total = (limits[0] + limits[1] + limits[2] + limits[3]).ToString();

            model.ConsumerAccountsOverview.Balance_CC = balances[0].ToString();
            model.ConsumerAccountsOverview.Balance_Mtg = balances[1].ToString();
            model.ConsumerAccountsOverview.Balance_PL = balances[2].ToString();
            model.ConsumerAccountsOverview.Balance_Other = balances[3].ToString();
            model.ConsumerAccountsOverview.Balance_Total = (balances[0] + balances[1] + balances[2] + balances[3]).ToString();

            var nocList = new List<NOCInfo>();
            try
            {
                if(eInfo.Output.Output.FullConsumerData.ConsumerData.NOC != null)
                    foreach (var nocInfo in eInfo.Output.Output.FullConsumerData.ConsumerData.NOC)
                    {
                        nocList.AddRange(nocInfo.NoCDetails.Select(nocDetails => new NOCInfo {NOCReference = nocDetails.Reference, NOCLines = nocDetails.TextLine}));
                    }
            }
            catch (Exception)
            {
                Errors.Add("Can`t read values for NOCs");
            }
            model.NOCs = nocList.ToArray();

            if(!eInfo.IsExpirianError)
            {
                model.ErrorList.AddRange(Errors);
            }

            return model;
        }

        protected void appendLimitedInfo(CreditBureauModel model, LimitedResults eInfo)
        {
            if(eInfo == null)
                return;

            model.ModelType = "Limited";
            model.LimitedInfo = new ExperianLimitedInfo
            {
                BureauScore = eInfo.BureauScore,
                RiskLevel = (eInfo.BureauScore > 90) ? "Low Risk" : ((eInfo.BureauScore < 40) ? "High Risk" : "Medium Risk"),
                ExistingBusinessLoans = eInfo.ExistingBusinessLoans
            };

            if (!string.IsNullOrEmpty(eInfo.Error))
            {
                model.ErrorList.AddRange(eInfo.Error.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        protected void appendNonLimitedInfo(CreditBureauModel model, NonLimitedResults eInfo)
        {
            if (eInfo == null)
                return;
            model.ModelType = "NonLimited";
            model.NonLimitedInfo = new ExperianNonLimitedInfo
                                       {
                                           BureauScore = eInfo.BureauScore,
                                           CompanyNotFoundOnBureau = eInfo.CompanyNotFoundOnBureau
                                       };
            if (!string.IsNullOrEmpty(eInfo.Error))
            {
                model.ErrorList.AddRange(eInfo.Error.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        protected void appendAMLInfo(AMLInfo data, EZBob.DatabaseLib.Model.Database.Customer customer, CustomerAddress customerAddress)
        {
            var srv = new IdHubService();
            var result = srv.Authenticate(customer.PersonalInfo.FirstName, string.Empty, customer.PersonalInfo.Surname,
                                          customer.PersonalInfo.Gender.ToString(),
                                          customer.PersonalInfo.DateOfBirth.HasValue ? customer.PersonalInfo.DateOfBirth.Value : DateTime.Now,
                                          customerAddress.Line1, customerAddress.Line2, customerAddress.Line3,
                                          customerAddress.Town, customerAddress.County, customerAddress.Postcode,
                                          customer.Id, true);
            if(null == result)
                return;
            data.AuthIndexText = result.AuthIndexText;
            data.AuthenticationIndexType = result.AuthenticationIndexType;
            data.NumPrimDataItems = result.NumPrimDataItems;
            data.NumPrimDataSources = result.NumPrimDataSources;
            data.NumSecDataItems = result.NumSecDataItems;
            data.ReturnedHRPCount = result.ReturnedHRPCount;
            data.StartDateOldestPrim = result.StartDateOldestPrim;
            data.StartDateOldestSec = result.StartDateOldestSec;
        }

        protected void appendBAVInfo(BankAccountVerificationInfo data, EZBob.DatabaseLib.Model.Database.Customer customer, CustomerAddress customerAddress)
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
                                                 bankAccount!=null ? bankAccount.SortCode : "",
                                                 bankAccount != null ? bankAccount.AccountNumber : "", 
                                                 customer.Id, true);
            if (null == result)
                return;
            data.AddressScore = result.AddressScore;
            data.NameScore = result.NameScore;
            data.AuthenticationText = result.AuthenticationText;
            data.AccountStatus = result.AccountStatus;
        }

        protected List<string> Errors = new List<string>();

        #region helper functions

        private void TryRead(Action a, string key)
        {
            try
            {
                a();
            }
            catch
            {
                Errors.Add("Can`t read value for: " + key);
            }
        }

        protected static List<string> statusScale = new List<string> 
        {"D", "U", "S", "?", "0", "1", "2", "3", "4", "5", "6", "8" ,"9" };

        protected string GetWorstStatus(string s1, string s2)
        {
            var idx1 = statusScale.IndexOf(s1);
            var idx2 = statusScale.IndexOf(s2);
            if (idx1 < 0)
                return s2;
            if (idx2 < 0)
                return s1;
            return (idx1 < idx2) ? s2 : s1;
        }

        // 0 - CC, 1 - Mortgage, 2 - PL, 3 - other
        protected static string cardsAccounts = "04,05,06,37,38";
        protected static string mortgageAccounts = "03,16,25,30,31,32,33,34,35,69";
        protected static string loanAccounts = "00,01,02,17,19,22,26,27,28,29";
        protected static string otherAccounts = "07,08,12,13,14,15,18,20,21,23,24,36,39,40,41,42,43,44,45,46,47,48,49,50,51,53,54,55,56,57,58,59,60,61,62,63,64,70";
        
        protected int getAccountType(string accType)
        {
            if (cardsAccounts.IndexOf(accType) >= 0)
                return 0;
            if (mortgageAccounts.IndexOf(accType) >= 0)
                return 1;
            if (loanAccounts.IndexOf(accType) >= 0)
                return 2;
            if (otherAccounts.IndexOf(accType) >= 0)
                return 3;
            return -1;
        }

        protected string GetRepaymentPeriodString(int months)
        {
            if (months == 999)
            {
                return "-";
            }
            int repaymentYears = months / 12;
            int repaymentMonths = months % 12;
            if(repaymentYears > 0)
            {
                if(repaymentMonths > 0)
                    return string.Format("{0} year(s) {1} month(s)", repaymentYears, repaymentMonths);
                return string.Format("{0} year(s)", repaymentYears);
            }
            return string.Format("{0} months", repaymentMonths);
        }

        protected string GetAccountStatusString(string status, out string dateType)
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

        #endregion

        [Ajax]
        [HttpGet]
        public JsonNetResult IdHubCustomAddress(int id)
        {
            var customer = _customers.Get(id);

            var model = new IdHubCustomAddressModel
                            {
                                Id = id,
                                Firstname = customer.PersonalInfo.FirstName ?? string.Empty,
                                MiddleName = customer.PersonalInfo.MiddleInitial ?? string.Empty,
                                Surname = customer.PersonalInfo.Surname ?? string.Empty,
                                FullName = customer.PersonalInfo.Fullname ?? string.Empty,
                                Gender = customer.PersonalInfo.Gender.ToString(),
                                DateOfBirth = customer.PersonalInfo.DateOfBirth.HasValue ? customer.PersonalInfo.DateOfBirth.Value.ToShortDateString() : string.Empty,
                                BankAccount = string.Empty,
                                SortCode = string.Empty
                            };
            if(customer.BankAccount != null)
            {
                model.BankAccount = customer.BankAccount.AccountNumber;
                model.SortCode = customer.BankAccount.SortCode;
            }

            var customerMainAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault();
            if(customerMainAddress != null)
            {
                model.CurAddressLine1 = customerMainAddress.Line1 ?? string.Empty;
                model.CurAddressLine2 = customerMainAddress.Line2 ?? string.Empty;
                model.CurAddressLine3 = customerMainAddress.Line3 ?? string.Empty;
                model.CurAddressTown = customerMainAddress.Town ?? string.Empty;
                model.CurAddressCounty = customerMainAddress.County ?? string.Empty;
                model.CurAddressPostcode = customerMainAddress.Postcode ?? string.Empty;
                model.CurAddressCountry = customerMainAddress.Country ?? string.Empty;
            }

            var customerPrevAddress = customer.AddressInfo.PrevPersonAddresses.FirstOrDefault();
            if (customerPrevAddress != null)
            {
                model.PrevAddressLine1 = customerPrevAddress.Line1 ?? string.Empty;
                model.PrevAddressLine2 = customerPrevAddress.Line2 ?? string.Empty;
                model.PrevAddressLine3 = customerPrevAddress.Line3 ?? string.Empty;
                model.PrevAddressTown = customerPrevAddress.Town ?? string.Empty;
                model.PrevAddressCounty = customerPrevAddress.County ?? string.Empty;
                model.PrevAddressPostcode = customerPrevAddress.Postcode ?? string.Empty;
                model.PrevAddressCountry = customerPrevAddress.Country ?? string.Empty;
            }

            var service = new IdHubService();
            var parsedAddress = service.FillAddress(model.CurAddressLine1, model.CurAddressLine2, model.CurAddressLine3,
                                                    model.CurAddressTown, model.CurAddressCounty,
                                                    model.CurAddressPostcode);
            if (parsedAddress != null)
            {
                model.IdHubAddressHouseNumber = parsedAddress.AddressDetail.HouseNumber ?? string.Empty;
                model.IdHubAddressHouseName = parsedAddress.AddressDetail.HouseName ?? string.Empty;
                model.IdHubAddressStreet = parsedAddress.AddressDetail.Address1 ?? string.Empty;
                model.IdHubAddressDistrict = parsedAddress.AddressDetail.Address2 ?? string.Empty;
                model.IdHubAddressTown = parsedAddress.AddressDetail.Address3 ?? string.Empty;
                model.IdHubAddressCounty = parsedAddress.AddressDetail.Address4 ?? string.Empty;
                model.IdHubAddressPostcode = parsedAddress.AddressDetail.PostCode ?? string.Empty;
                model.IdHubAddressCountry = parsedAddress.AddressDetail.Country ?? string.Empty;
            }
            return this.JsonNet(model);
        }

        [HttpPost]
        [Transactional]
        public JsonNetResult RunAMLBWACheck(int id, int checkType, string houseNumber, string houseName, string street,
                                            string district, string town, string county, string postcode, string bankAccount, string sortCode)
        {
            var isRunning = _applications.StratagyIsRunning(id, _config.ScoringResultStrategyName);

            if (isRunning)
                return this.JsonNet(new { Message = "The evaluation strategy is already running. Please wait..." });

            _creator.EvaluateWithIdHubCustomAddress(_users.Get(id), checkType, houseNumber, houseName, street,
                                            district, town, county, postcode, bankAccount, sortCode);
            return this.JsonNet(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
        }

        public ActionResult DownloadConsentAgreement(int id)
        {
            var customer = _customers.Get(id);
            var pdf = _concentAgreementHelper.GenerateWidhDataBase(customer);
            var fileName = _concentAgreementHelper.GetFileName(customer);
            return File(pdf, "application/pdf", fileName);
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

        private static string GetBwaScoreInfo(BankAccountVerificationInfo info)
        {
            return
                string.Format("{0}, {1}, {2}",
                info.NameScore, 
                info.AddressScore,
                string.IsNullOrEmpty(info.AccountStatus) ? "-" : info.AccountStatus);
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

	    public class AccountInfoComparer : IComparer<AccountInfo>
		{
			private static readonly Dictionary<string, int> sortedValues = new Dictionary<string, int> { { DefaultStatusName, 1 }, { DelinquentStatusName, 2 }, { ActiveStatusName, 3 }, { SettledStatusName, 4 } };

			public int Compare(AccountInfo x, AccountInfo y)
			{
				if (sortedValues.ContainsKey(x.AccountStatus) && sortedValues.ContainsKey(y.AccountStatus))
				{
					return sortedValues[x.AccountStatus].CompareTo(sortedValues[y.AccountStatus]);
				}

				if (sortedValues.ContainsKey(x.AccountStatus))
				{
					return 1;
				}

				if (sortedValues.ContainsKey(y.AccountStatus))
				{
					return -1;
				}

				return 0;
			}
		}
    }
}