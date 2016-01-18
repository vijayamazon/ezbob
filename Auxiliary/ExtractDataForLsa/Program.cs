namespace ExtractDataForLsa {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Ezbob.Context;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Microsoft.VisualBasic.FileIO;
	using Newtonsoft.Json;

	class Program {
		static void Main(string[] args) {
			new Program(args).Run();
		} // Main

		private enum WorkingMode {
			None,
			All,
			ExceptDropbox,
			Customer,
			Experian,
			Agreements,
			EchoSign,
			Emails,
			Sms,
			SnailMail,
			Dropbox,
		} // enum WorkingMode

		private Program(string[] args) {
			this.workingMode = WorkingMode.None;

			if ((args != null) && (args.Length > 0))
				Enum.TryParse(args[0], true, out this.workingMode);

			this.log = new FileLog("ExtractDataForLsa");
			var env = new Ezbob.Context.Environment(Name.Production, "alexbo", this.log);
			var db = new SqlConnection(env, log);

			this.loans = new SortedDictionary<string, LoanData>();
			this.nameToLoan = new SortedDictionary<string, SortedSet<string>>();

			this.loansForLsa = new RptLoansForLsa(db, this.log);
			this.loansForLsaDirectors = new RptLoansForLsaDirectors(db, this.log);
			this.loansForLsaExperian = new RptLoansForLsaExperian(db, this.log);
			this.loansForLsaAgreements = new RptLoansForLsaAgreements(db, this.log);
			this.loansForLsaAgreementsBasePaths = new RptLoansForLsaAgreementsBasePaths(db, this.log);
			this.loansForLsaEchoSign = new RptLoansForLsaEchoSign(db, this.log);
			this.loansForLsaCrm = new RptLoansForLsaCRM(db, this.log);
			this.loansForLsaEmails = new RptLoansForLsaEmails(db, this.log);
			this.loansForLsaSms = new RptLoansForLsaSms(db, this.log);
			this.loansForLsaSnailmails = new RptLoansForLsaSnailmails(db, this.log);

			this.log.Msg("Target path: {0}", TargetPath);
			this.log.Msg("Dropbox root path: {0}", DropboxRootPath);
			this.log.Msg("Working mode: {0}", this.workingMode);
		} // constructor

		private void Run() {
			ProcessCustomerData();
			ProcessExperianData();
			ProcessAgreements();
			ProcessEchoSign();
			ProcessEmails();
			ProcessSms();
			ProcessSnailmails();
			ProcessDropbox();
		} // Run

		private bool Do(WorkingMode mode) {
			if (mode == WorkingMode.Dropbox)
				return (this.workingMode == WorkingMode.All) || (this.workingMode == mode);

			return
				(this.workingMode == WorkingMode.All) ||
				(this.workingMode == WorkingMode.ExceptDropbox) ||
				(this.workingMode == mode);
		} // Do

		private void ProcessDropbox() {
			if (!Do(WorkingMode.Dropbox))
				return;

			var namesToLoans = JsonConvert.DeserializeObject<SortedDictionary<string, SortedSet<string>>>(
				File.ReadAllText(NamesToLoansFileName)
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
			if (!Do(WorkingMode.SnailMail))
				return;

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
			if (!Do(WorkingMode.Sms))
				return;

			this.loansForLsaSms.ForEachResult<SmsData>(ed => {
				ed.SaveTo(TargetPath);
				return ActionResult.Continue;
			});
		} // ProcessSms

		private void ProcessEmails() {
			if (!Do(WorkingMode.Emails))
				return;

			this.loansForLsaEmails.ForEachResult<EmailData>(ed => {
				ed.SaveTo(TargetPath);
				return ActionResult.Continue;
			});
		} // ProcessEmails

		private void ProcessEchoSign() {
			if (!Do(WorkingMode.EchoSign))
				return;

			this.loansForLsaEchoSign.ForEachResult<EchoSignData>(esd => {
				esd.SaveTo(TargetPath);
				return ActionResult.Continue;
			});
		} // ProcessEchoSign

		private void ProcessAgreements() {
			if (!Do(WorkingMode.Agreements))
				return;

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
			if (!Do(WorkingMode.Experian))
				return;

			this.loansForLsaExperian.ForEachResult<ExperianData>(cd => {
				cd.SaveTo(TargetPath);
				return ActionResult.Continue;
			});
		} // ProcessExperianData

		private void ProcessCustomerData() {
			if (!Do(WorkingMode.Customer))
				return;

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
				Path.Combine(TargetPath, NamesToLoansFileName),
				JsonConvert.SerializeObject(this.nameToLoan),
				System.Text.Encoding.UTF8
			);
		} // ProcessCustomerData

		private readonly ASafeLog log;

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

		private readonly WorkingMode workingMode;

		private const string TargetPath = "c:\\temp\\_lsa";
		private const string DropboxRootPath = @"d:\Dropbox (Orange Money Ltd)\Underwriters\2_Clients Documents";
		private const string NamesToLoansFileName = "names-to-loans.json";
	} // class Program
} // namespace

