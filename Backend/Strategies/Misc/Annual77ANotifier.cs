namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using IMailLib;

	public class Annual77ANotifier : AStrategy {

		public Annual77ANotifier() {
			this.now = DateTime.UtcNow;
			this.collectionIMailer = new CollectionMail(
			   ConfigManager.CurrentValues.Instance.ImailUserName,
			   ConfigManager.CurrentValues.Instance.IMailPassword,
			   ConfigManager.CurrentValues.Instance.IMailDebugModeEnabled,
			   ConfigManager.CurrentValues.Instance.IMailDebugModeEmail,
			   ConfigManager.CurrentValues.Instance.IMailSavePath);
		}

		public override string Name { get { return "Annual77ANotifier"; } } // Name

		public override void Execute() {
			LoadImailTemplates();
			var template = this.templates.FirstOrDefault(x => x.IsActive && x.OriginID == 1);
			if (template == null) {
				Log.Debug("No template found for annual 77A notification for originID {2} to customer {0} for loan {1}", 1, 1, 1);
				return;
			}
			PrepareAndSendMail(27, 54, template);
			//DB.ForEachRowSafe(HandleOneNotification, "LoadLoansForAnnual77ANofication", CommandSpecies.StoredProcedure, new QueryParameter("Now", this.now));
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
			var variables = PrepareVariables(loanID, customerID, out address);
			List<string> header = new List<string>{
				{"Date"},
				{"Amount"},
				{"Description"}
			};

			List<List<string>> content = new List<List<string>> {
				new List<string> { 
					{"2015-1-1"},
					{"100"},
					{"Payment"}
				},
				new List<string> {
					{"2015-2-1"},
					{"200"},
					{"Payment"}
				}
			};

			var schedule = new TableModel {
				Header = header,
				Content = content
			};

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

		private Dictionary<string, string> PrepareVariables(int loanID, int customerID, out Address address) {

			//TODO

			address = new Address {
				Line1 = "l1",
				Line2 = "l2",
				Line3 = "l3",
				Line4 = "l4",
				Postcode = "AB10 1BA"
			};
		
			return new Dictionary<string, string>{
				{"CustomerName","John Doe"},
				{"LoanRefNum","054656465465"},
				{"Date",DateTime.UtcNow.ToString("yyyy-MM-dd")},
				{"LoanDate",DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd")},
				{"LoanAmount","10000"},
				{"AnnualInterestRatePercent","15"},
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
	} // class Anual77ANotifier
} // namespace
