namespace ExtractDataForLsa {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.ExcelExt;
	using Ezbob.Utils;
	using OfficeOpenXml;

	class CustomerData : AResultRow {
		public string LoanID { get; set; }
		public int LoanInternalID { get; set; }
		public string CustomerRefnum { get; set; }
		public string Email { get; set; }
		public string PromoCode { get; set; }
		public decimal RequestedLoanAmount { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string Gender { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string MaritalStatus { get; set; }
		public string PostCode { get; set; }
		public string TimeAttAddress { get; set; }
		public string MobilePhone { get; set; }
		public string DaytimePhone { get; set; }
		public string TypeOfBusiness { get; set; }
		public string IndustryType { get; set; }
		public string CompanyRegistrationNumber { get; set; }
		public string CompanyName { get; set; }
		public string MonthsOfOperation { get; set; }
		public int EmployeeCount { get; set; }
		public decimal TotalMonthlySalary { get; set; }
		public string WhoFilesWithHmrc { get; set; }
		public string BusinessAddress_Organisation { get; set; }
		public string BusinessAddress_Line1 { get; set; }
		public string BusinessAddress_Line2 { get; set; }
		public string BusinessAddress_Line3 { get; set; }
		public string BusinessAddress_Town { get; set; }
		public string BusinessAddress_County { get; set; }
		public string BusinessAddress_Postcode { get; set; }
		public string BusinessAddress_Country { get; set; }
		public string UnderCurrentOwnership { get; set; }
		public string BusinessPhoneNumber { get; set; }
		public int NumberOfEmployees { get; set; }
		public string MainIndustry { get; set; }

		public string FullName {
			get {
				var lst = new List<string> {
					FirstName,
					MiddleName,
					LastName,
				};

				return string.Join(" ", lst.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
			} // get
		} // FullName

		public void SaveTo(ExcelWorksheet sheet) {
			int row = 2;

			this.Traverse((instance, pi) => {
				int column = 1;

				column = sheet.SetCellValue(row, column, pi.Name);
				column = sheet.SetCellValue(row, column, pi.GetValue(instance));

				row++;
			});
		} // SaveTo
	} // class CustomerData
} // namespace
