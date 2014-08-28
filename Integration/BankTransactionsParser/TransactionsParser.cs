namespace BankTransactionsParser {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using Ezbob.Logger;
	using Microsoft.VisualBasic.FileIO;

	public class TransactionsParser {
		private static readonly ConsoleLog log = new ConsoleLog();

		public void ParseXls(byte[] content, string fileName) {

		}

		public BankAccount ParseCsv(byte[] content, string fileName) {
			log.Debug("parsing {0}", fileName);
			Stream stream = new MemoryStream(content);
			var csvData = new DataTable();

			using (var csvReader = new TextFieldParser(stream) { TextFieldType = FieldType.Delimited, HasFieldsEnclosedInQuotes = true }) {
				csvReader.SetDelimiters(new string[] { ",", ";" });

				string[] colFields = csvReader.ReadFields();
				if (colFields != null) {
					foreach (string column in colFields) {
						var dataColumn = new DataColumn(column) { AllowDBNull = true };
						csvData.Columns.Add(dataColumn);

					}
				}

				while (!csvReader.EndOfData) {
					string[] fieldData = csvReader.ReadFields();
					//Making empty value as null
					if (fieldData != null) {
						for (int i = 0; i < fieldData.Length; i++) {
							if (fieldData[i] == "") {
								fieldData[i] = null;
							}
						}
						csvData.Rows.Add(fieldData);
					}
				}
			}

			var headerColumns = FindHeaderRow(csvData);

			if (headerColumns.HaveMinimumColumns()) {
				return BuildBankAccount(csvData, headerColumns, fileName);
			}

			log.Warn("Minimum columns not found");
			return null;
			//PrintDataTable(csvData);
		}

		private BankAccount BuildBankAccount(DataTable dataTable, HeaderColumns headerColumns, string fileName) {
			var bankAccount = new BankAccount { Name = fileName, Transactions = new List<BankTransaction>() };

			foreach (DataRow row in dataTable.Rows) {
				try {
					var transaction = new BankTransaction();
					transaction.Description = row[headerColumns.Description].ToString();

					transaction.Date = ParseDate(row[headerColumns.Date].ToString());
					if (headerColumns.Amount != null) {
						transaction.Amount = ParseDecimal(row[headerColumns.Amount].ToString());
						transaction.IsCredit = transaction.Amount >= 0;
						transaction.Amount = Math.Abs(transaction.Amount);
					}

					if (headerColumns.Credit != null && headerColumns.Debit != null) {
						if (!string.IsNullOrEmpty(row[headerColumns.Credit].ToString())) {
							transaction.Amount = ParseDecimal(row[headerColumns.Credit].ToString());
							transaction.IsCredit = true;
						}

						if (!string.IsNullOrEmpty(row[headerColumns.Debit].ToString())) {
							transaction.Amount = ParseDecimal(row[headerColumns.Debit].ToString());
							transaction.IsCredit = false;
							transaction.Amount = Math.Abs(transaction.Amount);
						}
					}

					if (headerColumns.Balance != null) {
						transaction.Balance = ParseDecimal(row[headerColumns.Balance].ToString());
					}

					bankAccount.Transactions.Add(transaction);

				} catch (Exception ex) {
					log.Warn("failed to parse row \n {0}", ex);
				}
			}

			bankAccount.NumOfTransactions = bankAccount.Transactions.Count();
			if (bankAccount.NumOfTransactions > 0) {
				bankAccount.DateFrom = bankAccount.Transactions.Min(x => x.Date);
				bankAccount.DateTo = bankAccount.Transactions.Max(x => x.Date);
				bankAccount.Balance = bankAccount.Transactions.OrderByDescending(x => x.Date).First().Balance;
			}
			log.Debug(bankAccount);
			return bankAccount;
		}

		private DateTime ParseDate(string dateStr) {
			DateTime date;
			if (DateTime.TryParseExact(dateStr, new[] { "d/M/yyyy", "dd/MM/yyyy", "MM/dd/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
				return date;
			}

			if (DateTime.TryParse(dateStr, out date)) {
				return date;
			}
			
			throw new Exception(string.Format("Failed to parse date {0}", dateStr));
		}

		private decimal ParseDecimal(string decimalStr) {
			return decimal.Parse(decimalStr, NumberStyles.AllowCurrencySymbol | NumberStyles.Number);
		}

		private HeaderColumns FindHeaderRow(DataTable dataTable) {
			var headerExists = false;
			foreach (DataColumn col in dataTable.Columns) {
				string cell = col.ToString();
				if (!string.IsNullOrEmpty(cell)) {
					var cellLower = cell.ToLowerInvariant();
					if (cellLower.Contains("amount") ||
					   cellLower.Contains("date") ||
					   cellLower.Contains("credit") ||
					   cellLower.Contains("debit") ||
					   cellLower.Contains("description") ||
					   cellLower.Contains("text") ||
					   cellLower.Contains("money") ||
					   cellLower.Contains("narrative")) {
						headerExists = true;
						break;
					}
				}
			}

			var headerColumns = new HeaderColumns();
			if (headerExists) {
				foreach (DataColumn col in dataTable.Columns) {
					string cell = col.ToString();
					if (!string.IsNullOrEmpty(cell)) {
						var cellLower = cell.ToLowerInvariant().Trim();

						if (cellLower.Contains("date")) {
							if (headerColumns.Date == null) {
								headerColumns.Date = col;
							} else {
								log.Debug("more then one date columns found");
							}
							continue;
						}

						if ((cellLower.Contains("amount") || cellLower.Contains("value") || cellLower == "money") && !cellLower.Contains("balance")) {
							if (headerColumns.Amount == null) {
								headerColumns.Amount = col;
							} else {
								log.Debug("more then one amount columns found");
							}
							continue;
						}

						if (cellLower.Contains("balance")) {
							if (headerColumns.Balance == null) {
								headerColumns.Balance = col;
							} else {
								log.Debug("more then one balance columns found");
							}
							continue;
						}

						if (cellLower.Contains("description") || cellLower.Contains("text") || cellLower.Contains("narrative")) {
							if (headerColumns.Description == null) {
								headerColumns.Description = col;
							} else {
								log.Debug("more then one description columns found");
							}
							continue;
						}

						if (cellLower.Contains("credit") || cellLower.Contains(" in")) {
							if (headerColumns.Credit == null) {
								headerColumns.Credit = col;
							} else {
								log.Debug("more then one credit columns found");
							}
							continue;
						}

						if (cellLower.Contains("debit") || cellLower.Contains(" out")) {
							if (headerColumns.Debit == null) {
								headerColumns.Debit = col;
							} else {
								log.Debug("more then one credit columns found");
							}
							continue;
						}
					}
				}
			} else {
				log.Debug("Header not found");
			}

			return headerColumns;
		}

		public void PrintDataTable(DataTable dataTable) {
			
			foreach (DataRow row in dataTable.Rows) {
				var sb = new StringBuilder();
				foreach (DataColumn col in dataTable.Columns) {
					sb.Append(row[col]);
				}
				log.Debug(sb.ToString());
			}
		}
	}
}
