namespace EzBob.Backend.Strategies
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using DbConnection;
	using ExperianLib.CaisFile;
	using Models;
	using log4net;

	public class CaisGenerator
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(CaisGenerator));
		private readonly StrategiesMailer mailer = new StrategiesMailer();
		private readonly StrategyHelper strategyHelper = new StrategyHelper();


		private readonly object caisGenerationLock = new object();
		private int caisGenerationTriggerer = -1;
		private string CaisPath;
		private string CaisPath2;
		private int DaysBetween;
		private string AccountStatus;
		private string CAISFlag;
		private decimal MonthlyPayment;
		private DateTime StratSatartDate;
		private decimal OriginalDefaultBalance;
		private int BusinessCounter;
		private int BusinessGoodCounter;
		private int BusinessDefaultsCounter;
		private int ConsumerCounter;
		private int ConsumerGoodCounter;
		private int ConsumerDefaultsCounter;
		private string CompanyTypeCode;
		private string CompanyRefNum;

		public CaisGenerator()
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetCaisFoldersPaths");
			DataRow results = dt.Rows[0];

			CaisPath = results["CaisPath"].ToString();
			CaisPath2 = results["CaisPath2"].ToString();
		}


		public void CAISGenerate(int underwriterId)
		{
			lock (caisGenerationLock)
			{
				if (caisGenerationTriggerer != -1)
				{
					log.WarnFormat("A CAIS generation is already in progress. Triggered by Underwriter:{0}", caisGenerationTriggerer);
					return;
				}
				caisGenerationTriggerer = underwriterId;
			}

			StratSatartDate = DateTime.UtcNow;

			string timeString = DateTime.UtcNow.ToString("%Y-%m-%d-%H-%M-%S"); // TODO: check out real path format!!!

			string dir_path = CaisPath + "\\" + timeString;
			string dir_path2 = CaisPath2 + "\\" + timeString;
			Directory.CreateDirectory(dir_path);
			Directory.CreateDirectory(dir_path2);




			DataTable dt = DbConnection.ExecuteSpReader("GetCaisData");
			foreach (DataRow row in dt.Rows)
			{
				int loanID = int.Parse(row["loanID"].ToString());
				int CustomerId = int.Parse(row["CustomerId"].ToString());
				DateTime StartDate = DateTime.Parse(row["StartDate"].ToString());
				DateTime DateClose = DateTime.Parse(row["DateClose"].ToString());//
				int MaxDelinquencyDays = int.Parse(row["MaxDelinquencyDays"].ToString());
				int RepaymentPeriod = int.Parse(row["RepaymentPeriod"].ToString());//
				decimal CurrentBalance = decimal.Parse(row["CurrentBalance"].ToString());
				string Gender = row["Gender"].ToString();
				string FirstName = row["FirstName"].ToString();
				string MiddleInitial = row["MiddleInitial"].ToString();
				string Surname = row["Surname"].ToString();
				string RefNumber = row["RefNumber"].ToString();
				string Line1 = row["Line1"].ToString();
				string Line2 = row["Line2"].ToString();
				string Line3 = row["Line3"].ToString();
				string Town = row["Town"].ToString();
				string County = row["County"].ToString();
				string Postcode = row["Postcode"].ToString();
				DateTime DateOfBirth = DateTime.Parse(row["DateOfBirth"].ToString());

				DateTime? MinLSDate = null;
				DateTime tmp;
				if (DateTime.TryParse(row["MinLSDate"].ToString(), out tmp))
				{
					MinLSDate = tmp;
				}
				decimal LoanAmount = decimal.Parse(row["LoanAmount"].ToString());
				int SceduledRepayments = int.Parse(row["SceduledRepayments"].ToString()); // TODO: fix to ScheduledRepayments
				string CompanyType = row["CompanyType"].ToString();
				string LimitedRefNum = row["LimitedRefNum"].ToString();
				string NonLimitedRefNum = row["NonLimitedRefNum"].ToString();
				string CustomerState = row["CustomerState"].ToString();
				string SortCode = row["SortCode"].ToString();
				bool IsDefaulted = bool.Parse(row["IsDefaulted"].ToString());
				string CaisAccountStatus = row["CaisAccountStatus"].ToString(); //
				bool CustomerStatusIsEnabled = bool.Parse(row["CustomerStatusIsEnabled"].ToString());
				string MaritalStatus = row["MaritalStatus"].ToString();
				string ManualCaisFlag = row["ManualCaisFlag"].ToString();



				string GenderPrefix;
				if (Gender == "M")
				{
					GenderPrefix = "Mr.";
				}
				else
				{
					GenderPrefix = MaritalStatus == "Married" ? "Mrs." : "Ms.";
				}

				if (!MinLSDate.HasValue)
				{
					DaysBetween = 0;
				}
				else
				{
					DaysBetween = (int)(DateTime.UtcNow - MinLSDate.Value).TotalDays;
				}

				if (string.IsNullOrEmpty(CaisAccountStatus) && CaisAccountStatus != "Calculated value")
				{
					AccountStatus = CaisAccountStatus;
				}
				else
				{
					if (DateClose > StartDate)
					{
						AccountStatus = "0";
					}
					if (DateClose < StartDate && DaysBetween <= 30)
					{
						AccountStatus = "0";
					}
					if (DateClose < StartDate && DaysBetween > 30 && DaysBetween <= 60)
					{
						AccountStatus = "1";
					}





					if (DateClose < StartDate && DaysBetween > 60 && DaysBetween <= 90)
					{
						AccountStatus = "2";
					}
					if (DateClose < StartDate && DaysBetween > 90 && DaysBetween <= 120)
					{
						AccountStatus = "3";
					}
					if (DateClose < StartDate && DaysBetween > 120 && DaysBetween <= 150)
					{
						AccountStatus = "4";
					}
					if (DateClose < StartDate && DaysBetween > 150 && DaysBetween <= 180)
					{
						AccountStatus = "5";
					}
					if (DateClose < StartDate && DaysBetween > 180)
					{
						AccountStatus = "6";
					}
					if (DateClose < StartDate && IsDefaulted)
					{
						AccountStatus = "8";
					}
					if (DateClose < StartDate && !CustomerStatusIsEnabled)
					{
						AccountStatus = "8";
					}
					if (AccountStatus == "8")
					{
						OriginalDefaultBalance = CurrentBalance;
						DateClose = DateTime.UtcNow;
					}
					else
					{
						OriginalDefaultBalance = 0;
					}
				}

				string Line23 = Line2 + " " + Line3;
				string FullName = GenderPrefix + " " + FirstName + " " + MiddleInitial + " " + Surname;

				CAISFlag = null;
				if (SceduledRepayments != 0)
				{
					MonthlyPayment = LoanAmount / (decimal)SceduledRepayments;
				}
				else
				{
					MonthlyPayment = 0;
				}

				if (AccountStatus != "8" && DateClose > StartDate)
				{
					CurrentBalance = 0;
				}

				string TransferredToCollectionFlag = CustomerState == "Collection" ? "Y" : string.Empty;


				string AccountNumber = RefNumber + loanID;

				if (CompanyType == "Entrepreneur")
				{
					var file = CaisFileManager.GetCaisFileData();
					var h = file.Header;
					h.SourceCodeNumber = 402;
					h.DateCreation = DateTime.UtcNow;
					h.CompanyPortfolioName = "Orange Money";
					h.OverdraftReportingCutOff = 0;
					h.IsCardsBehaviouralSharing = false;

					var account = new AccountRecord();

					account.AccountNumber = AccountNumber;
					account.AccountType = "02";
					account.StartDate = StartDate;
					account.CloseDate = StartDate > DateClose ? DateTime.MinValue : DateClose;
					account.MonthlyPayment = Convert.ToInt32(MonthlyPayment);
					account.RepaymentPeriod = Convert.ToInt32(SceduledRepayments);
					account.CurrentBalance = Convert.ToInt32(CurrentBalance);
					account.CreditBalanceIndicator = "";
					account.AccountStatus = AccountStatus;
					account.SpecialInstructionIndicator = "";
					account.PaymentAmount = 0;
					account.CreditPaymentIndicator = "";
					account.PreviousStatementBalance = 0;
					account.PreviousStatementBalanceIndicator = "";
					account.NumberCashAdvances = 0;
					account.ValueCashAdvances = 0;
					account.PaymentCode = "";
					account.PromotionActivityFlag = "";
					account.TransientAssociationFlag = "";
					account.AirtimeFlag = "";
					account.FlagSettings = CAISFlag;
					account.NameAndAddress = new NameAndAddressData();
					account.NameAndAddress.Name = FullName;
					account.NameAndAddress.AddressLine1 = Line1;
					account.NameAndAddress.AddressLine2 = Line23;
					account.NameAndAddress.AddressLine3 = Town;
					account.NameAndAddress.AddressLine4 = County;
					account.NameAndAddress.Postcode = Postcode;
					account.CreditLimit = 0;
					account.DateBirth = DateOfBirth;
					account.TransferredCollectionAccountFlag = TransferredToCollectionFlag;
					account.BalanceType = "";
					account.CreditTurnover = 0;
					account.PrimaryAccountIndicator = "";
					account.DefaultSatisfactionDate = DateTime.MinValue;
					account.TransactionFlag = "";
					account.OriginalDefaultBalance = Convert.ToInt32(OriginalDefaultBalance);
					account.PaymentFrequency = "M";
					account.NewAccountNumber = "";

					file.Accounts.Add(account);

					ConsumerCounter++;
					if (AccountStatus == "0")
					{
						ConsumerGoodCounter++;
					}
					else if (AccountStatus == "8")
					{
						ConsumerDefaultsCounter++;
					}
				}
				else
				{
					if (CompanyType == "Limited" || CompanyType == "PShip" || CompanyType == "LLP")
					{
						CompanyTypeCode = "L";
						CompanyRefNum = LimitedRefNum;
					}
					else if (CompanyType == "PShip3P" || CompanyType == "SoleTrader")
					{
						CompanyTypeCode = "N";
						CompanyRefNum = NonLimitedRefNum;
					}








					var cais = CaisFileManager.GetBusinessCaisFileData();
					cais.Header.CompanyPortfolioName = "Orange Money";
					cais.Header.CreditCardBehaviouralSharingFlag = "";
					cais.Header.DateOfCreation = DateTime.UtcNow;
					cais.Header.SourceCode = 721;

					var record = new BusinessAccountRecord();
					record.AccountNumber = AccountNumber;
					record.ProprietorPartnerDirectorNumber = 0;
					record.LimitedNonlimitedAndOtherFlag = CompanyTypeCode;


					record.NameAddressRegisteredOfficeTradingAddress.Name = FullName;
					record.NameAddressRegisteredOfficeTradingAddress.AddressLine1 = Line1;
					record.NameAddressRegisteredOfficeTradingAddress.AddressLine2 = Line23;
					record.NameAddressRegisteredOfficeTradingAddress.AddressLine3 = Town;
					record.NameAddressRegisteredOfficeTradingAddress.AddressLine4 = County;
					record.NameAddressRegisteredOfficeTradingAddress.PostCode = Postcode;

					record.AddressType = "";
					record.NameChange = "N";
					record.CompanyRegisteredNumberBusinessNumber = CompanyRefNum;
					record.SICCode = 0;
					record.VATNumber = "";
					record.YearBusinessStarted = 0;
					record.AdditionalTradingStyle = "";
					record.BusinessCompanyTelephoneNumber = "";
					record.BusinessCompanyWebsite = "";
					record.PointOfContactName = "";
					record.PointOfContactEmailAddress = "";
					record.PointOfContactTelephoneNumber = "";
					record.PointOfContactJobTitle = "";

					record.ParentCompanyNameAddress.Name = "";
					record.ParentCompanyNameAddress.AddressLine1 = "";
					record.ParentCompanyNameAddress.AddressLine2 = "";
					record.ParentCompanyNameAddress.AddressLine3 = "";
					record.ParentCompanyNameAddress.AddressLine4 = "";
					record.ParentCompanyNameAddress.PostCode = "";

					record.ParentCompanyRegisteredNumber = "";
					record.ParentCompanyTelephoneNumber = "";
					record.ParentCompanyVATNumber = "";

					record.PreviousNameandAddress.Name = "";
					record.PreviousNameandAddress.AddressLine1 = "";
					record.PreviousNameandAddress.AddressLine2 = "";
					record.PreviousNameandAddress.AddressLine3 = "";
					record.PreviousNameandAddress.AddressLine4 = "";
					record.PreviousNameandAddress.PostCode = "";

					record.ProprietorPartnerDirectororOtherFlag = "";
					record.SignatoryontheAccountFlag = "";
					record.ShareholdersFlag = "";
					record.CountryofRegistration = "";
					record.DateofBirth = DateTime.MinValue;
					record.ProprietorsDirectorsGuarantee = "";
					record.ProprietorsDirectorsGuaranteeCancelledDischarged = "";
					record.AccountType = 2;

					record.StartDateofAgreement = StartDate;
					if (StartDate > DateClose)
					{
						record.CloseDateofAgreement = DateTime.MinValue;
					}
					else
					{
						record.CloseDateofAgreement = DateClose;
					}
					record.MonthlyPayment = Convert.ToInt32(MonthlyPayment);
					record.RepaymentPeriod = Convert.ToInt32(SceduledRepayments);
					record.CurrentBalance = Convert.ToInt32(CurrentBalance);
					record.CreditBalanceIndicator = "";
					record.AccountStatus = AccountStatus;
					record.SpecialInstructionIndicator = "";

					record.CreditLimit = 0;
					record.FlagSettings = CAISFlag;
					record.Debenture = "";
					record.MortgageFlags = "";
					record.AirtimeStatusFlag = "";
					record.TransferredtoCollectionAccountFlag = TransferredToCollectionFlag;
					record.BalanceType = "";
					record.CreditTurnover = 0;
					record.PrimaryAccountIndicator = "";
					record.DefaultSatisfactionDate = DateTime.MinValue;
					record.RejectionFlag = "";
					record.BankerDetailsSortCode = Convert.ToInt32(SortCode);
					record.OriginalDefaultBalance = Convert.ToInt32(OriginalDefaultBalance);
					record.PaymentFrequencyIndicator = "M";
					record.NumberofCreditCardsissued = 0;

					record.PaymentAmount = 0;
					record.PaymentCreditIndicator = "";
					record.PreviousStatementBalance = 0;
					record.PreviousStatementBalanceIndicator = "";
					record.NumberofCashAdvances = 0;
					record.ValueofCashAdvances = 0;
					record.PaymentCode = "";
					record.PromotionActivityFlag = "";
					record.PaymentType = "B";
					record.NewAccountNumber = "";
					record.NewProprietorPartnerDirectorNumber = "";


					cais.Accounts.Add(record);













					BusinessCounter++;
					if (AccountStatus == "0")
					{
						BusinessGoodCounter++;
					}
					else if (AccountStatus == "8")
					{
						BusinessDefaultsCounter++;
					}




				}

				DbConnection.ExecuteSpNonQuery("UpdateLastReportedCAISstatus",
				    DbConnection.CreateParam("LoanId", loanID),
				    DbConnection.CreateParam("CAISStatus", AccountStatus));
			}

			// Mail addresses
			// 4 more nodes
			var variables = new Dictionary<string, string>
				{
					{"CurrDate", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)},
					{"Path", dir_path}
				};
			mailer.SendToEzbob(variables, "Mandrill - CAIS report", "CAIS Report generated");

			string BusinessPath = dir_path + "\\F1364.D.COMCAIS.ORMO.DI55CUST.INPUT";
			string consumerPath = dir_path + "\\F530.E.OMO.MSTEI49.XMIT";
			int ConsumerCompanyType = 1;
			int BusinessCompanyType = 2;
			string BusinessFilename = "F1364.D.COMCAIS.ORMO.DI55CUST";
			string ConsumerFilename = "F530.E.OMO.MSTEI49";




			var b = CaisFileManager.GetBusinessCaisFileData();
			var CAISstring = b.WriteToString();
			b.WriteToFile(dir_path + "\\F1364.D.COMCAIS.ORMO.DI55CUST.INPUT");
			b.WriteToFile(dir_path2 + "\\F1364.D.COMCAIS.ORMO.DI55CUST.INPUT");
			strategyHelper.SaveCAISFile(CAISstring, "F1364.D.COMCAIS.ORMO.DI55CUST.INPUT", dir_path, 2, BusinessCounter,
			                            BusinessGoodCounter, BusinessDefaultsCounter);
			CaisFileManager.RemoveBusinessCaisFileData();

			var c = CaisFileManager.GetCaisFileData();
			CAISstring = c.WriteToString();
			c.WriteToFile(dir_path + "\\F530.E.OMO.MSTEI49.XMIT");
			c.WriteToFile(dir_path2 + "\\F530.E.OMO.MSTEI49.XMIT");
			strategyHelper.SaveCAISFile(CAISstring, "F530.E.OMO.MSTEI49.XMIT", dir_path, 1, ConsumerCounter, ConsumerGoodCounter,
			                            ConsumerDefaultsCounter);
			CaisFileManager.RemoveCaisFileData();


			lock (caisGenerationLock)
			{
				caisGenerationTriggerer = -1;
			}
		}

		public void CAISUpdate(int caisId)
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetCaisFileData");
			DataRow results = dt.Rows[0];

			string fileName = results["FileName"].ToString();
			string dirName = results["DirName"].ToString();

			var unzippedFileContent = strategyHelper.GetCAISFileById(caisId);
			File.WriteAllText(string.Format("{0}\\{1}", dirName, fileName), unzippedFileContent, Encoding.ASCII);
		}
	}
}
