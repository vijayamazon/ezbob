namespace Ezbob.Backend.CalculateLoan.Models {
	using System;
	using System.Globalization;
	using Ezbob.Backend.Extensions;

	public class OneDayLoanStatus {
		public static string FormatField(string field, int length) {
			string preResult = " " + (field ?? string.Empty) + " ";
			return (length >= 0) ? preResult.PadLeft(length + 2) : preResult.PadRight(2 - length);
		} // FormatField

		/// <summary>
		/// 
		/// </summary>
		/// <param name="d"></param>
		/// <param name="openPrincipal"></param>
		/// <param name="previousDay"></param>
		public OneDayLoanStatus(DateTime d, decimal openPrincipal, OneDayLoanStatus previousDay) {
			IsReschedulingDay = false;
			IsBetweenLastPaymentAndReschedulingDay = false;
			Date = d;
			OpenPrincipalForInterest = openPrincipal;
			OpenPrincipalAfterRepayments = openPrincipal;
			AssignedFees = 0;
			DailyInterestRate = 0;
			this.previousDay = previousDay;

			Str = new FormattedData(this);
		} // constructor

		public DateTime Date {
			get { return this.date.Date; }
			set { this.date = value.Date; }
		} // Date

		/// <summary>
		/// This open principal customer has at the beginning (i.e. 0:00:00) of the day.
		/// </summary>
		public decimal RawOpenPrincipalForInterest { get; private set; }

		/// <summary>
		/// This open principal customer has at the end (i.e. 23:59:59) of the day.
		/// </summary>
		public decimal RawOpenPrincipalAfterRepayments { get; private set; }

		/// <summary>
		/// This is open principal is used to calculate daily earned interest for this day.
		/// </summary>
		public decimal OpenPrincipalForInterest {
			get { return IsBetweenLastPaymentAndReschedulingDay ? 0 : RawOpenPrincipalForInterest; }
			set { RawOpenPrincipalForInterest = value; }
		} // OpenPrincipalForInterest

		/// <summary>
		/// This is open principal is used to calculate daily current balance.
		/// </summary>
		public decimal OpenPrincipalAfterRepayments {
			get { return IsBetweenLastPaymentAndReschedulingDay ? 0 : RawOpenPrincipalAfterRepayments; }
			set { RawOpenPrincipalAfterRepayments = value; }
		} // OpenPrincipalAfterRepayments

		/// <summary>
		/// Interest earned on that day.
		/// </summary>
		public decimal DailyInterest {
			get { return OpenPrincipalForInterest * DailyInterestRate; }
		} // DailyInterest

		/// <summary>
		/// Fees assigned on that day.
		/// </summary>
		public decimal AssignedFees { get; set; }

		/// <summary>
		/// Interest rate on that day. This interest rate is for one day only.
		/// </summary>
		public decimal DailyInterestRate { get; set; }

		public decimal RepaidPrincipal { get; set; }
		public decimal RepaidInterest { get; set; }
		public decimal RepaidFees { get; set; }

		public decimal TotalRepaidPrincipal {
			get { return RepaidPrincipal + (this.previousDay == null ? 0 : this.previousDay.TotalRepaidPrincipal); }
		} // TotalRepaidPrincipal

		public decimal TotalEarnedInterest {
			get { return DailyInterest + (this.previousDay == null ? 0 : this.previousDay.TotalEarnedInterest); }
		} // TotalEarnedInterest

		public decimal TotalAssignedFees {
			get { return AssignedFees + (this.previousDay == null ? 0 : this.previousDay.TotalAssignedFees); }
		} // TotalAssignedFees

		public decimal TotalRepaidInterest {
			get { return RepaidInterest + (this.previousDay == null ? 0 : this.previousDay.TotalRepaidInterest); }
		} // TotalRepaidInterest

		public decimal TotalRepaidFees {
			get { return RepaidFees + (this.previousDay == null ? 0 : this.previousDay.TotalRepaidFees); }
		} // TotalRepaidFees

		public decimal TotalExpectedNonprincipalPayment {
			get { return AccruedInterest + TotalAssignedFees - TotalRepaidFees; }
		} // TotalExpectedNonprincipalPayment

		public decimal CurrentBalance{
			get { return OpenPrincipalAfterRepayments + TotalExpectedNonprincipalPayment; }
		} // CurrentBalance

		public decimal AccruedInterest {
			get { return TotalEarnedInterest - TotalRepaidInterest; }
		} // AccruedInterest

		//public void AddRepayment(Repayment rp) {
		//	if (rp == null)
		//		return;

		//	RepaidPrincipal += rp.Principal;
		//	RepaidInterest += rp.Interest;
		//	RepaidFees += rp.Fees;
		//} // AddRepayment

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"on {0}: p({1}::{2}) i{3} f{4} (dr {5}) rp{6} ri{7} rf{8}",
				Str.Date,
				Str.OpenPrincipalForInterest,
				Str.OpenPrincipalAfterRepayments,
				Str.DailyInterest,
				Str.AssignedFees,
				Str.DailyInterestRate,
				Str.RepaidPrincipal,
				Str.RepaidInterest,
				Str.RepaidFees
			);
		} // ToString

		public string ToFormattedString(
			string rowPrefix,
			string separator,
			string note,
			int dateLen,
			int openPrincipalLen,
			int dailyInterestLen,
			int assignedFeesLen,
			int dailyInterestRateLen,
			int currentBalanceLen,
			int repaidPrincipalLen,
			int repaidInterestLen,
			int repaidFeesLen,
			int ignoredDayLen,
			int notesLen
		) {
			return string.Format(
				"{0}{1}{2}{1}",
				rowPrefix,
				separator,
				string.Join(separator,
					FormatField(Str.Date, dateLen),
					FormatField(Str.OpenPrincipalForInterest, openPrincipalLen),
					FormatField(Str.OpenPrincipalAfterRepayments, openPrincipalLen),
					FormatField(Str.DailyInterest, dailyInterestLen),
					FormatField(Str.AssignedFees, assignedFeesLen),
					FormatField(Str.DailyInterestRate, dailyInterestRateLen),
					FormatField(Str.CurrentBalance, currentBalanceLen),
					FormatField(Str.RepaidPrincipal, repaidPrincipalLen),
					FormatField(Str.RepaidInterest, repaidInterestLen),
					FormatField(Str.RepaidFees, repaidFeesLen),
					FormatField(Str.IgnoredDay, ignoredDayLen),
					FormatField(note, notesLen)
				)
			);
		} // ToFormattedString

		public class FormattedData {
			internal FormattedData(OneDayLoanStatus odls) { this.odls = odls; }

			public string Date { get { return this.odls.Date.DateStr(); } }
			public string OpenPrincipalForInterest { get { return this.odls.OpenPrincipalForInterest.ToString("C8", Culture); } }
			public string OpenPrincipalAfterRepayments { get { return this.odls.OpenPrincipalAfterRepayments.ToString("C8", Culture); } }
			public string DailyInterest { get { return this.odls.DailyInterest.ToString("C8", Culture); } }
			public string AssignedFees { get { return this.odls.AssignedFees.ToString("C8", Culture); } }
			public string DailyInterestRate { get { return this.odls.DailyInterestRate.ToString("P8", Culture); } }
			public string CurrentBalance { get { return this.odls.CurrentBalance.ToString("C8", Culture); } }
			public string RepaidPrincipal { get { return this.odls.RepaidPrincipal.ToString("C8", Culture); } }
			public string RepaidInterest { get { return this.odls.RepaidInterest.ToString("C8", Culture); } }
			public string RepaidFees { get { return this.odls.RepaidFees.ToString("C8", Culture); } }
			public string IgnoredDay { get { return this.odls.IsBetweenLastPaymentAndReschedulingDay ? "yes" : "no"; } }

			private readonly OneDayLoanStatus odls;
		} // class FormattedData

		public FormattedData Str { get; private set; }

		public bool IsReschedulingDay { get; set; }

		public bool IsBetweenLastPaymentAndReschedulingDay { get; set; }

		public bool IsPaymentDay {
			get { return (RepaidPrincipal > 0) || (RepaidInterest > 0); }
		} // IsPaymentDay

		private static CultureInfo Culture {
			get { return Extensions.Library.Instance.Culture; }
		} // Culture

		private DateTime date;
		private readonly OneDayLoanStatus previousDay;
	} // class OneDayLoanStatus
} // namespace
