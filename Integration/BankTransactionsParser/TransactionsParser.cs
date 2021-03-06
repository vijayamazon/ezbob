﻿namespace BankTransactionsParser {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using ExcelLibrary.SpreadSheet;
	using log4net;
	using Microsoft.VisualBasic.FileIO;
	using OfficeOpenXml;

	public class TransactionsParser {
		public static byte[] GetBytesFromFile(string fullFilePath) {
			// this method is limited to 2^32 byte files (4.2 GB)

			FileStream fs = null;
			try {
				fs = File.OpenRead(fullFilePath);
				byte[] bytes = new byte[fs.Length];
				fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
				return bytes;
			} finally {
				if (fs != null) {
					fs.Close();
					fs.Dispose();
				}
			}
		}

		public ParsedBankAccount ParseFile(string fileName, byte[] context) {
			try {
				string file = null;
				var csvContext = context;

				if (fileName.EndsWith(".csv"))
					file = fileName;

				if (fileName.EndsWith(".xlsx"))
					csvContext = XlsxToCsv(new MemoryStream(context));

				if (fileName.EndsWith(".xls"))
					csvContext = XlsToCsv(new MemoryStream(context));

				if (csvContext == null) {
					return new ParsedBankAccount {
						Error = "Failed to convert file to csv"
					};
				}
				return ParseCsv(csvContext, file);
			} catch (Exception ex) {
				Log.WarnFormat("Failed to parse the file {0} \n {1}", fileName, ex);
				return new ParsedBankAccount {
					Error = string.Format("Failed to parse the file {0} \n{1}", fileName, ex.Message)
				};
			}
		}

		public ParsedBankAccount ParseFile(string filePath) {
			try {
				string file = null;
				if (filePath.EndsWith(".csv"))
					file = filePath;

				if (filePath.EndsWith(".xlsx"))
					file = XlsxToCsv(filePath);

				if (filePath.EndsWith(".xls"))
					file = XlsToCsv(filePath);

				return ParseCsv(file);
			} catch (Exception ex) {
				Log.WarnFormat("Failed to parse the file {0} \n {1}", filePath, ex);
                return new ParsedBankAccount {
                    Error = string.Format("Failed to parse the file {0} \n{1}", filePath, ex.Message)
                };
			}
		}

		public string XlsToCsv(string filePath) {
			Workbook book = Workbook.Load(filePath);
			if (book.Worksheets != null) {
				var worksheet = book.Worksheets[0];
				int maxColumnNumber = worksheet.Cells.LastColIndex;
				var convertedRecords = new List<List<string>>(worksheet.Cells.LastRowIndex);

				for (int rowIndex = worksheet.Cells.FirstRowIndex; rowIndex <= worksheet.Cells.LastRowIndex; rowIndex++) {
					var currentRecord = new List<string>(maxColumnNumber);
					Row row = worksheet.Cells.GetRow(rowIndex);
					for (int colIndex = worksheet.Cells.FirstColIndex; colIndex <= worksheet.Cells.LastColIndex; colIndex++) {
						try {
							Cell currentCell = row.GetCell(colIndex);
							string val = currentCell.StringValue;
							try {
								val = currentCell.DateTimeValue.ToShortDateString();
							} catch {
								// ignored
							}
							AddCellValue(val, currentRecord);
						} catch (Exception) {
							AddCellValue(String.Empty, currentRecord);
						}
					}
					convertedRecords.Add(currentRecord);
				}

				string outputFilePath = String.Format("{0}.csv", filePath);
				WriteToFile(convertedRecords, outputFilePath);
				return outputFilePath;
			}

			return null;
		}

		public string XlsxToCsv(string filePath) {
			using (var doc = new ExcelPackage(new FileInfo(filePath))) {
				var workbook = doc.Workbook;
				if (workbook != null) {
					if (workbook.Worksheets.Count > 0) {
						var worksheet = workbook.Worksheets[1];
						int maxColumnNumber = worksheet.Dimension.End.Column;
						var convertedRecords = new List<List<string>>(worksheet.Dimension.End.Row);
						var excelRows = worksheet.Cells.GroupBy(c => c.Start.Row)
							.ToList();
						excelRows.ForEach(r => {
							var currentRecord = new List<string>(maxColumnNumber);
							var cells = r.OrderBy(cell => cell.Start.Column)
								.ToList();
							for (int i = 1; i <= maxColumnNumber; i++) {
								var currentCell = cells.FirstOrDefault(c => c.Start.Column == i);
								if (currentCell == null) {
									// Add a cell value for empty cells to keep data aligned.
									AddCellValue(String.Empty, currentRecord);
								} else {
									// Can't use .Text: http://epplus.codeplex.com/discussions/349696
									AddCellValue(currentCell.Value == null ? String.Empty : currentCell.Value.ToString(), currentRecord);
								}
							}
							convertedRecords.Add(currentRecord);
						});

						string outputFilePath = String.Format("{0}.csv", filePath);
						WriteToFile(convertedRecords, outputFilePath);
						return outputFilePath;
					}
				}
			}

			return null;
		}

		public byte[] XlsToCsv(Stream file) {
			Workbook book = Workbook.Load(file);
			if (book.Worksheets != null) {
				var worksheet = book.Worksheets[0];
				int maxColumnNumber = worksheet.Cells.LastColIndex;
				var convertedRecords = new List<List<string>>(worksheet.Cells.LastRowIndex);

				for (int rowIndex = worksheet.Cells.FirstRowIndex; rowIndex <= worksheet.Cells.LastRowIndex; rowIndex++) {
					var currentRecord = new List<string>(maxColumnNumber);
					Row row = worksheet.Cells.GetRow(rowIndex);
					for (int colIndex = worksheet.Cells.FirstColIndex; colIndex <= worksheet.Cells.LastColIndex; colIndex++) {
						try {
							Cell currentCell = row.GetCell(colIndex);
							string val = currentCell.StringValue;
							try {
								val = currentCell.DateTimeValue.ToShortDateString();
							} catch {
								// ignored
							}
							AddCellValue(val, currentRecord);
						} catch (Exception) {
							AddCellValue(String.Empty, currentRecord);
						}
					}
					convertedRecords.Add(currentRecord);
				}

				return GetCsvByteArray(convertedRecords);
			}

			return null;
		}

		public byte[] XlsxToCsv(Stream file) {
			using (var doc = new ExcelPackage(file)) {
				var workbook = doc.Workbook;
				if (workbook != null) {
					if (workbook.Worksheets.Count > 0) {
						var worksheet = workbook.Worksheets[1];
						int maxColumnNumber = worksheet.Dimension.End.Column;
						var convertedRecords = new List<List<string>>(worksheet.Dimension.End.Row);
						var excelRows = worksheet.Cells.GroupBy(c => c.Start.Row)
							.ToList();
						excelRows.ForEach(r => {
							var currentRecord = new List<string>(maxColumnNumber);
							var cells = r.OrderBy(cell => cell.Start.Column)
								.ToList();
							for (int i = 1; i <= maxColumnNumber; i++) {
								var currentCell = cells.FirstOrDefault(c => c.Start.Column == i);
								if (currentCell == null) {
									// Add a cell value for empty cells to keep data aligned.
									AddCellValue(String.Empty, currentRecord);
								} else {
									// Can't use .Text: http://epplus.codeplex.com/discussions/349696
									AddCellValue(currentCell.Value == null ? String.Empty : currentCell.Value.ToString(), currentRecord);
								}
							}
							convertedRecords.Add(currentRecord);
						});

						return GetCsvByteArray(convertedRecords);
					}
				}
			}

			return null;
		}

		public ParsedBankAccount ParseCsv(string filePath) {
			return ParseCsv(GetBytesFromFile(filePath), filePath);
		}

		public ParsedBankAccount ParseCsv(byte[] content, string fileName) {
			Log.DebugFormat("parsing {0}", fileName);
			Stream stream = new MemoryStream(content);
			var csvData = new DataTable();

			using (var csvReader = new TextFieldParser(stream) {
				TextFieldType = FieldType.Delimited,
				HasFieldsEnclosedInQuotes = true
			}) {
				csvReader.SetDelimiters(",");
				bool headerFound = false;
				do {
					string[] colFields = csvReader.ReadFields();
					if (colFields != null && IsHeader(colFields)) {
						headerFound = true;
						foreach (string column in colFields) {
							var dataColumn = new DataColumn(column) {
								AllowDBNull = true
							};
							csvData.Columns.Add(dataColumn);
						}
					}
				} while (!headerFound);

				if (csvReader.EndOfData) {
					Log.Warn("Header not found");
					return new ParsedBankAccount {
						Error = "Header not found"
					};
				}

				while (!csvReader.EndOfData) {
					string[] fieldData = csvReader.ReadFields();
					//Making empty value as null
					if (fieldData != null) {
						for (int i = 0; i < fieldData.Length; i++) {
							if (fieldData[i] == "")
								fieldData[i] = null;
						}
						csvData.Rows.Add(fieldData);
					}
				}
			}

			var headerColumns = FindHeaderRow(csvData);

			if (headerColumns.HaveMinimumColumns())
				return BuildBankAccount(csvData, headerColumns, fileName);

			Log.Warn("Minimum columns not found");
			return new ParsedBankAccount {
				Error = "Minimum columns not found" + Environment.NewLine + headerColumns
			};
			//PrintDataTable(csvData);
		}

		public void PrintDataTable(DataTable dataTable) {
			foreach (DataRow row in dataTable.Rows) {
				var sb = new StringBuilder();
				foreach (DataColumn col in dataTable.Columns)
					sb.Append(row[col]);
				Log.Debug(sb.ToString());
			}
		}

		private static void AddCellValue(string s, List<string> record) {
			record.Add(String.Format("{0}{1}{0}", '"', s));
		}

		/// <summary>
		///     Assumes file isn't massive
		/// </summary>
		private static void WriteToFile(List<List<string>> records, string path) {
			var commaDelimited = new List<string>(records.Count);
			records.ForEach(r => commaDelimited.Add(r.ToDelimitedString()));
			File.WriteAllLines(path, commaDelimited);
		}

		private byte[] GetCsvByteArray(List<List<string>> records) {
			var commaDelimited = new List<string>(records.Count);
			records.ForEach(r => commaDelimited.Add(r.ToDelimitedString() + Environment.NewLine));
			return commaDelimited.SelectMany(s => System.Text.Encoding.UTF8.GetBytes(s))
				.ToArray();
		}

		private ParsedBankAccount BuildBankAccount(DataTable dataTable, HeaderColumns headerColumns, string fileName) {
			var bankAccount = new ParsedBankAccount {
				Name = fileName,
				Transactions = new List<ParsedBankTransaction>()
			};

			foreach (DataRow row in dataTable.Rows) {
				try {
					var transaction = new ParsedBankTransaction();
					transaction.Description = row[headerColumns.Description].ToString();

					transaction.Date = ParseDate(row[headerColumns.Date].ToString());
					if (headerColumns.Amount != null) {
						transaction.Amount = ParseDecimal(row[headerColumns.Amount].ToString());
						transaction.IsCredit = transaction.Amount >= 0;
						transaction.Amount = Math.Abs(transaction.Amount);
					}

					if (headerColumns.Credit != null && headerColumns.Debit != null) {
						if (!String.IsNullOrEmpty(row[headerColumns.Credit].ToString())) {
							transaction.Amount = ParseDecimal(row[headerColumns.Credit].ToString());
							transaction.IsCredit = true;
						}

						if (!String.IsNullOrEmpty(row[headerColumns.Debit].ToString())) {
							transaction.Amount = ParseDecimal(row[headerColumns.Debit].ToString());
							transaction.IsCredit = false;
							transaction.Amount = Math.Abs(transaction.Amount);
						}
					}

					if (headerColumns.Balance != null)
						transaction.Balance = ParseDecimal(row[headerColumns.Balance].ToString());

					bankAccount.Transactions.Add(transaction);
				} catch (Exception ex) {
					Log.Warn("failed to parse row \n {0}", ex);
				}
			}

			bankAccount.NumOfTransactions = bankAccount.Transactions.Count();
			if (bankAccount.NumOfTransactions > 0) {
				bankAccount.DateFrom = bankAccount.Transactions.Min(x => x.Date);
				bankAccount.DateTo = bankAccount.Transactions.Max(x => x.Date);
				bankAccount.Balance = bankAccount.Transactions.OrderByDescending(x => x.Date)
					.First()
					.Balance;
			}
			Log.Debug(bankAccount);
			return bankAccount;
		}

		private DateTime ParseDate(string dateStr) {
			DateTime date;
			if (DateTime.TryParseExact(dateStr, new[] {
				"d/M/yyyy", "dd/MM/yyyy", "MM/dd/yyyy", "M/d/yyyy", "d-M-yyyy", "dd-MM-yyyy", "M-d-yyyy", "MM-dd-yyyy"
			}, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
				return date;

			if (DateTime.TryParse(dateStr, out date))
				return date;

			throw new Exception(String.Format("Failed to parse date {0}", dateStr));
		}

		private decimal ParseDecimal(string decimalStr) {
			decimal dec;
			if (Decimal.TryParse(decimalStr, NumberStyles.AllowCurrencySymbol | NumberStyles.Number, CultureInfo.InvariantCulture, out dec))
				return dec;

			if (Decimal.TryParse(Regex.Replace(decimalStr, @"[^0-9-,.]", ""), out dec))
				return dec;

			throw new Exception(String.Format("Failed to parse number {0}", decimalStr));
		}

		private bool IsHeader(string[] row) {
			var isHeader = false;
			foreach (string cell in row) {
				if (!String.IsNullOrEmpty(cell)) {
					var cellLower = cell.ToLowerInvariant();
					if (cellLower.Contains("amount") ||
						cellLower.Contains("date") ||
						cellLower.Contains("credit") ||
						cellLower.Contains("debit") ||
						cellLower.Contains("description") ||
						cellLower.Contains("text") ||
						cellLower.Contains("money") ||
						cellLower.Contains("narrative") ||
                        cellLower.Contains("memo")) {
						isHeader = true;
						break;
					}
				}
			}
			return isHeader;
		}

		private HeaderColumns FindHeaderRow(DataTable dataTable) {
			var headerColumns = new HeaderColumns();

			foreach (DataColumn col in dataTable.Columns) {
				string cell = col.ToString();
				if (!String.IsNullOrEmpty(cell)) {
					var cellLower = cell.ToLowerInvariant()
						.Trim();

					if (cellLower.Contains("date")) {
						if (headerColumns.Date == null)
							headerColumns.Date = col;
						else
							Log.Debug("more then one date columns found");
						continue;
					}
					if (cellLower.Contains("credit") || (cellLower.Contains(" in") && !cellLower.Contains("amount"))) {
						if (headerColumns.Credit == null)
							headerColumns.Credit = col;
						else
							Log.Debug("more then one credit columns found");
						continue;
					}

					if (cellLower.Contains("debit") || (cellLower.Contains(" out") && !cellLower.Contains("amount"))) {
						if (headerColumns.Debit == null)
							headerColumns.Debit = col;
						else
							Log.Debug("more then one credit columns found");
						continue;
					}

					if (cellLower.Contains("balance")) {
						if (headerColumns.Balance == null)
							headerColumns.Balance = col;
						else
							Log.Debug("more then one balance columns found");
						continue;
					}

					if ((cellLower.Contains("amount") || cellLower.Contains("value") || cellLower == "money") && !cellLower.Contains("balance") && !cellLower.Contains("debit") && !cellLower.Contains("credit")) {
						if (headerColumns.Amount == null)
							headerColumns.Amount = col;
						else
							Log.Debug("more then one amount columns found");
						continue;
					}

					if (cellLower.Contains("description") || cellLower.Contains("text") || cellLower.Contains("narrative") || cellLower.Contains("memo")) {
						if (headerColumns.Description == null)
							headerColumns.Description = col;
						else
							Log.Debug("more then one description columns found");
						continue;
					}
				}
			}
			return headerColumns;
		}

		private static readonly ILog Log = LogManager.GetLogger(typeof(TransactionsParser));
	}
}
