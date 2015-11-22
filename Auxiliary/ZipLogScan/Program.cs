namespace ZipLogScan {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;

	class Program {
		static void Main(string[] args) {
			var app = new Program(args);

			if (app.Init())
				app.Run();

			app.Done();
		} // Main

		private Program(string[] args) {
			this.requests = new List<Request>();
			this.monthlyResults = new SortedTable<string, RequestTypes, int>();
			this.dailyResults = new SortedTable<string, RequestTypes, int>();
			this.monthList = new SortedSet<int>();

			this.log = new FileLog("ziplogscan", true);

			this.log.Info("Logging started.");

			this.args = new Queue<string>((args ?? new string[0]).Where(s => !string.IsNullOrWhiteSpace(s)));

			this.log.Info(
				"Command line arguments (count = {0}): {1}",
				this.args.Count,
				this.args.Count == 0 ? "-- none --" : string.Join(" ", this.args)
			);
		} // constructor

		private bool Init() {
			if (this.args.Count < 1) {
				this.log.Fatal("No command line arguments specified.");
				Usage();
				return false;
			} // if

			this.sourcePath = this.args.Dequeue();

			if (!Directory.Exists(this.sourcePath)) {
				this.log.Fatal("Nothing to do: source path '{0}' not found.", this.sourcePath);
				return false;
			} // if

			if (this.args.Count < 1) {
				this.log.Fatal("No enough command line arguments specified.");
				Usage();
				return false;
			} // if

			if (!int.TryParse(this.args.Dequeue(), out this.year)) {
				this.log.Fatal("Could not parse <year> argument.");
				Usage();
				return false;
			} // if

			if (this.year < 2012) {
				this.log.Fatal("<year> should be at least 2012.");
				Usage();
				return false;
			} // if

			if (this.args.Count < 1) {
				this.log.Fatal("No enough command line arguments specified.");
				Usage();
				return false;
			} // if

			while (this.args.Count > 0) {
				int mon;
					if (!int.TryParse(this.args.Dequeue(), out mon))
						continue;

				if ((mon < 1) || (mon > 12))
					continue;

				this.monthList.Add(mon);
			} // while

			if (this.monthList.Count < 1) {
				this.log.Fatal("No months specified.");
				Usage();
				return false;
			} // if

			return true;
		} // Init

		private void Usage() {
			this.log.Info("Usage: ziplogscan <path to scan> <year> <month> [<month> ...]");
			this.log.Info("Year must be at least 2012.");
			this.log.Info("Month must be between 1 and 12.");
			this.log.Info("At least one month must be specified.");
		} // Usage

		private void Done() {
			this.log.Info("Logging stopped.");
		} // Done

		private void Run() {
			this.log.Debug("Scan started for path '{0}'...", this.sourcePath);

			foreach (int mon in this.monthList) {
				this.month = mon;
				ScanMonth(this.sourcePath);
				FillZeros();
			} // for each

			foreach (Request rq in this.requests) {
				this.monthlyResults[rq.Month, rq.RequestType]++;
				this.dailyResults[rq.Day, rq.RequestType]++;
			} // for each

			this.log.Msg(
				"Monthly results are:\n\n{0}\n\n",
				this.monthlyResults.ToFormattedString("Experian requests per months", bSkipSeparationLine: true)
			);

			this.log.Msg(
				"Daily results are:\n\n{0}\n\n",
				this.dailyResults.ToFormattedString("Experian requests per days", bSkipSeparationLine: true)
			);

			this.log.Debug("Scan complete for path '{0}'.", this.sourcePath);
		} // Run

		private void FillZeros() {
			string monthStr = string.Format("{0}-{1}", this.year.ToString("0000"), this.month.ToString("00"));

			foreach (RequestTypes reqType in (RequestTypes[])Enum.GetValues(typeof(RequestTypes))) {
				this.monthlyResults[monthStr, reqType] = 0;

				for (int i = 1; i <= DateTime.DaysInMonth(this.year, this.month); i++)
					this.dailyResults[monthStr + "-" + i.ToString("00"), reqType] = 0;
			} // for each request type

		} // FillZeros

		private void ScanMonth(string sourceDir) {
			this.log.Debug("Scanning month {0}/{1} in directory {2}...", this.month, this.year, sourceDir);

			this.fsmState = FsmStates.Start;

			for (int day = 1; day <= DateTime.DaysInMonth(this.year, this.month); day++) {
				ScanDay(sourceDir, ZippedFileNamePattern, day);
				ScanDay(sourceDir, UnzippedFileNamePattern, day);
			} // for

			string[] subDirs = Directory.GetDirectories(sourceDir);

			foreach (string subDir in subDirs) {
				if ((subDir == ".") || (subDir == ".."))
					continue;

				ScanMonth(subDir);
			} // for each subDir

			this.log.Debug("Scanning month {0}/{1} in directory {2} complete.", this.month, this.year, sourceDir);
		} // ScanMonth

		private void ScanDay(string sourceDir, string pattern, int day) {
			string mask = string.Format(
				pattern,
				this.year.ToString("0000"),
				this.month.ToString("00"),
				day.ToString("00")
			);

			this.log.Debug("Scanning day {0}/{1}/{2} using mask '{3}'...", day, this.month, this.year, sourceDir);

			string[] files = Directory.GetFiles(sourceDir, mask);

			this.log.Info("{0} found by mask {1}.", Grammar.Number(files.Length, "file"), mask);

			if (files.Length > 0) {
				var sortedFileNames = new SortedDictionary<int, string>();
				string lastFileName = null;

				foreach (string fileName in files) {
					Match match = fileNameRe.Match(fileName);

					if (string.IsNullOrEmpty(match.Groups[1].Value))
						lastFileName = fileName;
					else {
						int idx = int.Parse(match.Groups[1].Value.Substring(1));
						sortedFileNames[idx] = fileName;
					} // if
				} // for each

				DateTime time = new DateTime(this.year, this.month, day, 0, 0, 0, DateTimeKind.Utc);

				foreach (KeyValuePair<int, string> pair in sortedFileNames.Reverse())
					ProcessFile(pair.Value, time);

				ProcessFile(lastFileName, time);
			} // if

			this.log.Debug(
				"Scanning day {0}/{1}/{2} using mask '{3}' complete.",
				day,
				this.month,
				this.year,
				sourceDir
			);
		} // ScanDay

		private void ProcessFile(string fileName, DateTime time) {
			if (fileName.ToLowerInvariant().EndsWith(".zip"))
				ExtractAndProcess(fileName, time);
			else
				ProcessFileContent(new StreamReader(fileName), time);
		} // ProcessFile

		private void ExtractAndProcess(string fileName, DateTime time) {
			this.log.Debug("Processing zip file '{0}'...", fileName);

			ZipArchive archive = ZipFile.OpenRead(fileName);

			foreach (ZipArchiveEntry entry in archive.Entries)
				ProcessFileContent(new StreamReader(entry.Open()), time);

			this.log.Debug("Processing zip file '{0}' complete.", fileName);
		} // ExtractAndProcess

		private void ProcessFileContent(StreamReader rdr, DateTime time) {
			var pc = new ProgressCounter("{0} lines processed.", this.log, 25000UL);

			for ( ; ; ) {
				string line = rdr.ReadLine();

				if (line == null)
					break;

				ProcessLine(line, time);
				pc.Next();
			} // for

			pc.Log();

			rdr.Dispose();
		} // ProcessFileContent

		private void ProcessLine(string line, DateTime time) {
			switch (this.fsmState) {
			case FsmStates.Start:
				if (line.IndexOf(ConsumerNeedle, StringComparison.InvariantCulture) > -1) {
					this.requests.Add(new Request(RequestTypes.Consumer, time, this.log));
					break;
				} // if

				if (line.IndexOf(AmlNeedle, StringComparison.InvariantCulture) > -1) {
					this.requests.Add(new Request(RequestTypes.Aml, time, this.log));
					break;
				} // if

				int requestPos = line.IndexOf(RequestNeedle, StringComparison.InvariantCulture);

				if (requestPos < 0)
					break;

				int urlPos = line.IndexOf(ToUrlNeedle, requestPos + RequestNeedle.Length, StringComparison.InvariantCulture);

				if (urlPos < 0)
					break;

				this.fsmState = FsmStates.ProcessingRequestData;

				break;

			case FsmStates.ProcessingRequestData:
				int formIDPos = line.IndexOf(FormIDNeedle, StringComparison.InvariantCulture);

				if (formIDPos > -1) {
					this.requests.Add(new Request(line[formIDPos + FormIDNeedle.Length], time, this.log));
					this.fsmState = FsmStates.Start;
				} // if

				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessLine

		private FsmStates fsmState;

		private readonly ASafeLog log;

		private readonly List<Request> requests;

		private readonly Queue<string> args;
		private readonly SortedTable<string, RequestTypes, int> monthlyResults; 
		private readonly SortedTable<string, RequestTypes, int> dailyResults; 

		private string sourcePath;
		private int year;
		private int month;
		private readonly SortedSet<int> monthList; 

		private const string ZippedFileNamePattern = "EzService.log{0}-{1}-{2}*.zip";
		private const string UnzippedFileNamePattern = "EzService.log{0}-{1}-{2}*";
		private static readonly Regex fileNameRe = new Regex(@"EzService.log\d\d\d\d-\d\d-\d\d(\.\d+)*.zip$");

		private const string RequestNeedle = "Request ";
		private const string ToUrlNeedle = " to URL: ";
		private const string FormIDNeedle = "<FORM_ID>";
		private const string AmlNeedle = "Request AML A service for key ";
		private const string ConsumerNeedle = "GetConsumerInfo: request Experian service.";
	} // class Program
} // namespace
