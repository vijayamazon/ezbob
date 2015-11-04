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

			if (!int.TryParse(this.args.Dequeue(), out this.month)) {
				this.log.Fatal("Could not parse <month> argument.");
				Usage();
				return false;
			} // if

			if ((this.month < 1) || (this.month > 12)) {
				this.log.Fatal("<month> should be between 1 and 12.");
				Usage();
				return false;
			} // if

			return true;
		} // Init

		private void Usage() {
			this.log.Info("Usage: ziplogscan <path to scan> <year> <month>");
			this.log.Info("Year must be at least 2012.");
			this.log.Info("Month must be between 1 and 12.");
		} // Usage

		private void Done() {
			this.log.Info("Logging stopped.");
		} // Done

		private void Run() {
			this.log.Debug("Scan started for path '{0}'...", this.sourcePath);

			ScanMonth();

			foreach (Request rq in this.requests) {
				if (!this.monthlyResults.Contains(rq.Month, rq.RequestType))
					this.monthlyResults[rq.Month, rq.RequestType] = 1;
				else
					this.monthlyResults[rq.Month, rq.RequestType]++;

				if (!this.dailyResults.Contains(rq.Day, rq.RequestType))
					this.dailyResults[rq.Day, rq.RequestType] = 1;
				else
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

		private void ScanMonth() {
			this.log.Debug("Scanning month {0}/{1}...", this.month, this.year);

			this.fsmState = FsmStates.Start;

			for (int day = 1; day <= DateTime.DaysInMonth(this.year, this.month); day++)
				ScanDay(day);

			this.log.Debug("Scanning month {0}/{1} complete.", this.month, this.year);
		} // ScanMonth

		private void ScanDay(int day) {
			string mask = string.Format(
				FileNamePattern,
				this.year.ToString("0000"),
				this.month.ToString("00"),
				day.ToString("00")
			);

			this.log.Debug("Scanning day {0}/{1}/{2} using mask '{3}'...", day, this.month, this.year, this.sourcePath);

			string[] files = Directory.GetFiles(this.sourcePath, mask);

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
					ExtractAndProcess(pair.Value, time);

				ExtractAndProcess(lastFileName, time);
			} // if

			this.log.Debug(
				"Scanning month {0}/{1}/{2} using mask '{3}' complete.",
				day,
				this.month,
				this.year,
				this.sourcePath
			);
		} // ScanDay

		private void ExtractAndProcess(string fileName, DateTime time) {
			this.log.Debug("Processing zip file '{0}'...", fileName);

			ZipArchive archive = ZipFile.OpenRead(fileName);

			foreach (ZipArchiveEntry entry in archive.Entries)
				ProcessZippedFile(entry, time);

			this.log.Debug("Processing zip file '{0}' complete.", fileName);
		} // ExtractAndProcess

		private void ProcessZippedFile(ZipArchiveEntry zippedFile, DateTime time) {
			var rdr = new StreamReader(zippedFile.Open());

			var pc = new ProgressCounter("{0} lines processed.", this.log, 25000UL);

			for ( ; ; ) {
				string line = rdr.ReadLine();

				if (line == null)
					break;

				ProcessLine(line, time);
				pc.Next();
			} // for

			pc.Log();
		} // ProcessZippedFile

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

		private const string FileNamePattern = "EzService.log{0}-{1}-{2}*.zip";
		private static readonly Regex fileNameRe = new Regex(@"EzService.log\d\d\d\d-\d\d-\d\d(\.\d+)*.zip$");

		private const string RequestNeedle = "Request ";
		private const string ToUrlNeedle = " to URL: ";
		private const string FormIDNeedle = "<FORM_ID>";
		private const string AmlNeedle = "Request AML A service for key ";
		private const string ConsumerNeedle = "GetConsumerInfo: request Experian service.";
	} // class Program
} // namespace
