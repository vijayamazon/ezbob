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
			log = new FileLog("ziplogscan", true);

			log.Info("Logging started.");

			log.Info(
				"Command line arguments (count = {0}): {1}",
				args.Length,
				args.Length == 0 ? "-- none --" : string.Join(" ", args)
			);

			string sourcePath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

			if (Directory.Exists(sourcePath))
				new Program(sourcePath).Run();
			else
				log.Warn("Nothing to do: source path '{0}' not found.", sourcePath);

			log.Info("Logging stopped.");
		} // Main

		private Program(string sourcePath) {
			this.requests = new List<Request>();
			this.results = new SortedTable<string, RequestTypes, int>();
			this.sourcePath = sourcePath;
		} // constructor

		private void Run() {
			log.Debug("Scan started for path '{0}'...", this.sourcePath);

			foreach (int year in years)
				foreach (int month in months)
					ScanMonth(year, month);

			foreach (Request rq in this.requests) {
				if (!this.results.Contains(rq.Month, rq.RequestType))
					this.results[rq.Month, rq.RequestType] = 1;
				else
					this.results[rq.Month, rq.RequestType]++;
			} // for each

			log.Debug("Results are:\n\n{0}\n\n", this.results.ToFormattedString("Experian requests per months"));

			log.Debug("Scan complete for path '{0}'.", this.sourcePath);
		} // Run

		private void ScanMonth(int year, int month) {
			log.Debug("Scanning month {0}/{1}...", month, year);

			this.fsmState = FsmStates.Start;

			for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
				ScanDay(year, month, day);

			log.Debug("Scanning month {0}/{1} complete.", month, year);
		} // ScanMonth

		private void ScanDay(int year, int month, int day) {
			string mask = string.Format(FileNamePattern, year.ToString("0000"), month.ToString("00"), day.ToString("00"));

			log.Debug("Scanning day {0}/{1}/{2} using mask '{3}'...", day, month, year, this.sourcePath);

			string[] files = Directory.GetFiles(this.sourcePath, mask);

			log.Info("{0} found by mask {1}.", Grammar.Number(files.Length, "file"), mask);

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

				DateTime time = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);

				foreach (KeyValuePair<int, string> pair in sortedFileNames.Reverse())
					ExtractAndProcess(pair.Value, time);

				ExtractAndProcess(lastFileName, time);
			} // if

			log.Debug("Scanning month {0}/{1}/{2} using mask '{3}' complete.", day, month, year, this.sourcePath);
		} // ScanDay

		private void ExtractAndProcess(string fileName, DateTime time) {
			log.Debug("Processing zip file '{0}'...", fileName);

			ZipArchive archive = ZipFile.OpenRead(fileName);

			foreach (ZipArchiveEntry entry in archive.Entries)
				ProcessZippedFile(entry, time);

			log.Debug("Processing zip file '{0}' complete.", fileName);
		} // ExtractAndProcess

		private void ProcessZippedFile(ZipArchiveEntry zippedFile, DateTime time) {
			var rdr = new StreamReader(zippedFile.Open());

			var pc = new ProgressCounter("{0} lines processed.", log, 25000UL);

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
					this.requests.Add(new Request(RequestTypes.Consumer, time, log));
					break;
				} // if

				if (line.IndexOf(AmlNeedle, StringComparison.InvariantCulture) > -1) {
					this.requests.Add(new Request(RequestTypes.Consumer, time, log));
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
					this.requests.Add(new Request(line[formIDPos + FormIDNeedle.Length], time, log));
					this.fsmState = FsmStates.Start;
				} // if

				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // ProcessLine

		private FsmStates fsmState;

		private static ASafeLog log;

		private readonly List<Request> requests;

		private readonly SortedTable<string, RequestTypes, int> results; 

		private const string FileNamePattern = "EzService.log{0}-{1}-{2}*.zip";
		private static readonly Regex fileNameRe = new Regex(@"EzService.log\d\d\d\d-\d\d-\d\d(\.\d+)*.zip$");

		private const string RequestNeedle = "Request ";
		private const string ToUrlNeedle = " to URL: ";
		private const string FormIDNeedle = "<FORM_ID>";
		private const string AmlNeedle = "Request AML A service for key ";
		private const string ConsumerNeedle = "GetConsumerInfo: request Experian service.";

		private readonly string sourcePath;
		private static readonly SortedSet<int> years = new SortedSet<int> { 2015, };
		private static readonly SortedSet<int> months = new SortedSet<int> { 7, 8, 9, };
	} // class Program
} // namespace
