using System;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Ezbob.Logger;

namespace PacnetBalance {
	public static class ParsePacNetText {
		private static decimal openingBalance;
		private static decimal closingBalance;
		private static decimal credits;
		private static decimal debits;
		private static DateTime date;

		static List<decimal> fasterPayment = new List<decimal>();
		static List<decimal> fee = new List<decimal>();

		static ParsePacNetText() {
			Logger = new SafeLog();
		} // static constructor

		public static ASafeLog Logger { get; set; }

		/// <summary>
		/// Parse pdf to text string
		/// </summary>
		/// <param name="data">pdf data stream</param>
		public static void ParsePdf(byte[] data) {
			fasterPayment = new List<decimal>();
			fee = new List<decimal>();
			var reader = new PdfReader(data);

			for (int page = 1; page <= reader.NumberOfPages; page++) {
				string text = PdfTextExtractor.GetTextFromPage(reader, page);
				string[] lines = text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

				foreach (string line in lines)
					HandleLine(line);
			} // for

			reader.Close();

			Logger.Info("PaymentSum: {0}", fasterPayment.Sum() + fee.Sum());

			PacNetBalance.PopulateList(date, openingBalance, closingBalance, credits, debits, fasterPayment, GetFee());
		} // ParsePdf

		/// <summary>
		/// Parse each line of report text and populate the fields they represent 
		/// </summary>
		/// <param name="line">string line of report</param>
		public static void HandleLine(string line) {
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

				fasterPayment.Add(value);
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

				fee.Add(value);
				Logger.Info("Fee: {0}", value);
			} // if
		} // HandleLine

		/// <summary>
		/// Returns the value (string representing a number without commas
		/// </summary>
		/// <param name="strList">line separated by spaces</param>
		/// <param name="name">Name of before value</param>
		/// <returns></returns>
		private static string GetValue(string[] strList, string name) {
			for (int i = 0; i < strList.Length; ++i)
				if (strList[i] == name)
					return strList[i + 1].Replace(",", "");

			return string.Empty;
		} // GetValue

		/// <summary>
		/// Returns calculated fee per transaction
		/// </summary>
		/// <returns> Fee per transfer </returns>
		private static decimal GetFee() {
			decimal feeSum = fee.Sum();
			decimal feePerTransfer = feeSum / fasterPayment.Count;
			Logger.Info("FeeSum: {0} Fee for each transfer: {1}", feeSum, feePerTransfer);
			return feePerTransfer;
		} // GetFee
	} // class ParsePacNetText
} // namespace
