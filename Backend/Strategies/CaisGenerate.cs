namespace EzBob.Backend.Strategies
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using ExperianLib.CaisFile;
	using Models;

	public class CaisGenerate : AStrategy
	{
		public CaisGenerate(int underwriterId, AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
			mailer = new StrategiesMailer(Db, Log);

			DataTable dt = Db.ExecuteReader("GetCaisFoldersPaths", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];

			caisPath = results["CaisPath"].ToString();
			caisPath2 = results["CaisPath2"].ToString();
			this.underwriterId = underwriterId;
		}

		public override string Name {
			get { return "CAIS Generator"; }
		} // Name

		public override void Execute() {
			lock (caisGenerationLock) {
				if (caisGenerationTriggerer != -1) {
					Log.Warn("A CAIS generation is already in progress. Triggered by Underwriter:{0}", caisGenerationTriggerer);
					return;
				} // if
				caisGenerationTriggerer = underwriterId;
			} // lock

			Generate();

			lock (caisGenerationLock) {
				caisGenerationTriggerer = -1;
			} // lock
		} // Execute
		
		private void Generate()
		{
			string timeString = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");

			string dirPath = caisPath + "\\" + timeString;
			string dirPath2 = caisPath2 + "\\" + timeString;
			Directory.CreateDirectory(dirPath);
			Directory.CreateDirectory(dirPath2);

			DataTable dt = Db.ExecuteReader("GetCaisData", CommandSpecies.StoredProcedure);
			foreach (DataRow row in dt.Rows)
			{
				int loanId = int.Parse(row["loanID"].ToString());
				DateTime startDate = DateTime.Parse(row["StartDate"].ToString());
				DateTime dateClose = DateTime.Parse(row["DateClose"].ToString());
				decimal currentBalance = decimal.Parse(row["CurrentBalance"].ToString());
				string gender = row["Gender"].ToString();
				string firstName = row["FirstName"].ToString();
				string middleInitial = row["MiddleInitial"].ToString();
				string surname = row["Surname"].ToString();
				string refNumber = row["RefNumber"].ToString();
				string line1 = row["Line1"].ToString();
				string line2 = row["Line2"].ToString();
				string line3 = row["Line3"].ToString();
				string town = row["Town"].ToString();
				string county = row["County"].ToString();
				string postcode = row["Postcode"].ToString();
				DateTime dateOfBirth = DateTime.Parse(row["DateOfBirth"].ToString());

				DateTime? minLsDate = null;
				DateTime tmp;
				if (DateTime.TryParse(row["MinLSDate"].ToString(), out tmp))
				{
					minLsDate = tmp;
				}
				decimal loanAmount = decimal.Parse(row["LoanAmount"].ToString());
				int scheduledRepayments = int.Parse(row["ScheduledRepayments"].ToString());
				string companyType = row["CompanyType"].ToString();
				string limitedRefNum = row["LimitedRefNum"].ToString();
				string nonLimitedRefNum = row["NonLimitedRefNum"].ToString();
				string customerState = row["CustomerState"].ToString();
				string sortCode = row["SortCode"].ToString();
				bool isDefaulted = Convert.ToBoolean(row["IsDefaulted"]);
				string caisAccountStatus = row["CaisAccountStatus"].ToString();
				bool customerStatusIsEnabled = Convert.ToBoolean(row["CustomerStatusIsEnabled"]);
				string maritalStatus = row["MaritalStatus"].ToString();

				string genderPrefix;
				if (gender == "M")
				{
					genderPrefix = "Mr.";
				}
				else
				{
					genderPrefix = maritalStatus == "Married" ? "Mrs." : "Ms.";
				}

				accountStatus = GetAccountStatus(minLsDate, caisAccountStatus, dateClose, startDate, isDefaulted, customerStatusIsEnabled);

				if (accountStatus == "8")
				{
					originalDefaultBalance = currentBalance;
					dateClose = DateTime.UtcNow;
				}
				else
				{
					originalDefaultBalance = 0;
				}

				string line23 = string.Format("{0} {1}", line2, line3);
				string fullName = string.Format("{0} {1} {2} {3}", genderPrefix, firstName, middleInitial, surname);

				if (scheduledRepayments != 0)
				{
					monthlyPayment = loanAmount / scheduledRepayments;
				}
				else
				{
					monthlyPayment = 0;
				}

				if (accountStatus != "8" && dateClose > startDate)
				{
					currentBalance = 0;
				}

				string transferredToCollectionFlag = customerState == "Collection" ? "Y" : string.Empty;
				string accountNumber = refNumber + loanId;

				if (companyType == "Entrepreneur")
				{
					var file = CaisFileManager.GetCaisFileData();
					var h = file.Header;
					h.SourceCodeNumber = 402;
					h.DateCreation = DateTime.UtcNow;
					h.CompanyPortfolioName = "Orange Money";
					h.OverdraftReportingCutOff = 0;
					h.IsCardsBehaviouralSharing = false;

					var account = CreateConsumerRecord(accountNumber, startDate, dateClose, scheduledRepayments, currentBalance, fullName, line1, line23, town, county, postcode, dateOfBirth, transferredToCollectionFlag);

					file.Accounts.Add(account);

					consumerCounter++;
					if (accountStatus == "0")
					{
						consumerGoodCounter++;
					}
					else if (accountStatus == "8")
					{
						consumerDefaultsCounter++;
					}
				}
				else
				{
					if (companyType == "Limited" || companyType == "PShip" || companyType == "LLP")
					{
						companyTypeCode = "L";
						companyRefNum = limitedRefNum;
					}
					else if (companyType == "PShip3P" || companyType == "SoleTrader")
					{
						companyTypeCode = "N";
						companyRefNum = nonLimitedRefNum;
					}

					var cais = CaisFileManager.GetBusinessCaisFileData();
					cais.Header.CompanyPortfolioName = "Orange Money";
					cais.Header.CreditCardBehaviouralSharingFlag = "";
					cais.Header.DateOfCreation = DateTime.UtcNow;
					cais.Header.SourceCode = 721;

					var record = CreateBusinessRecord(accountNumber, fullName, line1, line23, town, county, postcode, startDate, dateClose, scheduledRepayments, currentBalance, transferredToCollectionFlag, sortCode);
					
					cais.Accounts.Add(record);
					
					businessCounter++;
					if (accountStatus == "0")
					{
						businessGoodCounter++;
					}
					else if (accountStatus == "8")
					{
						businessDefaultsCounter++;
					}
				}

				Db.ExecuteNonQuery("UpdateLastReportedCAISstatus",
					CommandSpecies.StoredProcedure,
					new QueryParameter("LoanId", loanId),
					new QueryParameter("CAISStatus", accountStatus)
				);
			}

			var variables = new Dictionary<string, string>
				{
					{"CurrDate", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)},
					{"Path", dirPath}
				};
			mailer.SendToEzbob(variables, "Mandrill - CAIS report", "CAIS Report generated");
			
			var businessCaisFileData = CaisFileManager.GetBusinessCaisFileData();
			businessCaisFileData.WriteToFile(dirPath + "\\F1364.D.COMCAIS.ORMO.DI55CUST.INPUT");
			businessCaisFileData.WriteToFile(dirPath2 + "\\F1364.D.COMCAIS.ORMO.DI55CUST.INPUT");
			strategyHelper.SaveCAISFile(businessCaisFileData.WriteToString(), "F1364.D.COMCAIS.ORMO.DI55CUST.INPUT", dirPath, 2, businessCounter,
			                            businessGoodCounter, businessDefaultsCounter);
			CaisFileManager.RemoveBusinessCaisFileData();

			var consumerCaisFileData = CaisFileManager.GetCaisFileData();
			consumerCaisFileData.WriteToFile(dirPath + "\\F530.E.OMO.MSTEI49.XMIT");
			consumerCaisFileData.WriteToFile(dirPath2 + "\\F530.E.OMO.MSTEI49.XMIT");
			strategyHelper.SaveCAISFile(consumerCaisFileData.WriteToString(), "F530.E.OMO.MSTEI49.XMIT", dirPath, 1, consumerCounter, consumerGoodCounter,
			                            consumerDefaultsCounter);
			CaisFileManager.RemoveCaisFileData();
		}

		private string GetAccountStatus(DateTime? minLsDate, string caisAccountStatus, DateTime dateClose, DateTime startDate, bool isDefaulted,
		                      bool customerStatusIsEnabled)
		{
			int daysBetween;
			if (!minLsDate.HasValue)
			{
				daysBetween = 0;
			}
			else
			{
				daysBetween = (int)(DateTime.UtcNow - minLsDate.Value).TotalDays;
			}

			if (string.IsNullOrEmpty(caisAccountStatus) && caisAccountStatus != "Calculated value")
			{
				return caisAccountStatus;
			}
			
			if (dateClose < startDate)
			{
				if (isDefaulted || !customerStatusIsEnabled)
				{
					return "8";
				}
				if (daysBetween > 180)
				{
					return "6";
				}
				if (daysBetween > 150 && daysBetween <= 180)
				{
					return "5";
				}
				if (daysBetween > 120 && daysBetween <= 150)
				{
					return "4";
				}
				if (daysBetween > 90 && daysBetween <= 120)
				{
					return "3";
				}
				if (daysBetween > 60 && daysBetween <= 90)
				{
					return "2";
				}
				if (daysBetween > 30 && daysBetween <= 60)
				{
					return "1";
				}
				if (daysBetween <= 30)
				{
					return "0";
				}
			}

			return "0";
		}

		private AccountRecord CreateConsumerRecord(string accountNumber, DateTime startDate, DateTime dateClose, int scheduledRepayments,
		                                    decimal currentBalance, string fullName, string line1, string line23, string town,
		                                    string county, string postcode, DateTime dateOfBirth,
		                                    string transferredToCollectionFlag)
		{
			// TODO: investigate if we need all the assigments to 0 and empty strings
			var account = new AccountRecord
				{
					AccountNumber = accountNumber,
					AccountType = "02",
					StartDate = startDate,
					CloseDate = startDate > dateClose ? DateTime.MinValue : dateClose,
					MonthlyPayment = Convert.ToInt32(monthlyPayment),
					RepaymentPeriod = Convert.ToInt32(scheduledRepayments),
					CurrentBalance = Convert.ToInt32(currentBalance),
					CreditBalanceIndicator = string.Empty,
					AccountStatus = accountStatus,
					SpecialInstructionIndicator = string.Empty,
					PaymentAmount = 0,
					CreditPaymentIndicator = string.Empty,
					PreviousStatementBalance = 0,
					PreviousStatementBalanceIndicator = string.Empty,
					NumberCashAdvances = 0,
					ValueCashAdvances = 0,
					PaymentCode = string.Empty,
					PromotionActivityFlag = string.Empty,
					TransientAssociationFlag = string.Empty,
					AirtimeFlag = string.Empty,
					FlagSettings = null,
					NameAndAddress = new NameAndAddressData
						{
							Name = fullName,
							AddressLine1 = line1,
							AddressLine2 = line23,
							AddressLine3 = town,
							AddressLine4 = county,
							Postcode = postcode
						},
					CreditLimit = 0,
					DateBirth = dateOfBirth,
					TransferredCollectionAccountFlag = transferredToCollectionFlag,
					BalanceType = string.Empty,
					CreditTurnover = 0,
					PrimaryAccountIndicator = string.Empty,
					DefaultSatisfactionDate = DateTime.MinValue,
					TransactionFlag = string.Empty,
					OriginalDefaultBalance = Convert.ToInt32(originalDefaultBalance),
					PaymentFrequency = "M",
					NewAccountNumber = string.Empty
				};

			return account;
		}

		private BusinessAccountRecord CreateBusinessRecord(string accountNumber, string fullName, string line1, string line23,
		                                            string town, string county, string postcode, DateTime startDate,
		                                            DateTime dateClose, int scheduledRepayments, decimal currentBalance,
		                                            string transferredToCollectionFlag, string sortCode)
		{
			// TODO: investigate if we need all the assigments to 0 and empty strings
			var record = new BusinessAccountRecord
				{
					AccountNumber = accountNumber,
					ProprietorPartnerDirectorNumber = 0,
					LimitedNonlimitedAndOtherFlag = companyTypeCode,
					AddressType = string.Empty,
					NameChange = "N",
					CompanyRegisteredNumberBusinessNumber = companyRefNum,
					SICCode = 0,
					VATNumber = string.Empty,
					YearBusinessStarted = 0,
					AdditionalTradingStyle = string.Empty,
					BusinessCompanyTelephoneNumber = string.Empty,
					BusinessCompanyWebsite = string.Empty,
					PointOfContactName = string.Empty,
					PointOfContactEmailAddress = string.Empty,
					PointOfContactTelephoneNumber = string.Empty,
					PointOfContactJobTitle = string.Empty,
					ParentCompanyRegisteredNumber = string.Empty,
					ParentCompanyTelephoneNumber = string.Empty,
					ParentCompanyVATNumber = string.Empty,
					ProprietorPartnerDirectororOtherFlag = string.Empty,
					SignatoryontheAccountFlag = string.Empty,
					ShareholdersFlag = string.Empty,
					CountryofRegistration = string.Empty,
					DateofBirth = DateTime.MinValue,
					ProprietorsDirectorsGuarantee = string.Empty,
					ProprietorsDirectorsGuaranteeCancelledDischarged = string.Empty,
					AccountType = 2,
					StartDateofAgreement = startDate,
					CloseDateofAgreement = startDate > dateClose ? DateTime.MinValue : dateClose,
					MonthlyPayment = Convert.ToInt32(monthlyPayment),
					RepaymentPeriod = Convert.ToInt32(scheduledRepayments),
					CurrentBalance = Convert.ToInt32(currentBalance),
					CreditBalanceIndicator = string.Empty,
					AccountStatus = accountStatus,
					SpecialInstructionIndicator = string.Empty,
					CreditLimit = 0,
					FlagSettings = null,
					Debenture = string.Empty,
					MortgageFlags = string.Empty,
					AirtimeStatusFlag = string.Empty,
					TransferredtoCollectionAccountFlag = transferredToCollectionFlag,
					BalanceType = string.Empty,
					CreditTurnover = 0,
					PrimaryAccountIndicator = string.Empty,
					DefaultSatisfactionDate = DateTime.MinValue,
					RejectionFlag = string.Empty,
					BankerDetailsSortCode = Convert.ToInt32(sortCode),
					OriginalDefaultBalance = Convert.ToInt32(originalDefaultBalance),
					PaymentFrequencyIndicator = "M",
					NumberofCreditCardsissued = 0,
					PaymentAmount = 0,
					PaymentCreditIndicator = string.Empty,
					PreviousStatementBalance = 0,
					PreviousStatementBalanceIndicator = string.Empty,
					NumberofCashAdvances = 0,
					ValueofCashAdvances = 0,
					PaymentCode = string.Empty,
					PromotionActivityFlag = string.Empty,
					PaymentType = "B",
					NewAccountNumber = string.Empty,
					NewProprietorPartnerDirectorNumber = string.Empty,
					NameAddressRegisteredOfficeTradingAddress =
						{
							Name = fullName,
							AddressLine1 = line1,
							AddressLine2 = line23,
							AddressLine3 = town,
							AddressLine4 = county,
							PostCode = postcode
						},
					ParentCompanyNameAddress =
						{
							Name = string.Empty,
							AddressLine1 = string.Empty,
							AddressLine2 = string.Empty,
							AddressLine3 = string.Empty,
							AddressLine4 = string.Empty,
							PostCode = string.Empty
						},
					PreviousNameandAddress =
					{
						Name = string.Empty,
						AddressLine1 = string.Empty,
						AddressLine2 = string.Empty,
						AddressLine3 = string.Empty,
						AddressLine4 = string.Empty,
						PostCode = string.Empty
					}
				};
			
			return record;
		}

		private readonly StrategiesMailer mailer;
		private readonly StrategyHelper strategyHelper = new StrategyHelper();

		private readonly object caisGenerationLock = new object();
		private int caisGenerationTriggerer = -1;
		private readonly string caisPath;
		private readonly string caisPath2;
		private string accountStatus;
		private decimal monthlyPayment;
		private decimal originalDefaultBalance;
		private int businessCounter;
		private int businessGoodCounter;
		private int businessDefaultsCounter;
		private int consumerCounter;
		private int consumerGoodCounter;
		private int consumerDefaultsCounter;
		private string companyTypeCode;
		private string companyRefNum;
		private readonly int underwriterId;
	} // CaisGenerator
} // namespace
