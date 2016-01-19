namespace Ezbob.Backend.Strategies.Tasks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
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

		public void ExecuteTest(int customerID, int loanID, int originID, DateTime? lastSent = null) {
			LoadImailTemplates();
			var template = this.templates.FirstOrDefault(x => x.IsActive && x.OriginID == originID);
			if (template == null) {
				Log.Debug("No template found for annual 77A notification for originID {2} to customer {0} for loan {1}", customerID, loanID, originID);
				return;
			}
			PrepareAndSendMail(loanID, customerID, template, lastSent);
		} // Execute

		private ActionResult HandleOneNotification(SafeReader sr, bool bRowSetStart) {
			int loanID = sr["LoanID"];
			int customerID = sr["CustomerID"];
			DateTime? lastSent = sr["LastSent"];
			int originID = sr["OriginID"];

			if (lastSent.HasValue && lastSent.Value.AddYears(1) > this.now) {
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

			if (!typeOfBusiness.IsRegulated()) {
				Log.Debug("Not sending annual 77A notification to customer {0} with regulated business type {1}", customerID, typeOfBusiness);
				return ActionResult.Continue;
			}

			PrepareAndSendMail(loanID, customerID, template, lastSent);
			return ActionResult.Continue;
		}//HandleOneNotification

		private void PrepareAndSendMail(int loanID, int customerID, SnailMailTemplate template, DateTime? lastSent){
			Address address;
			TableModel schedule;
			var variables = PrepareVariables(loanID, customerID, lastSent, out address, out schedule);

			var metadata = this.collectionIMailer.SendAnual77ANotification(customerID, template, address, variables, schedule, "@ScheduleTable@");
			var collectionLogID = AddCollectionLog(customerID, loanID, CollectionType.Annual77ANotification, CollectionMethod.Mail);
			SaveCollectionSnailMailMetadata(collectionLogID, metadata);
		}
		private void LoadImailTemplates() {
			List<CollectionSnailMailTemplate> dbTemplates = DB.Fill<CollectionSnailMailTemplate>("LoadCollectionSnailMailTemplates", CommandSpecies.StoredProcedure);
			this.templates = dbTemplates
				.Where(x => x.Type == CollectionType.Annual77ANotification.ToString())
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

		private Dictionary<string, string> PrepareVariables(int loanID, int customerID, DateTime? lastSent, out Address address, out TableModel scheduleModel) {
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

			var monthlyInterestRate = loan.InterestRate * 100;
			var interestRate = monthlyInterestRate * 12;
			var repaymentPeriod = loan.CustomerSelectedTerm ?? loan.CashRequest.RepaymentPeriod;

			scheduleModel = new TableModel();

			var pacnet = loan.PacnetTransactions
				.Select(ScheduleRowModel.CreatePacnet)
				.ToList();
			var paypoint = loan.TransactionsWithPaypointSuccesefull
				.Select(ScheduleRowModel.CreatePaypoint)
				.ToList();
			//var fees = loan.Charges.Where(x => x.State != "Paid" && x.State != "Expired")
			//	.Select(ScheduleRowModel.CreateFees)
			//	.ToList();
			var schedule = loan.Schedule
				.Where(x => x.Status == LoanScheduleStatus.AlmostPaid || x.Status == LoanScheduleStatus.Late || x.Status == LoanScheduleStatus.StillToPay)
				.Select(ScheduleRowModel.CreateSchedule)
				.ToList();

			var allRows = pacnet;
			allRows.AddRange(paypoint);
			//allRows.AddRange(fees);
			allRows.AddRange(schedule);

			DateTime startPeriod = loan.Date;
			if (lastSent.HasValue) {
				startPeriod = lastSent.Value;
			}

			DateTime endPeriod = startPeriod.AddYears(1);

			scheduleModel.Content = CalculateBalance(allRows, startPeriod, endPeriod);

			scheduleModel.Header = new List<string>{
				{"Date"},
				{"Description"},
				{"Payment received"},
				{"Interest and fees"},
				{"Balance"}
			};
			
			return new Dictionary<string, string> {
				{"CustomerName",customerPersonalInfo.Fullname},
				{"LoanRefNum",loan.RefNumber},
				{"Date",this.now.ToLongUKDate()},
				{"StartPeriod",startPeriod.ToLongUKDate()},
				{"EndPeriod",endPeriod.ToLongUKDate()},
				{"LoanDate",loan.Date.ToLongUKDate()},
				{"LoanAmount",loan.LoanAmount.ToNumeric2Decimals()},
				{"AnnualInterestRatePercent", interestRate.ToNumeric2Decimals()},
				{"MonthlyInterestRatePercent", monthlyInterestRate.ToNumeric2Decimals()},
				{"LoanTermMonths", repaymentPeriod.ToString()},
				{"ScheduleTable", ""}
			};
		}//PrepareVariables

		private List<List<string>> CalculateBalance(List<List<ScheduleRowModel>> allRows, DateTime startPeriod, DateTime endPeriod) {
			decimal balance = 0;
			foreach (var typeRows in allRows) {
				foreach (var row in typeRows) {
					if (row.Date > endPeriod) {
						break;
					}
					row.Balance = row.Balance + balance + (row.InterestAndFees ?? 0) - (row.PaymentReceived ?? 0);
					balance = row.Balance;
				}
			}

			allRows.Add(new List<ScheduleRowModel> {
				ScheduleRowModel.CreateClosingBalance(endPeriod, balance)
			});

			var allRowsInPeriod = allRows.SelectMany(x => x.ToArray())
				.Where(x => x.Date >= startPeriod && x.Date <= endPeriod).ToList();

			var first = allRowsInPeriod.FirstOrDefault();
			if (first != null) {
				if (first.Description != "Opening balance") {
					ScheduleRowModel openingBalanceRow = new ScheduleRowModel {
						Balance = first.Balance - (first.InterestAndFees ?? 0) + (first.PaymentReceived ?? 0),
						Description = "Opening balance",
						Date = startPeriod
					};
					allRowsInPeriod.Insert(0, openingBalanceRow);
				}
			}

			var content = allRowsInPeriod
				.Select(x => new List<string> {
					{x.Date.ToLongUKDate()},
					{x.Description},
					{x.PaymentReceived.ToNumeric2Decimals(true)},
					{x.InterestAndFees.ToNumeric2Decimals(true)},
					{x.Balance.ToNumeric2Decimals(true)}
				})
				.ToList();

			return content;
		}//CalculateBalance

		private int AddCollectionLog(int customerID, int loanID, CollectionType type, CollectionMethod method) {
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
		public DateTime Date { get; set; }
		public string Description { get; set; }
		public decimal? PaymentReceived { get; set; }
		public decimal? InterestAndFees { get; set; }
		public decimal Balance { get; set; }
		
		public static List<ScheduleRowModel> CreatePacnet(PacnetTransaction p) {
			return new List<ScheduleRowModel> {
				new ScheduleRowModel {
					Description = "Opening balance",
					Date = p.PostDate,
					PaymentReceived = null,
					InterestAndFees = null,
					Balance = p.Amount
				}
			};
		}//CreatePacnet

		public static List<ScheduleRowModel> CreatePaypoint(PaypointTransaction p) {
			var list = new List<ScheduleRowModel>();
			if (p.Fees > 0) {
				list.Add(new ScheduleRowModel {
					Description = "Fee",
					Date = p.PostDate,
					PaymentReceived = null,
					InterestAndFees = p.Fees,
				});
			}

			if (p.Interest > 0) {
				list.Add(new ScheduleRowModel {
					Description = "Interest",
					Date = p.PostDate,
					PaymentReceived = null,
					InterestAndFees = p.Interest,
				});
			}

			if (p.Amount > 0) {
				list.Add(new ScheduleRowModel {
					Description = "Repayment",
					Date = p.PostDate,
					PaymentReceived = p.Amount,
					InterestAndFees = null,
				});
			}

			return list;
		}//CreatePaypoint

		public static List<ScheduleRowModel> CreateSchedule(LoanScheduleItem ls) {
			var list = new List<ScheduleRowModel>();
			if (ls.Fees > 0) {
				list.Add(new ScheduleRowModel {
					Description = "Fee",
					Date = ls.Date,
					PaymentReceived = null,
					InterestAndFees = ls.Fees,
				});
			}

			if (ls.Interest > 0) {
				list.Add(new ScheduleRowModel {
					Description = "Interest",
					Date = ls.Date,
					PaymentReceived = null,
					InterestAndFees = ls.Interest,
				});
			}

			return list;
		}//CreateSchedule

		public static ScheduleRowModel CreateClosingBalance(DateTime date, decimal balance) {
			return new ScheduleRowModel {
				Description = "Closing balance",
				Date = date,
				PaymentReceived = null,
				InterestAndFees = null,
				Balance = balance
			};
		}//CreateClosingBalance
	}//class ScheduleRowModel
} // namespace
