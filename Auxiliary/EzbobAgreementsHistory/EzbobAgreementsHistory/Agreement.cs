namespace EzbobAgreementsHistory {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;

	class Agreement {
		public static void InitBasePaths(AConnection db, ASafeLog log) {
			basePaths = new List<string>();
			db.ForEachRowSafe(sr => basePaths.Add(sr["Value"]), LoadBasePathsQuery, CommandSpecies.Text);
			log.Info("Base paths are:\n\t{0}", string.Join("\n\t", basePaths));
		} // InitBasePaths

		public Agreement(string name, string changeDates, SortedSet<DateTime> releaseDates, AConnection db, ASafeLog log) {
			this.db = db;
			this.log = log.Safe();

			Name = name;
			ChangeDates = new SortedSet<DateTime>();
			ReleaseDates = new SortedSet<DateTime>();

			foreach (string date in changeDates.Split(','))
				ChangeDates.Add(S2D(date));

			if ((ChangeDates.Count < 1) || (releaseDates == null) || (releaseDates.Count < 1))
				return;

			DateTime firstChangeDate = ChangeDates.First();

			DateTime firstReleaseDate;

			try {
				firstReleaseDate = releaseDates.First(d => d >= firstChangeDate);
			} catch {
				return;
			} // try

			ReleaseDates.Add(firstReleaseDate);

			DateTime lastReleaseDate = firstReleaseDate;

			foreach (DateTime curReleaseDate in releaseDates.Where(d => d > firstReleaseDate)) {
				if (ChangeDates.Any(d => lastReleaseDate < d && d <= curReleaseDate))
					ReleaseDates.Add(curReleaseDate);

				lastReleaseDate = curReleaseDate;
			} // for each
		} // constructor

		public Agreement Log(Severity severity = Severity.Msg) {
			this.log.Say(
				severity,
				"\nAgreement {0}:\n\tChanged  on:{1}\n\tReleased on:{2}\n",
				Name,
				string.Join(" ", ChangeDates.Select(D2S)),
				string.Join(" ", ReleaseDates.Select(D2S))
			);

			return this;
		} // Log

		public void CopyFiles(string targetPath) {
			foreach (DateTime date in ReleaseDates)
				CopyAtLeastOneFile(targetPath, date);
		} // CopyFiles

		public string Name { get; private set; }
		public SortedSet<DateTime> ChangeDates { get; private set; }
		public SortedSet<DateTime> ReleaseDates { get; private set; }

		public static string D2S(DateTime date) {
			return date.ToString("dd/MMM/yyyy", CultureInfo.InvariantCulture);
		} // D2S

		public static DateTime S2D(string date) {
			return DateTime.ParseExact(
				date,
				"dd/MM/yyyy",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
			);
		} // S2D

		public void CopyAtLeastOneFile(string targetPath, DateTime date) {
			bool foundInDB = false;
			bool copied = false;
			
			this.db.ForEachRowSafe(
				(sr, ignored) => {
					foundInDB = true;

					if (CopyOneFile(sr["FilePath"], date, targetPath)) {
						copied = true;
						return ActionResult.SkipAll;
					} // if

					return ActionResult.Continue;
				},
				string.Format(QueryFormat, Name, date.ToString("MMMM d yyyy", CultureInfo.InvariantCulture)),
				CommandSpecies.Text
			);

			if (!foundInDB) {
				this.log.Alert(
					"No file found in DB for {0} on {1}.",
					Name,
					date.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture)
				);
			} else if (!copied) {
				this.log.Alert(
					"No file found on disk for {0} on {1}.",
					Name,
					date.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture)
				);
			} // if
		} // CopyAtLeastOneFile

		private bool CopyOneFile(string sourceRelativePath, DateTime date, string targetPath) {
			string targetFileName = Path.Combine(
				targetPath,
				string.Format("{0}-{1}.pdf", Name, date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
			);

			foreach (string basePath in basePaths) {
				string sourceFileName = Path.Combine(basePath, sourceRelativePath);

				if (!File.Exists(sourceFileName))
					continue;

				File.Copy(sourceFileName, targetFileName);

				this.log.Info(
					"{0} on {1} file copied to {2}.",
					Name,
					date.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture),
					targetFileName
				);

				return true;
			} // for each

			this.log.Warn(
				"No file found on disk for {0} on {1} with relative path of {2}.",
				Name,
				date.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture),
				sourceRelativePath
			);

			return false;
		} // CopyOneFile

		private static List<string> basePaths;

		private const string QueryFormat = @"SELECT
	a.FilePath
FROM
	LoanAgreement a
	INNER JOIN LoanAgreementTemplate t ON a.TemplateId = t.Id
	INNER JOIN LoanAgreementTemplateTypes tt ON t.TemplateType = tt.TemplateTypeID
	INNER JOIN Loan l ON a.LoanId = l.Id
	INNER JOIN CashRequests r ON l.RequestCashId = r.Id
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
WHERE
	tt.TemplateType = '{0}'
	AND
	r.UnderwriterDecisionDate >= DATEADD(day, 1, '{1}')
ORDER BY
	r.UnderwriterDecisionDate";

		private const string LoadBasePathsQuery = "SELECT Value FROM ConfigurationVariables WHERE Name IN (" +
			"'AgreementPdfConsentPath1','AgreementPdfConsentPath2','AgreementPdfLoanPath1','AgreementPdfLoanPath2'" +
		")";

		private readonly AConnection db;
		private readonly ASafeLog log;
	} // class Agreement
} // namespace
