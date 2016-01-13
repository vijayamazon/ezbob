namespace ExtractDataForLsa {
	using System.Collections.Generic;
	using System.IO;
	using Ezbob.Context;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Microsoft.VisualBasic.FileIO;
	using Newtonsoft.Json;

	class Program {
		static void Main(string[] args) {
			new Program().Run();
		} // Main

		private Program() {
			this.log = new FileLog("ExtractDataForLsa");
			var env = new Ezbob.Context.Environment(Name.Production, "alexbo", this.log);
			this.db = new SqlConnection(env, log);

			this.loans = new SortedDictionary<string, LoanData>();
			this.nameToLoan = new SortedDictionary<string, SortedSet<string>>();

			this.loansForLsa = new RptLoansForLsa(this.db, this.log);
			this.loansForLsaDirectors = new RptLoansForLsaDirectors(this.db, this.log);
			this.loansForLsaExperian = new RptLoansForLsaExperian(this.db, this.log);
			this.loansForLsaAgreements = new RptLoansForLsaAgreements(this.db, this.log);
			this.loansForLsaAgreementsBasePaths = new RptLoansForLsaAgreementsBasePaths(this.db, this.log);
			this.loansForLsaEchoSign = new RptLoansForLsaEchoSign(this.db, this.log);
			this.loansForLsaCrm = new RptLoansForLsaCRM(this.db, this.log);
			this.loansForLsaEmails = new RptLoansForLsaEmails(this.db, this.log);
			this.loansForLsaSms = new RptLoansForLsaSms(this.db, this.log);
			this.loansForLsaSnailmails = new RptLoansForLsaSnailmails(this.db, this.log);
		} // constructor

		private void Run() {
			ProcessDropbox();
		} // Run

		private void ProcessDropbox() {
			var namesToLoans = JsonConvert.DeserializeObject<SortedDictionary<string, SortedSet<string>>>(
				File.ReadAllText("names-to-loans.json")
			);

			foreach (KeyValuePair<string, SortedSet<string>> pair in namesToLoans) {
				string customerName = pair.Key;
				SortedSet<string> loanList = pair.Value;

				string sourcePath = Path.Combine(DropboxRootPath, customerName);

				if (!Directory.Exists(sourcePath)) {
					this.log.Alert("Customer directory not found in the dropbox: {0}", customerName);
					continue;
				} // if

				foreach (string loanID in loanList) {
					string loanPath = Path.Combine(TargetPath, loanID, "uploaded-files");

					if (!Directory.Exists(loanPath))
						Directory.CreateDirectory(loanPath);

					FileSystem.CopyDirectory(sourcePath, loanPath, UIOption.AllDialogs);
				} // for each loan
			} // for each customer

		} // ProcessDropbox

		private void ProcessSnailmails() {
			this.loansForLsaSnailmails.ForEachRowSafe(sr => {
				string loanID = sr["LoanID"];
				string path = sr["Path"];

				string loanPath = Path.Combine(TargetPath, loanID, "snail-mails");

				if (!Directory.Exists(loanPath))
					Directory.CreateDirectory(loanPath);

				if (File.Exists(path))
					File.Copy(path, Path.Combine(loanPath, Path.GetFileName(path)), true);
			});
		} // ProcessSnailmails

		private void ProcessSms() {
			this.loansForLsaSms.ForEachResult<SmsData>(ed => {
				ed.SaveTo(TargetPath);
				return ActionResult.Continue;
			});
		} // ProcessSms

		private void ProcessEmails() {
			this.loansForLsaEmails.ForEachResult<EmailData>(ed => {
				ed.SaveTo(TargetPath);
				return ActionResult.Continue;
			});
		} // ProcessEmails

		private void ProcessEchoSign() {
			this.loansForLsaEchoSign.ForEachResult<EchoSignData>(esd => {
				esd.SaveTo(TargetPath);
				return ActionResult.Continue;
			});
		} // ProcessEchoSign

		private void ProcessAgreements() {
			var basePaths = new List<string>();

			this.loansForLsaAgreementsBasePaths.ForEachRowSafe(sr => {
				basePaths.Add(sr[0]);
			});

			this.loansForLsaAgreements.ForEachRowSafe(sr => {
				string loanID = sr["LoanID"];
				string filePath = sr["FilePath"];

				string loanPath = Path.Combine(TargetPath, loanID, "agreements");

				if (!Directory.Exists(loanPath))
					Directory.CreateDirectory(loanPath);

				foreach (string basePath in basePaths) {
					string fullSourcePath = Path.Combine(basePath, filePath);

					if (!File.Exists(fullSourcePath))
						continue;

					File.Copy(fullSourcePath, Path.Combine(loanPath, Path.GetFileName(fullSourcePath)), true);
					break;
				} // for each base path
			});
		} // ProcessAgreements

		private void ProcessExperianData() {
			this.loansForLsaExperian.ForEachResult<ExperianData>(cd => {
				cd.SaveTo(TargetPath);
				return ActionResult.Continue;
			});
		} // ProcessExperianData

		private void ProcessCustomerData() {
			this.loansForLsa.ForEachResult<CustomerData>(cd => {
				if (!this.loans.ContainsKey(cd.LoanID))
					this.loans[cd.LoanID] = new LoanData(cd.LoanID);

				this.loans[cd.LoanID].CustomerData = cd;

				if (!this.nameToLoan.ContainsKey(cd.FullName))
					this.nameToLoan[cd.FullName] = new SortedSet<string>();

				this.nameToLoan[cd.FullName].Add(cd.LoanID);

				return ActionResult.Continue;
			});

			this.loansForLsaDirectors.ForEachResult<DirectorData>(cd => {
				if (!this.loans.ContainsKey(cd.LoanID)) {
					this.loans[cd.LoanID] = new LoanData(cd.LoanID);
					this.log.Alert("Loan {0} not found for director.", cd.LoanID);
				} // if

				this.loans[cd.LoanID].Directors.Add(cd);

				return ActionResult.Continue;
			});

			this.loansForLsaCrm.ForEachResult<CRMData>(cd => {
				if (!this.loans.ContainsKey(cd.LoanID)) {
					this.loans[cd.LoanID] = new LoanData(cd.LoanID);
					this.log.Alert("Loan {0} not found for CRM.", cd.LoanID);
				} // if

				this.loans[cd.LoanID].Crm.Add(cd);

				return ActionResult.Continue;
			});

			foreach (LoanData ld in this.loans.Values)
				ld.SaveTo(TargetPath);

			File.WriteAllText(
				Path.Combine(TargetPath, "names-to-loans.json"),
				JsonConvert.SerializeObject(this.nameToLoan),
				System.Text.Encoding.UTF8
			);
		} // ProcessCustomerData

		private readonly ASafeLog log;
		private readonly AConnection db;

		private readonly RptLoansForLsa loansForLsa;
		private readonly RptLoansForLsaDirectors loansForLsaDirectors;
		private readonly RptLoansForLsaExperian loansForLsaExperian;
		private readonly RptLoansForLsaAgreements loansForLsaAgreements;
		private readonly RptLoansForLsaAgreementsBasePaths loansForLsaAgreementsBasePaths;
		private readonly RptLoansForLsaEchoSign loansForLsaEchoSign;
		private readonly RptLoansForLsaCRM loansForLsaCrm;
		private readonly RptLoansForLsaEmails loansForLsaEmails;
		private readonly RptLoansForLsaSms loansForLsaSms;
		private readonly RptLoansForLsaSnailmails loansForLsaSnailmails;

		private readonly SortedDictionary<string, SortedSet<string>> nameToLoan;

		private readonly SortedDictionary<string, LoanData> loans; 

		private const string TargetPath = "c:\\temp\\_lsa";
		private const string DropboxRootPath = @"d:\Dropbox (Orange Money Ltd)\Underwriters\2_Clients Documents";
	} // class Program
} // namespace

