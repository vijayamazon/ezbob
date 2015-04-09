namespace Ezbob.Dabinuto {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Data.SqlClient;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using Microsoft.SqlServer.Management.Common;
	using Microsoft.SqlServer.Management.Smo;

	class Program {
		static int Main(string[] args) {
			var app = new Program(args);

			if (app.exitCode == ExitCode.Success) {
				app.Run();
				app.Done();
			} // if

			return (int)app.exitCode;
		} // Main

		private Program(string[] args) {
			this.now = DateTime.UtcNow;
			this.exitCode = ExitCode.Success;

			this.force = false;
			this.folders = new List<string>();

			this.log = new ConsoleLog(new FileLog(AppName));
			this.log.NotifyStart();

			try {
				ParseCommandLine(args);

				if (this.exitCode != ExitCode.Success)
					return;

				FillMissingParameters();

				LoadFolders();

				if (this.exitCode != ExitCode.Success)
					return;

				if (this.folders.Count < 1) {
					this.log.Fatal("No source folders specified in app.config.");
					this.exitCode = ExitCode.NoFoldersSpecified;
					return;
				} // if

				this.environment = new Ezbob.Context.Environment(this.log);

				LoadConnectionString();

				if (this.exitCode != ExitCode.Success)
					return;

				this.fileTimes = new FileTimes(this.environment.Context, Path.Combine(this.basePath, LastRunTimesFileName));

				if (!this.force)
					this.fileTimes.Load();

				LogConfiguration();

				this.counter = new Counter();
			} catch (Exception e) {
				this.log.Fatal(e, "Failed to initialize.");
				this.exitCode = ExitCode.InitFailed;
			} // try
		} // constructor

		private void Run() {
			if (this.exitCode != ExitCode.Success)
				return;

			try {
				foreach (string path in this.folders) {
					ProcessDirectory(path);

					if (this.exitCode != ExitCode.Success)
						return;
				} // for each
			} catch (Exception e) {
				this.log.Fatal(e, "Failure during running.");
				this.exitCode = ExitCode.RunFailed;
			} // try

			try {
				this.fileTimes.Save();
			} catch (Exception e) {
				this.log.Fatal(e, "Failed to save file execution times.");
				this.exitCode = ExitCode.LastRunTimeSaveFailed;
			} // try
		} // Run

		private void ProcessDirectory(string path) {
			this.counter.Directories++;

			this.log.Debug("Entering '{0}'...", path);

			string[] files = Directory.GetFiles(path, "*.sql");
			Array.Sort(files);

			foreach (string filePath in files) {
				ProcessFile(filePath);

				if (this.exitCode != ExitCode.Success)
					return;
			} // for each file

			string[] directories = Directory.GetDirectories(path);
			Array.Sort(directories);

			foreach (string dirPath in directories) {
				ProcessDirectory(dirPath);

				if (this.exitCode != ExitCode.Success)
					return;
			} // for each file

			this.log.Debug("Leaving '{0}'.", path);
		} // ProcessDirectory

		private void ProcessFile(string filePath) {
			DateTime? lastRunTime = this.fileTimes[filePath];

			DateTime lastUpdateTime = File.GetLastWriteTimeUtc(filePath);

			bool wasChanged = (lastRunTime == null) || (lastUpdateTime >= lastRunTime.Value);

			this.log.Debug(
				"File '{0}':\n\tlast run time:    {1}\n\tlast update time: {2}\n\tnow:              {3}\n\tchanged: {4}.",
				filePath,
				lastRunTime.HasValue ? lastRunTime.Value.ToString(DateFormat, Culture) : "never",
				lastUpdateTime.ToString(DateFormat, Culture),
				this.now.ToString(DateFormat, Culture),
				wasChanged ? "yes" : "no"
			);

			if (!wasChanged) {
				this.counter.Files.Skipped++;
				this.log.Debug("Not changed - skipping file '{0}'.", filePath);
				return;
			} // if

			this.log.Debug("Executing file '{0}'...", filePath);

			try {
				string sql = File.ReadAllText(filePath, Encoding.UTF8);

				ExecuteSql(sql);
				
				this.fileTimes[filePath] = this.now;
				this.counter.Files.Executed++;
			} catch (Exception e) {
				this.log.Fatal(e, "Error in file '{0}'.", filePath);
				this.exitCode = ExitCode.ErrorInFile;
			} // try

			this.log.Debug("Done with file '{0}'.", filePath);
		} // ProcessFile

		private void ExecuteSql(string sql) {
			var svrConnection = new ServerConnection(this.sqlConnection);
			var server = new Server(svrConnection);
			server.ConnectionContext.ExecuteNonQuery(sql);
			svrConnection.Disconnect();
		} // ExecuteSql

		private void ParseCommandLine(IReadOnlyList<string> args) {
			if (args.Count < 1)
				return;

			for (int i = 0; i < args.Count; i++) {
				switch (args[i]) {
				case "--help":
					Usage();
					this.exitCode = ExitCode.HelpOnly;
					return;

				case "--force":
					this.force = true;
					break;

				case "--base":
					i++;

					if (i < args.Count)
						this.basePath = args[i];

					break;
				} // switch
			} // for i
		} // ParseCommandLine

		private void Done() {
			if (this.counter != null) {
				this.log.Info("{0} processed.", Grammar.Number(this.counter.Directories, "folder"));

				this.log.Info(
					"Files: {0} processed = {1} executed + {2} skipped.",
					this.counter.Files.Processed,
					this.counter.Files.Executed,
					this.counter.Files.Skipped
				);
			} // if

			if (this.exitCode == ExitCode.Success)
				this.log.Info("Normal shutdown, everything went smoothly.");
			else {
				this.log.Fatal(@"

           )                                 (           )           )  (
    (   ( /(              (       )       )  )\       ( /(    (   ( /(  )\ )      (  (
    )\  )\())  (      (   )(     (     ( /( ((_)  (   )\())  ))\  )\())(()/(  (   )\))(    (
 _ ((_)((_)\   )\ )   )\ (()\    )\  ' )(_)) _    )\ ((_)\  /((_)(_))/  ((_)) )\ ((_)()\   )\ )
| | | || |(_) _(_/(  ((_) ((_) _((_)) ((_)_ | |  ((_)| |(_)(_))( | |_   _| | ((_)_(()((_) _(_/(
| |_| || '_ \| ' \))/ _ \| '_|| '  \()/ _` || |  (_-<| ' \ | || ||  _|/ _` |/ _ \\ V  V /| ' \))
 \___/ |_.__/|_||_| \___/|_|  |_|_|_| \__,_||_|  /__/|_||_| \_,_| \__|\__,_|\___/ \_/\_/ |_||_|

Exit code '{0}', see details above.
", this.exitCode);
			}

			this.log.NotifyStop();
			this.log.Dispose();
		} // Done

		private void FillMissingParameters() {
			if (string.IsNullOrWhiteSpace(this.basePath) || !Directory.Exists(this.basePath)) {
				this.log.Debug("Path '{0}' is empty or not found, defaulting to executable location.", this.basePath);
				this.basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			} // if
		} // FillMissingParameters

		private void LoadFolders() {
			var section = (CfgSourceFoldersSection)ConfigurationManager.GetSection("SourceFolders");

			if (section == null)
				return;

			for (int i = 0; i < section.Folders.Count; i++) {
				string path = Path.Combine(this.basePath, section.Folders[i].Name);

				if (!Directory.Exists(path)) {
					this.log.Fatal("Requested folder not found: '{0}'.", path);
					this.exitCode = ExitCode.BadFolderSpecified;
					return;
				} // if

				this.folders.Add(path);
			} // for
		} // LoadFolders

		private void LoadConnectionString() {
			try {
				this.connectionString =
					ConfigurationManager.ConnectionStrings[this.environment.Context.ToLower()].ConnectionString;

				this.sqlConnection = new SqlConnection(this.connectionString);
			} catch (Exception e) {
				this.log.Fatal(e,
					"Failed to load connection string from configuration file using name {0}",
					this.environment.Context.ToLower()
				);

				this.exitCode = ExitCode.FailedToLoadConnectionString;
			} // try	
		} // LoadConnectionString

		private void LogConfiguration() {
			this.log.Debug("Base directory: {0}", this.basePath);
			this.log.Debug("Connection string: {0}", this.connectionString);
			this.log.Debug("Working mode: execute {0}.", this.force ? "all the scripts" : "delta since last time only");
			this.log.Debug("Source folders: {0}.", string.Join(", ", this.folders));
		} // LogConfiguration

		private enum ExitCode {
			Success = 0,
			HelpOnly = 1,
			NoFoldersSpecified = 2,
			BadFolderSpecified = 3,
			LastRunTimeSaveFailed = 4,
			InitFailed = 5,
			RunFailed = 6,
			ErrorInFile = 7,
			FailedToLoadConnectionString = 8,
		} // enum ExitCode

		private void Usage() {
			this.log.Info(@"

{0} [--help] [--force] [--base <base path>]

--help:             Show this note and exit.
--force:            Ignore last run time and re-run all the scripts.
--base <base path>: Use specified path as parent of directories listed in app.config (usually it is c:\ezbob\sql).
                    Default value: this executable location.

", AppName);
		} // Usage

		private readonly string AppName = typeof(Program).Assembly.GetName().Name;
		private readonly ASafeLog log;
		private readonly List<string> folders;
		private readonly DateTime now;
		private readonly FileTimes fileTimes;
		private readonly Counter counter;
		private readonly Ezbob.Context.Environment environment;

		private const string LastRunTimesFileName = "last.run.times.json";
		private const string DateFormat = "MMM dd yyyy HH:mm:ss";
		private readonly CultureInfo Culture = new CultureInfo("en-GB", false);

		private string connectionString;
		private SqlConnection sqlConnection;
		private string basePath;
		private ExitCode exitCode;
		private bool force;
	} // class Program
} // namespace
