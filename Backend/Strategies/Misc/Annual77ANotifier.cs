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
			DB.ForEachRowSafe(HandleOneNotification, "LoadLoansForAnnual77ANofication", CommandSpecies.StoredProcedure, new QueryParameter("Now", this.now));
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

			var variables = PrepareVariables(loanID, customerID);
			var metadata = this.collectionIMailer.SendAnual77ANotification(customerID, template, new Address(), variables);
			var collectionLogID = AddCollectionLog(customerID, loanID, SetLateLoanStatus.CollectionType.Annual77ANotification, SetLateLoanStatus.CollectionMethod.Mail);
			SaveCollectionSnailMailMetadata(collectionLogID, metadata);
			return ActionResult.Continue;
		}//HandleOneNotification

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

		private Dictionary<string, string> PrepareVariables(int loanID, int customerID) {
			//TODO
			return new Dictionary<string, string>();
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
