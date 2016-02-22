namespace ExtractDataForLsa {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Microsoft.VisualBasic.FileIO;

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

			this.cfg = CustomConfiguration.Load();

			var env = new Ezbob.Context.Environment(this.cfg.Environment.Name, this.cfg.Environment.Variant, this.log);

			this.log.Msg("Target path: {0}", TargetPath);
			this.log.Msg("Dropbox root path: {0}", DropboxRootPath);
			this.log.Msg("Working mode: {0}", this.workingMode);

			if (this.workingMode == WorkingMode.None)
				return;

			var db = new SqlConnection(env, this.log);

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
		} // constructor

		private void Run() {
			if (this.workingMode == WorkingMode.None) {
				this.log.Msg("Working mode is {0} - nothing to do.", this.workingMode);
				return;
			} // if

			ProcessCustomerData();
			ProcessExperianData();
			ProcessAgreements();
			ProcessEchoSign();
			ProcessEmails();
			ProcessSms();
			ProcessSnailmails();
			ProcessDropbox();

			this.log.Debug("Processing complete.");
		} // Run

		private bool Do(WorkingMode mode) {
			bool result;

			if (mode == WorkingMode.Dropbox)
				result = (this.workingMode == WorkingMode.All) || (this.workingMode == mode);
			else {
				result =
					(this.workingMode == WorkingMode.All) ||
					(this.workingMode == WorkingMode.ExceptDropbox) ||
					(this.workingMode == mode);
			} // if

			this.log.Msg("Step {0}: {1}processing.", mode, result ? string.Empty : "not ");

			return result;
		} // Do

		private void ProcessDropbox() {
			if (!Do(WorkingMode.Dropbox))
				return;

			foreach (KeyValuePair<string, SortedSet<string>> pair in this.nameToLoan) {
				string customerName = pair.Key;
				SortedSet<string> loanList = pair.Value;

				string sourcePath = Path.Combine(DropboxRootPath, customerName);

				if (!Directory.Exists(sourcePath)) {
					this.log.Warn("Customer directory not found in the dropbox: {0}", customerName);
					continue;
				} // if

				foreach (string loanID in loanList) {
					string loanPath = Path.Combine(TargetPath, loanID, "uploaded-files");

					if (!Directory.Exists(loanPath))
						Directory.CreateDirectory(loanPath);

					this.log.Debug(
						"For loan #{0} copying dropbox directory {1} to {2}...",
						loanID,
						sourcePath,
						loanPath
					);

					try {
						FileSystem.CopyDirectory(sourcePath, loanPath, true);
						this.log.Debug("Loan #{0} has attached directory {1}.", loanID, loanPath);
					} catch (IOException io) {
						var lst = new List<string>();

						foreach (DictionaryEntry kv in io.Data)
							lst.Add(string.Format("{0}: {1}", kv.Key, kv.Value));

						this.log.Alert(
							io,
							"For loan #{0} failed to dropbox directory {1} to {2}.\n" +
							"Exception was thrown of type {3} with message: {4}.\n" +
							"More details:\n\t{5}",
							loanID,
							sourcePath,
							loanPath,
							io.GetType().FullName,
							io.Message,
							string.Join("\n\t", lst)
						);
					} catch (Exception e) {
						this.log.Alert(
							e,
							"For loan #{0} failed to dropbox directory {1} to {2}.\n" +
							"Exception was thrown of type {3} with message: {4}",
							loanID,
							sourcePath,
							loanPath,
							e.GetType().FullName,
							e.Message
						);
					} // try
				} // for each loan
			} // for each customer
		} // ProcessDropbox

		private void ProcessSnailmails() {
			if (!Do(WorkingMode.SnailMail))
				return;

			this.loansForLsaSnailmails.ForEachRowSafe(sr => {
				string loanID = sr["LoanID"];
				string path = sr["Path"];

				if (path == null)
					path = string.Empty;

				string loanPath = Path.Combine(TargetPath, loanID, "snail-mails");

				if (!Directory.Exists(loanPath)) {
					Directory.CreateDirectory(loanPath);
					this.log.Debug("Created snail mail path for loan {0}: {1}", loanID, loanPath);
				} else
					this.log.Debug("Already exists snail mail path for loan {0}: {1}", loanID, loanPath);

				string fileName = Path.GetFileName(path);

				if (File.Exists(path)) {
					File.Copy(path, Path.Combine(loanPath, fileName), true);
					this.log.Debug("Loan #{0} has agreement {1}.", loanID, fileName);
				} // if
			});
		} // ProcessSnailmails

		private void ProcessSms() {
			if (!Do(WorkingMode.Sms))
				return;

			this.loansForLsaSms.ForEachResult<SmsData>(ed => {
				ed.SaveTo(TargetPath);
				this.log.Debug("Loan #{0} has SMS '{1}'.", ed.LoanID, ed.Body);
				return ActionResult.Continue;
			});
		} // ProcessSms

		private void ProcessEmails() {
			if (!Do(WorkingMode.Emails))
				return;

			this.loansForLsaEmails.ForEachResult<EmailData>(ed => {
				ed.SaveTo(TargetPath);
				this.log.Debug("Loan #{0} has email file {1}.", ed.LoanID, ed.FileName);
				return ActionResult.Continue;
			});
		} // ProcessEmails

		private void ProcessEchoSign() {
			if (!Do(WorkingMode.EchoSign))
				return;

			this.loansForLsaEchoSign.ForEachResult<EchoSignData>(esd => {
				esd.SaveTo(TargetPath);

				this.log.Debug("Loan #{0} has EchoSign file {1}.", esd.LoanID, esd.FileNameBase);

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

					string fileName = Path.GetFileName(fullSourcePath);

					File.Copy(fullSourcePath, Path.Combine(loanPath, fileName), true);

					this.log.Debug("Loan #{0} has agreement {1}.", loanID, fileName);

					break;
				} // for each base path
			});
		} // ProcessAgreements

		private void ProcessExperianData() {
			if (!Do(WorkingMode.Experian))
				return;

			this.loansForLsaExperian.ForEachResult<ExperianData>(cd => {
				cd.SaveTo(TargetPath);
				this.log.Debug("Loan #{0} has Experian request {1} at {2}.", cd.LoanID, cd.ServiceType, cd.FetchTime);
				return ActionResult.Continue;
			});
		} // ProcessExperianData

		private void ProcessCustomerData() {
			int loanCount = 0;

			this.loansForLsa.ForEachResult<CustomerData>(cd => {
				if (!this.loans.ContainsKey(cd.LoanID))
					this.loans[cd.LoanID] = new LoanData(cd.LoanID, cd.LoanInternalID);

				this.loans[cd.LoanID].CustomerData = cd;

				if (!this.nameToLoan.ContainsKey(cd.FullName))
					this.nameToLoan[cd.FullName] = new SortedSet<string>();

				this.nameToLoan[cd.FullName].Add(cd.LoanID);

				this.log.Debug("{0} has loan #{1}.", cd.FullName, cd.LoanID);

				loanCount++;

				return ActionResult.Continue;
			});

			this.log.Debug("{0} loans loaded.", loanCount);

			this.loansForLsaDirectors.ForEachResult<DirectorData>(cd => {
				if (!this.loans.ContainsKey(cd.LoanID)) {
					this.loans[cd.LoanID] = new LoanData(cd.LoanID, cd.LoanInternalID);
					this.log.Alert("Loan {0} not found for director.", cd.LoanID);
				} // if

				this.loans[cd.LoanID].Directors.Add(cd);

				this.log.Debug("Loan #{0} has director {1}.", cd.LoanID, cd.FullName);

				return ActionResult.Continue;
			});

			this.loansForLsaCrm.ForEachResult<CRMData>(cd => {
				if (!this.loans.ContainsKey(cd.LoanID)) {
					this.loans[cd.LoanID] = new LoanData(cd.LoanID, cd.LoanInternalID);
					this.log.Alert("Loan {0} not found for CRM.", cd.LoanID);
				} // if

				this.loans[cd.LoanID].Crm.Add(cd);

				this.log.Debug("Loan #{0} has CRM message {1}.", cd.LoanID, cd.Action);

				return ActionResult.Continue;
			});

			if (Do(WorkingMode.Customer))
				foreach (LoanData ld in this.loans.Values)
					ld.SaveTo(TargetPath, this.log);

			File.WriteAllLines(
				Path.Combine(TargetPath, "loans.csv"),
				this.loans.Values.Select(ld => string.Format("{0},{1}", ld.LoanID, ld.LoanInternalID)),
				System.Text.Encoding.ASCII
			);
		} // ProcessCustomerData

		private string TargetPath { get { return this.cfg.TargetPath; } }
		private string DropboxRootPath { get { return this.cfg.DropboxRootPath; } }

		private readonly CustomConfiguration cfg;

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
	} // class Program
} // namespace

