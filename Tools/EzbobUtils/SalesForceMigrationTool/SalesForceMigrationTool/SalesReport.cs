using System;
using System.Collections.Generic;

namespace SalesForceLib {
	using System.IO;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using OfficeOpenXml;
	using SalesForceLib.Models;

	public class SalesReport {
		private readonly AConnection db;
		private readonly ISalesForceAppClient sfClient;

		public SalesReport(AConnection db, ISalesForceAppClient sfClient) {
			this.db = db;
			this.sfClient = sfClient;
		}

		public void Execute(DateTime from, DateTime to) {
			List<SalesLoanModel> loansList = this.db.Fill<SalesLoanModel>("SalesLoans", new QueryParameter("@DateStart", from), new QueryParameter("@DateEnd", to));
			List<SalesModel> salesList = new List<SalesModel>();  
			foreach (var loan in loansList) {
				SalesModel model = new SalesModel();
				model.Loan = loan;

				model.Loan.ActiveLoans = this.db.ExecuteScalar<int>("SalesLoansActiveCount", new QueryParameter("@CustomerID", loan.Id));
				model.Sales = new List<ActivityResultModel>();
				try {
					var activity = this.sfClient.GetActivity(new GetActivityModel {
						Email = loan.Email,
						Origin = loan.Origin
					});

					if (activity != null && string.IsNullOrEmpty(activity.Error) && activity.Activities.Any()) {
						model.Sales = activity.Activities.Where(x => x.Originator != "System");
					} else {
						if (activity != null && !string.IsNullOrEmpty(activity.Error)) {
							model.Sales = new List<ActivityResultModel> {
								new ActivityResultModel {
									Creator = "Error occurred",
									Description = activity.Error
								}
							};
						}
					}
				} catch (Exception ex) {
					model.Sales = new List<ActivityResultModel> {
						new ActivityResultModel {
							Creator = "Error occurred"
						}
					};
				}
				
				salesList.Add(model);
			}

			BuildExcel(salesList);

		}

		private void BuildExcel(List<SalesModel> salesList) {
			ExcelPackage xlsx = new ExcelPackage(new FileInfo("salesreport.xlsx"));
			ExcelWorksheet sheet = xlsx.CreateSheet("SalesReport", false);
			AddTitle(sheet);
			int row = 2;
			foreach (var sale in salesList) {
				int curColumn = 1;
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.Id);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.CustomerRefNumber);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.Email);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.Fullname);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.DaytimePhone);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.MobilePhone);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.Broker);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.Origin);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.RefNum);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.LoanDate);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.LoanAmount);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.IsFirstLoan ? "Yes" : "No");
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.ActiveLoans);
				curColumn = sheet.SetCellValue(row, curColumn, sale.Loan.ApproveDate);
				var saleColumn = sheet.SetCellValue(row, curColumn, sale.Loan.ApproveAmount);

				foreach (var saleData in sale.Sales) {
					sheet.SetCellValue(row, saleColumn, saleData.Creator);
					sheet.SetCellValue(row, saleColumn + 1, saleData.Originator);
					sheet.SetCellValue(row, saleColumn + 2, saleData.StartDate);
					sheet.SetCellValue(row, saleColumn + 3, saleData.Type);
					sheet.SetCellValue(row, saleColumn + 4, saleData.Status);
					sheet.SetCellValue(row, saleColumn + 5, saleData.Subject, wrapText: true);
					sheet.SetCellValue(row, saleColumn + 6, saleData.Description, wrapText: true);
					row++;
				}

				if(!sale.Sales.Any()) row++;
			}

			xlsx.Save();
		}

		private int AddTitle(ExcelWorksheet sheet) {
			int row = 1;

			return sheet.SetRowTitles(row,
				"Id",
				"CustomerRefNumber",
				"Email",
				"Fullname",
				"DaytimePhone",
				"MobilePhone",
				"Broker",
				"Origin",
				"RefNum",
				"LoanDate",
				"LoanAmount",
				"IsFirstLoan",
				"ActiveLoans",
				"ApproveDate",
				"ApproveAmount",
				"ActivityCreator",
				"ActivityOriginator",
				"ActivityDate",
				"ActivityType",
				"ActivityStatus",
				"Subject",
				"Description");
		}
	}

	public class SalesLoanModel {
		public int Id { get; set; }
		public string CustomerRefNumber { get; set; }
		public string Email { get; set; }
		public string Fullname { get; set; }
		public string DaytimePhone { get; set; }
		public string MobilePhone { get; set; }
		public string Broker { get; set; }
		public string Origin { get; set; }
		public string RefNum { get; set; }
		public DateTime LoanDate { get; set; }
		public decimal LoanAmount { get; set; }
		public bool IsFirstLoan { get; set; }
		public DateTime ApproveDate { get; set; }
		public decimal ApproveAmount { get; set; }
		public int ActiveLoans { get; set; }
	}

	public class SalesModel {
		public SalesLoanModel Loan { get; set; }
		public IEnumerable<ActivityResultModel> Sales { get; set; } 
	}

	public class SalesDataModel {
		public string Originator { get; set; }
		public DateTime Date { get; set; }
		public string Description { get; set; }
	}
}
