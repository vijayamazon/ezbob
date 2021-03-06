﻿namespace Ezbob.HmrcHarvester {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Logger;

	public class VatReturnSeeds : ISeeds {
		public enum Field {
			Period,
			DateFrom,
			DateTo,
			DateDue,
			RegistrationNo,
			BusinessName,
			BusinessAddress,
			ReturnDetails,
		} // enum Field

		public VatReturnSeeds(ASafeLog oLog) {
			this.log = oLog.Safe();

			this.properties = new SortedDictionary<Field, dynamic>();

			Period = null;
			DateFrom = longTimeAgo;
			DateTo = longTimeAgo;
			DateDue = longTimeAgo;

			RegistrationNo = 0;
			BusinessName = null;
			BusinessAddress = null;

			ReturnDetails = new SortedDictionary<string, Coin>();

			FatalErrors = new List<string>();
		} // constructor

		public List<string> FatalErrors { get; private set; }

		public string FatalError {
			get {
				if (FatalErrors.Count < 1)
					return string.Empty;

				return string.Join(" ", FatalErrors);
			} // get
		} // FatalError

		public string Period {
			get { return Get(Field.Period); }
			set { Set(Field.Period, value); }
		} // Period

		public DateTime DateFrom {
			get { return Get(Field.DateFrom); }
			set { Set(Field.DateFrom, value); }
		} // DateFrom

		public DateTime DateTo {
			get { return Get(Field.DateTo); }
			set { Set(Field.DateTo, value); }
		} // DateTo

		public DateTime DateDue {
			get { return Get(Field.DateDue); }
			set { Set(Field.DateDue, value); }
		} // DateDue

		public bool IsPeriodValid() {
			bool bIsValid = true;

			if (string.IsNullOrWhiteSpace(Period)) {
				bIsValid = false;
				AddFatalError("VAT return 'Period' not specified.");
			} // if

			InterpolateDatesFromPeriod();

			if (DateFrom <= longTimeAgo) {
				bIsValid = false;
				AddFatalError("VAT return 'Date From' not specified.");
			} // if

			if (DateTo <= longTimeAgo) {
				bIsValid = false;
				AddFatalError("VAT return 'Date To' not specified.");
			} // if

			if (DateDue <= longTimeAgo) {
				bIsValid = false;
				AddFatalError("VAT return 'Date Due' not specified.");
			} // if

			return bIsValid;
		} // IsPeriodValid

		/// <summary>
		/// Interpolate dates from period if needed & possible.
		/// </summary>
		private void InterpolateDatesFromPeriod() {
			if ((DateFrom > longTimeAgo) && (DateTo > longTimeAgo)) {
				this.log.Debug("Not interpolating dates from period: dates have already been set.");
				return;
			} // if

			string[] monthYear = Period.Split(' ');

			if (monthYear.Length != 2) {
				this.log.Debug("Not interpolating dates from period: period '{0}' is not in expected format.", Period);
				return;
			} // if

			while (monthYear[0].StartsWith("0"))
				monthYear[0] = monthYear[0].Substring(1);

			while (monthYear[1].StartsWith("0"))
				monthYear[1] = monthYear[1].Substring(1);

			int month;
			int year;

			if (!int.TryParse(monthYear[0], out month)) {
				this.log.Debug(
					"Not interpolating dates from period: period's month '{0}' is not in expected format.",
					monthYear[0]
				);
				return;
			} // if

			if ((month < 1) || (month > 12)) {
				this.log.Debug(
					"Not interpolating dates from period: period's month '{0}' is not in range 1..12.",
					month
				);

				return;
			} // if

			if (!int.TryParse(monthYear[1], out year)) {
				this.log.Debug(
					"Not interpolating dates from period: period's year '{0}' is not in expected format.",
					monthYear[1]
				);
				return;
			} // if

			// 29 is a standard (at least in windows) cutoff year.
			year += (year <= 29) ? 2000 : 1900;

			DateFrom = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-2).Date;
			DateTo = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddSeconds(-1).Date;
			DateDue = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(2).AddDays(7).Date;

			this.log.Debug(
				"Dates have been interpolated from period:\n\tperiod: {0}\n\tfrom: {1}\n\tto: {2}\n\tdue: {3}",
				Period,
				DateFrom.ToString("M d yyyy H:mm:ss", CultureInfo.InvariantCulture),
				DateTo.ToString("M d yyyy H:mm:ss", CultureInfo.InvariantCulture),
				DateDue.ToString("M d yyyy H:mm:ss", CultureInfo.InvariantCulture)
			);
		} // InterpolateDatesFromPeriod

		public long RegistrationNo {
			get { return Get(Field.RegistrationNo); }
			set { Set(Field.RegistrationNo, value); }
		} // RegistrationNo

		public string BusinessName {
			get { return Get(Field.BusinessName); }
			set { Set(Field.BusinessName, value); }
		} // BusinessName

		public string[] BusinessAddress {
			get { return Get(Field.BusinessAddress); }
			set { Set(Field.BusinessAddress, value); }
		} // BusinessAddress

		public bool AreBusinessDetailsValid() {
			bool bIsValid = true;

			if (RegistrationNo <= 0) {
				bIsValid = false;
				AddFatalError("VAT return 'Company Registration #' not specified.");
			} // if

			if (string.IsNullOrWhiteSpace(BusinessName)) {
				bIsValid = false;
				AddFatalError("VAT return 'Company Name' not specified.");
			} // if

			if (BusinessAddress == null) {
				bIsValid = false;
				AddFatalError("VAT return 'Company Address' not specified.");
			} else if (BusinessAddress.Length < 1) {
				bIsValid = false;
				AddFatalError("VAT return 'Company Address' not specified.");
			} // if

			return bIsValid;
		} // AreBusinessDetailsValid

		public SortedDictionary<string, Coin> ReturnDetails {
			get { return this.properties[Field.ReturnDetails]; }
			private set { this.properties[Field.ReturnDetails] = value; }
		} // ReturnDetails

		public void Set(Field f, dynamic oValue, ASafeLog oLog = null) {
			this.properties[f] = oValue;

			if (oLog != null)
				oLog.Debug("VatReturnSeeds.{0} := {1}", f, (oValue == null) ? "-- null --" : oValue.ToString());
		} // Set

		public dynamic Get(Field f) {
			return this.properties[f];
		} // Get

		private void AddFatalError(string sError) {
			FatalErrors.Add(sError);
			this.log.Debug(sError);
		} // AddFatalError

		private readonly SortedDictionary<Field, dynamic> properties;

		private readonly ASafeLog log;

		private static readonly DateTime longTimeAgo = new DateTime(1976, 7, 1, 9, 30, 0, DateTimeKind.Utc);
	} // class VatReturnSeeds
} // namespace Ezbob.HmrcHarvester
