namespace EzBob.Backend.Strategies
{
	using System;
	using System.Data;
	using System.IO;
	using System.Text;
	using DbConnection;
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

				string Line23Scalar = Line2 + " " + Line3;
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

				//if (CustomerStatus == "Collection")
				//{

				//}
			}

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
