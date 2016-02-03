
namespace PacnetBalance {
	using System;
	using System.Collections.Generic;
	using iTextSharp.text.pdf;
	using iTextSharp.text.pdf.parser;
	using Ezbob.Logger;

	public class ParsePacNetText {
		private readonly string loginAddress;
		private readonly string loginPassword;
		private static decimal openingBalance;
		private static decimal closingBalance;
		private static decimal credits;
		private static decimal debits;
		private static DateTime date;
		private List<PacNetBalanceRow> pacNetBalanceRows;

		public ParsePacNetText() {
			Logger = new SafeLog();
		}

		public ParsePacNetText(string loginAddress, string loginPassword) {
			this.loginAddress = loginAddress;
			this.loginPassword = loginPassword;
			Logger = new SafeLog();
		}

		public static ASafeLog Logger { get; set; }

		/// <summary>
		/// Parse pdf to text string
		/// </summary>
		/// <param name="data">pdf data stream</param>
		public void ParsePdf(byte[] data) {
			this.pacNetBalanceRows = new List<PacNetBalanceRow>();
			var reader = new PdfReader(data);

			for (int page = 1; page <= reader.NumberOfPages; page++) {
				string text = PdfTextExtractor.GetTextFromPage(reader, page);
				string[] lines = text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

				foreach (string line in lines)
					HandleLine(line);
			} // for

			reader.Close();


			PacNetBalance.PopulateList(date, openingBalance, closingBalance, credits, debits, this.pacNetBalanceRows, this.loginAddress, this.loginPassword);
		} // ParsePdf

		/// <summary>
		/// Parse each line of report text and populate the fields they represent 
		/// </summary>
		/// <param name="line">string line of report</param>
		public void HandleLine(string line) {
			if (line.Contains("Transaction Detail Report:")) {
				string text = GetValue(line.Replace("Transaction Detail Report:", "TransactionDetailReport:").Split(' '), "TransactionDetailReport:");

				if (!DateTime.TryParse(text, out date)) {
					Logger.Error("Error parsing Date: {0}", text);
					throw new PacNetBalanceException(string.Format("PacNet Error parsing Date: {0}", text));
				} // if

				Logger.Info("Date: {0}", date);
				return;
			} // if

			if (line.Contains("Opening Balance")) {
				string text = GetValue(line.Replace("Opening Balance", "OpeningBalance").Split(' '), "OpeningBalance");

				if (!decimal.TryParse(text, out openingBalance)) {
					Logger.Error("Error parsing OpeningBalance: {0}", text);
					throw new PacNetBalanceException(string.Format("PacNet Error parsing OpeningBalance: {0}", text));
				} // if

				Logger.Info("OpeningBalance: {0}", openingBalance);
				return;
			} // if

			if (line.Contains("Closing Balance")) {
				string text = GetValue(line.Replace("Closing Balance", "ClosingBalance").Split(' '), "ClosingBalance");

				if (!decimal.TryParse(text, out closingBalance)) {
					Logger.Error("Error parsing ClosingBalance: {0}", text);
					throw new PacNetBalanceException(string.Format("PacNet Error parsing ClosingBalance: {0}", text)); 
				} // if

				Logger.Info("ClosingBalance: {0}", closingBalance);
				return;
			} // if

			if (line.Contains("  Credits")) {
				string text = GetValue(line.Split(' '), "Credits");

				if (!decimal.TryParse(text, out credits)) {
					Logger.Error("Error parsing Credits: {0}", text);
					throw new PacNetBalanceException(string.Format("PacNet Error parsing Credits: {0}", text)); 
				} // if

				Logger.Info("Credits: {0}", credits);
				return;
			} // if

			if (line.Contains("  Debits")) {
				string text = GetValue(line.Split(' '), "Debits");

				if (!decimal.TryParse(text, out debits)) {
					Logger.Error("Error parsing Debits: {0}", text);
					throw new PacNetBalanceException(string.Format("PacNet Error parsing Debits: {0}", text)); 
				} // if

				Logger.Info("Debits: {0}", debits);
				return;
			} // if

			if (line.Contains("FasterPayment")) {
				string text = GetValue(line.Split(' '), "GBP");
				decimal value;

				if (!decimal.TryParse(text, out value)) {
					Logger.Error("Error parsing FasterPayment: {0}", text);
					throw new PacNetBalanceException(string.Format("PacNet Error parsing FasterPayment: {0}", text)); 
				} // if

				this.pacNetBalanceRows.Add(new PacNetBalanceRow {
					Amount = value,
					Date = date,
					IsCredit = value < 0
				});
				
				Logger.Info("FasterPayment: {0}", value);
				return;
			} // if

			if (line.Contains("File fee") || line.Contains("Item Fee")) {
				string text = GetValue(line.Split(' '), "GBP");
				decimal value;

				if (!decimal.TryParse(text, out value)) {
					Logger.Error("Error parsing Fee: {0}", text);
					throw new PacNetBalanceException(string.Format("PacNet Error parsing Fee: {0}", text)); 
				} // if

				this.pacNetBalanceRows.Add(new PacNetBalanceRow {
					Fees = value,
					Date = date,
					IsCredit = value < 0
				});
				Logger.Info("Fee: {0}", value);
			} // if

			if (line.Contains("Commission")) {
				string text = GetValue(line.Split(' '), "GBP");
				decimal value;

				if (!decimal.TryParse(text, out value)) {
					Logger.Error("Error parsing Commission: {0}", text);
					throw new PacNetBalanceException(string.Format("PacNet Error parsing Commission: {0}", text));
				} // if

				this.pacNetBalanceRows.Add(new PacNetBalanceRow {
					Fees = value,
					Date = date,
					IsCredit = value < 0
				});
				Logger.Info("Fee: {0}", value);
			} // if

			if (line.Contains("Wire in") || line.Contains("Transfer in")) {
				string text = GetValue(line.Split(' '), "GBP");
				decimal value;

				if (!decimal.TryParse(text, out value)) {
					Logger.Error("Error parsing Wire in: {0}", text);
					throw new PacNetBalanceException(string.Format("PacNet Error parsing Wire in: {0}", text));
				} // if

				this.pacNetBalanceRows.Add(new PacNetBalanceRow {
					Amount = value,
					Date = date,
					IsCredit = value > 0
				});
				Logger.Info("Fee: {0}", value);
			} // if
		} // HandleLine

		/// <summary>
		/// Returns the value (string representing a number without commas
		/// </summary>
		/// <param name="strList">line separated by spaces</param>
		/// <param name="name">Name of before value</param>
		/// <returns></returns>
		private string GetValue(string[] strList, string name) {
			for (int i = 0; i < strList.Length; ++i)
				if (strList[i].EndsWith(name))
					return strList[i + 1].Replace(",", "");

			return string.Empty;
		} // GetValue
	} // class ParsePacNetText
} // namespace
