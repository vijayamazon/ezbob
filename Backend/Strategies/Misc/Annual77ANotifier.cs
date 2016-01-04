namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	// using DotNetOpenAuth.Messaging;     <------------ It compiles without this. And appears to work without this.
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using IMailLib;
	using IMailLib.Helpers;
	using StructureMap;

	public class Annual77ANotifier : AStrategy {
		public Annual77ANotifier() {
			this.now = DateTime.UtcNow;
			this.collectionIMailer = new CollectionMail(
			   ConfigManager.CurrentValues.Instance.ImailUserName,
			   ConfigManager.CurrentValues.Instance.IMailPassword,
			   ConfigManager.CurrentValues.Instance.IMailDebugModeEnabled,
			   ConfigManager.CurrentValues.Instance.IMailDebugModeEmail,
			   ConfigManager.CurrentValues.Instance.IMailSavePath);
			this.loanRepository = ObjectFactory.GetInstance<ILoanRepository>();
		}

		public override string Name { get { return "Annual77ANotifier"; } } // Name

		public override void Execute() {
			LoadImailTemplates();
			DB.ForEachRowSafe(HandleOneNotification, "LoadLoansForAnnual77ANofication", CommandSpecies.StoredProcedure, new QueryParameter("Now", this.now));
		} // Execute

		public void ExecuteTest(int customerID, int loanID, int originID) {
			LoadImailTemplates();
			var template = this.templates.FirstOrDefault(x => x.IsActive && x.OriginID == originID);
			if (template == null) {
				Log.Debug("No template found for annual 77A notification for originID {2} to customer {0} for loan {1}", customerID, loanID, originID);
				return;
			}
			PrepareAndSendMail(loanID, customerID, template);
		} // Execute

		private ActionResult HandleOneNotification(SafeReader sr, bool bRowSetStart) {
			int loanID = sr["LoanID"];
			int customerID = sr["CustomerID"];
			DateTime? lastSent = sr["LastSent"];
			int originID = sr["OriginID"];

			if (lastSent.HasValue && (this.now - lastSent.Value).TotalDays < 365) {
				Log.Debug("Already sent annual 77A notification to customer {0} for loan {1} on {2}", customerID, loanID, lastSent);
				return ActionResult.Continue;
			}
			string typeOfBusinessStr = sr["TypeOfBusiness"];
			TypeOfBusiness typeOfBusiness;
			if (!Enum.TryParse(typeOfBusinessStr, out typeOfBusiness)) {
				Log.Error("Failed parsing type of business {0} for customer {1}", typeOfBusinessStr, customerID);
				return ActionResult.Continue;
			}
			var template = this.templates.FirstOrDefault(x => x.IsActive && x.OriginID == originID);
			if (template == null) {
				Log.Debug("No template found for annual 77A notification for originID {2} to customer {0} for loan {1}", customerID, loanID, originID);
				return ActionResult.Continue;
			}

			PrepareAndSendMail(loanID, customerID, template);
			return ActionResult.Continue;
		}//HandleOneNotification

		private void PrepareAndSendMail(int loanID, int customerID, SnailMailTemplate template){
			Address address;
			TableModel schedule;
			var variables = PrepareVariables(loanID, customerID, out address, out schedule);

			var metadata = this.collectionIMailer.SendAnual77ANotification(customerID, template, address, variables, schedule, "@ScheduleTable@");
			var collectionLogID = AddCollectionLog(customerID, loanID, SetLateLoanStatus.CollectionType.Annual77ANotification, SetLateLoanStatus.CollectionMethod.Mail);
			SaveCollectionSnailMailMetadata(collectionLogID, metadata);
		}
		private void LoadImailTemplates() {
			List<CollectionSnailMailTemplate> dbTemplates = this.DB.Fill<CollectionSnailMailTemplate>("LoadCollectionSnailMailTemplates", CommandSpecies.StoredProcedure);
			this.templates = dbTemplates
				.Where(x => x.Type == SetLateLoanStatus.CollectionType.Annual77ANotification.ToString())
				.Select(x => new SnailMailTemplate {
					ID = x.CollectionSnailMailTemplateID,
					Type = x.Type,
					OriginID = x.OriginID,
					Template = x.Template,
					IsActive = x.IsActive,
					TemplateName = x.TemplateName,
					FileName = x.FileName,
					IsLimited = x.IsLimited
				});
		}

		private Dictionary<string, string> PrepareVariables(int loanID, int customerID, out Address address, out TableModel scheduleModel) {
			var loan = this.loanRepository.Get(loanID);
			if (loan == null) {
				throw new Exception(string.Format("Loan not found for id {0} customer {1}", loanID, customerID));
			}

			var customer = loan.Customer;
			var customerAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault();
			var customerPersonalInfo = customer.PersonalInfo ?? new PersonalInfo();
			if (customerAddress == null) {
				throw new Exception(string.Format("Customer {0} does not have address", customerID));
			}

			address = new Address {
				Line1 = customerAddress.Line1,
				Line2 = customerAddress.Line2,
				Line3 = customerAddress.Line3,
				Line4 = customerAddress.Town,
				Postcode = customerAddress.Postcode
			};

			var interestRate = loan.InterestRate * 12 * 100;
			var repaymentPeriod = loan.CustomerSelectedTerm ?? loan.CashRequest.RepaymentPeriod;

			scheduleModel = new TableModel();

			var pacnet = loan.PacnetTransactions
				.Select(ScheduleRowModel.CreatePacnet)
				.ToList();
			var paypoint = loan.TransactionsWithPaypointSuccesefull
				.Select(ScheduleRowModel.CreatePaypoint)
				.ToList();
			var fees = loan.Charges.Where(x => x.State != "Paid" && x.State != "Expired")
				.Select(ScheduleRowModel.CreateFees)
				.ToList();
			var schedule = loan.Schedule
				.Where(x => x.Status == LoanScheduleStatus.AlmostPaid || x.Status == LoanScheduleStatus.Late || x.Status == LoanScheduleStatus.StillToPay)
				.Select(ScheduleRowModel.CreateSchedule)
				.ToList();

			var allRows = pacnet;
			allRows.AddRange(paypoint);
			allRows.AddRange(fees);
			allRows.AddRange(schedule);

			scheduleModel.Header = new List<string>{
				{"Type"},
				{"Date"},
				{"Status"},
				{"Interest"},
				{"Principal"},
				{"Fee"},
				{"Total"}
			};
			
			scheduleModel.Content = allRows
				.Select(x => new List<string> {
					{x.Type},
					{x.Date.ToLongUKDate()},
					{x.Status},
					{x.Interest.ToNumeric2Decimals(true)},
					{x.Principal.ToNumeric2Decimals(true)},
					{x.Fees.ToNumeric2Decimals(true)},
					{x.Total.ToNumeric2Decimals(true)},
				})
				.ToList();

			return new Dictionary<string, string> {
				{"CustomerName",customerPersonalInfo.Fullname},
				{"LoanRefNum",loan.RefNumber},
				{"Date",this.now.ToLongUKDate()},
				{"LoanDate",loan.Date.ToLongUKDate()},
				{"LoanAmount",loan.LoanAmount.ToNumeric2Decimals()},
				{"AnnualInterestRatePercent", interestRate.ToNumeric2Decimals()},
				{"LoanTermMonths", repaymentPeriod.ToString()},
				{"ScheduleTable", ""}
			};


		}//PrepareVariables

		private int AddCollectionLog(int customerID, int loanID, SetLateLoanStatus.CollectionType type, SetLateLoanStatus.CollectionMethod method) {
			Log.Info("Adding collection log to customer {0} loan {1} type {2} method {3}", customerID, loanID, type, method);
			return DB.ExecuteScalar<int>("AddCollectionLog",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("LoanID", loanID),
				new QueryParameter("Type", type.ToString()),
				new QueryParameter("Method", method.ToString()),
				new QueryParameter("Now", this.now));
		}//AddCollectionLog

		private void SaveCollectionSnailMailMetadata(int collectionLogID, FileMetadata fileMetadata) {
			if (fileMetadata == null) {
				return;
			}

			Log.Info("Adding collection snail mail metadata collection log id {0} file {1}", collectionLogID, fileMetadata.Name);
			DB.ExecuteNonQuery("AddCollectionSnailMailMetadata",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CollectionLogID", collectionLogID),
				new QueryParameter("Name", fileMetadata.Name),
				new QueryParameter("ContentType", fileMetadata.ContentType),
				new QueryParameter("Path", fileMetadata.Path),
				new QueryParameter("Now", this.now),
				new QueryParameter("CollectionSnailMailTemplateID", fileMetadata.TemplateID));
		}//SaveCollectionSnailMailMetadata

		private readonly DateTime now;
		private readonly CollectionMail collectionIMailer;
		private IEnumerable<SnailMailTemplate> templates;
		private readonly ILoanRepository loanRepository;
	} // class Anual77ANotifier

	public class ScheduleRowModel {
		public string Type { get; set; }
		public string Status { get; set; }
		public DateTime Date { get; set; }
		public decimal? Principal { get; set; }
		public decimal? Interest { get; set; }
		public decimal Fees { get; set; }
		public decimal Total { get; set; }

		public static ScheduleRowModel CreatePacnet(PacnetTransaction p) {
			return new ScheduleRowModel {
				Status = p.Status.ToDescription(),
				Type = "Loan advance",
				Date = p.PostDate,
				Interest = null,
				Principal = null,
				Fees = p.Fees,
				Total = p.Amount
			};
		}

		public static ScheduleRowModel CreatePaypoint(PaypointTransaction p) {
			return new ScheduleRowModel {
				Status = p.Status.ToDescription(),
				Type = "Repayment",
				Date = p.PostDate,
				Interest = p.Interest,
				Principal = p.LoanRepayment,
				Fees = p.Fees,
				Total = p.Amount
			};
		}

		public static ScheduleRowModel CreateFees(LoanCharge lc) {
			return new ScheduleRowModel {
				Status = string.IsNullOrEmpty(lc.State) ? "Active" : lc.State,
				Type = "Charge",
				Date = lc.Date,
				Interest = null,
				Principal = null,
				Fees = lc.Amount,
				Total = lc.Amount
			};
		}

		public static ScheduleRowModel CreateSchedule(LoanScheduleItem ls) {
			return new ScheduleRowModel {
				Status = ls.Status.DescriptionAttr(),
				Type = "Schedule",
				Date = ls.Date,
				Interest = ls.Interest,
				Principal = ls.LoanRepayment,
				Fees = ls.Fees,
				Total = ls.AmountDue
			};
		}
	}//class ScheduleRowModel
} // namespace
