using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Reports
{
	#region class CciReportItem

	public class CciReportItem
	{
		#region public

		#region method CreateTable

		public static DataTable CreateTable()
		{
			var oOutput = new DataTable();

			oOutput.Columns.Add("DebtorType", typeof(string));
			oOutput.Columns.Add("LoanType", typeof(string));
			oOutput.Columns.Add("OriginalAmount", typeof(decimal));
			oOutput.Columns.Add("Principal", typeof(decimal));
			oOutput.Columns.Add("Fees", typeof(decimal));
			oOutput.Columns.Add("Interest", typeof(decimal));
			oOutput.Columns.Add("TotalDue", typeof(decimal));
			oOutput.Columns.Add("Currency", typeof(string));
			oOutput.Columns.Add("LoanRef", typeof(string));
			oOutput.Columns.Add("DateOfAgreement", typeof(DateTime));
			oOutput.Columns.Add("DateOfDefault", typeof(DateTime));
			oOutput.Columns.Add("CompanyName", typeof(string));
			oOutput.Columns.Add("LegalProcess", typeof(string));
			oOutput.Columns.Add("LegalAmount", typeof(decimal));
			oOutput.Columns.Add("Gender", typeof(string));
			oOutput.Columns.Add("FirstName", typeof(string));
			oOutput.Columns.Add("LastName", typeof(string));
			oOutput.Columns.Add("DateOfBirth", typeof(DateTime));
			oOutput.Columns.Add("NationalInsurance", typeof(string));
			oOutput.Columns.Add("MobilePhone", typeof(string));
			oOutput.Columns.Add("DaytimePhone", typeof(string));
			oOutput.Columns.Add("EmailApplicant", typeof(string));
			oOutput.Columns.Add("EbayPhone", typeof(string));
			oOutput.Columns.Add("PaypalPhone", typeof(string));
			oOutput.Columns.Add("EmailEbay", typeof(string));
			oOutput.Columns.Add("HasOtherEbay", typeof(string));
			oOutput.Columns.Add("CurrentAddress", typeof(string));
			oOutput.Columns.Add("OfficeAdress", typeof(string));
			oOutput.Columns.Add("PropertyStatusDescription", typeof(string));
			oOutput.Columns.Add("Notes", typeof(string));
			oOutput.Columns.Add("CssClass", typeof(string));

			return oOutput;
		} // CreateTable

		#endregion method CreateTable

		#region constructor

		public CciReportItem(DbDataReader oRow, SortedDictionary<int, decimal> oEarnedInterestList)
		{
			try
			{
				int nLoanID = Convert.ToInt32(oRow["LoanID"]);
				decimal nEarnedInterest = oEarnedInterestList.ContainsKey(nLoanID) ? oEarnedInterestList[nLoanID] : 0;

				DebtorType = oRow["DebtorType"].ToString();
				LoanType = oRow["LoanType"].ToString();
				OriginalAmount = Convert.ToDecimal(oRow["OriginalAmount"]);
				Principal = Convert.ToDecimal(oRow["Principal"]);
				Fees = Convert.ToDecimal(oRow["Fees"]) - Convert.ToDecimal(oRow["RepaidFees"]);
				Interest = nEarnedInterest - Convert.ToDecimal(oRow["RepaidInterest"]);
				LoanRef = oRow["LoanRef"].ToString();
				DateOfAgreement = AdjustDate(Convert.ToDateTime(oRow["DateOfAgreement"]));
				DateOfDefault = AdjustDate(Convert.ToDateTime(oRow["DateOfDefault"]));
				CompanyName = oRow["CompanyName"].ToString();
				Gender = oRow["Gender"].ToString();
				FirstName = oRow["FirstName"].ToString();
				LastName = oRow["LastName"].ToString();
				DateOfBirth = AdjustDate(Convert.ToDateTime(oRow["DateOfBirth"]));
				MobilePhone = oRow["MobilePhone"].ToString();
				DaytimePhone = oRow["DaytimePhone"].ToString();
				EmailApplicant = oRow["EmailApplicant"].ToString();
				EbayPhone = oRow["EbayPhone"].ToString();
				PaypalPhone = oRow["PaypalPhone"].ToString();
				EmailEbay = oRow["EmailEbay"].ToString();
				HasOtherEbay = oRow["HasOtherEbay"].ToString();
				CurrentAddress = oRow["CurrentAddress"].ToString();
				OfficeAdress = oRow["OfficeAddress"].ToString();
				PropertyStatusDescription = oRow["PropertyStatusDescription"].ToString();
			}
			catch {
				// Silently ignore.
			} // try
		} // constructor

		#endregion constructor

		#region method ToRow

		public void ToRow(DataTable tbl)
		{
			if (Math.Abs(TotalDue) < 0.01m)
				return;

			var sClass = string.Empty;

			if (Math.Abs(Principal + Fees) < 0.01m)
				sClass = (Interest <= -0.01m) ? "unmatched" : sClass;
			else
				sClass = (Interest <= -0.01m) ? "highlight" : sClass;

			tbl.Rows.Add(
				DebtorType, LoanType, OriginalAmount, Principal, Fees, Interest, TotalDue,
				Currency, LoanRef, DateOfAgreement, DateOfDefault, CompanyName, LegalProcess, LegalAmount,
				Gender, FirstName, LastName, DateOfBirth, NationalInsurance, MobilePhone, DaytimePhone,
				EmailApplicant, EbayPhone, PaypalPhone, EmailEbay, HasOtherEbay, CurrentAddress, OfficeAdress,
				PropertyStatusDescription, Notes, sClass
			);
		} // ToRow

		#endregion method ToRow

		#region properties

		public string DebtorType { get; private set; }
		public string LoanType { get; private set; }
		public decimal OriginalAmount { get; private set; }
		public decimal Principal { get; private set; }
		public decimal Fees { get; private set; }
		public decimal Interest { get; private set; }
		public decimal TotalDue { get { return Principal + Fees + Interest; } }
		public string Currency { get { return "GBP"; } }
		public string LoanRef { get; private set; }
		public DateTime? DateOfAgreement { get; private set; }
		public DateTime? DateOfDefault { get; private set; }
		public string CompanyName { get; private set; }
		public string LegalProcess { get { return ""; } }
		public decimal? LegalAmount { get { return null; } }
		public string Gender { get; private set; }
		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public DateTime? DateOfBirth { get; private set; }
		public string NationalInsurance { get { return ""; } }
		public string MobilePhone { get; private set; }
		public string DaytimePhone { get; private set; }
		public string EmailApplicant { get; private set; }
		public string EbayPhone { get; private set; }
		public string PaypalPhone { get; private set; }
		public string EmailEbay { get; private set; }
		public string HasOtherEbay { get; private set; }
		public string CurrentAddress { get; private set; }
		public string OfficeAdress { get; private set; }
		public string PropertyStatusDescription { get; private set; }
		public string Notes { get { return ""; } }

		#endregion properties

		#endregion public

		#region private

		#region method AdjustDate

		private static DateTime? AdjustDate(DateTime? oDate)
		{
			if (!oDate.HasValue)
				return null;

			return (oDate.Value.Date == ms_oLongAgo.Date) ? (DateTime?)null : oDate.Value.Date;
		} // AdjustDate

		#endregion method AdjustDate

		private static DateTime ms_oLongAgo = new DateTime(1976, 7, 1, 0, 0, 0, DateTimeKind.Utc);

		#endregion private
	} // class CciReportItem

	#endregion class CciReportItem
} // namespace Reports
