namespace Ezbob.HmrcHarvester {
	using System;
	using System.Collections.Generic;
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
			m_oLog = oLog ?? new SafeLog();

			m_oProperties = new SortedDictionary<Field, dynamic>();

			Period = null;
			DateFrom = ms_oLongTimeAgo;
			DateTo = ms_oLongTimeAgo;
			DateDue = ms_oLongTimeAgo;

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

			if (DateFrom.Equals(ms_oLongTimeAgo)) {
				bIsValid = false;
				AddFatalError("VAT return 'Date From' not specified.");
			} // if

			if (DateTo.Equals(ms_oLongTimeAgo)) {
				bIsValid = false;
				AddFatalError("VAT return 'Date To' not specified.");
			} // if

			if (DateDue.Equals(ms_oLongTimeAgo)) {
				bIsValid = false;
				AddFatalError("VAT return 'Date Due' not specified.");
			} // if

			return bIsValid;
		} // IsPeriodValid

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
			}

			if (string.IsNullOrWhiteSpace(BusinessName)) {
				bIsValid = false;
				AddFatalError("VAT return 'Company Name' not specified.");
			}

			if (BusinessAddress == null) {
				bIsValid = false;
				AddFatalError("VAT return 'Company Address' not specified.");
			}
			else if (BusinessAddress.Length < 1) {
				bIsValid = false;
				AddFatalError("VAT return 'Company Address' not specified.");
			}

			return bIsValid;
		} // AreBusinessDetailsValid

		public SortedDictionary<string, Coin> ReturnDetails {
			get { return m_oProperties[Field.ReturnDetails]; }
			private set { m_oProperties[Field.ReturnDetails] = value; }
		} // ReturnDetails

		public void Set(Field f, dynamic oValue, ASafeLog oLog = null) {
			m_oProperties[f] = oValue;

			if (oLog != null)
				oLog.Debug("VatReturnSeeds.{0} := {1}", f.ToString(), oValue.ToString());
		} // Set

		public dynamic Get(Field f) {
			return m_oProperties[f];
		} // Get

		private void AddFatalError(string sError) {
			FatalErrors.Add(sError);
			m_oLog.Debug(sError);
		} // AddFatalError

		private readonly SortedDictionary<Field, dynamic> m_oProperties;

		private readonly ASafeLog m_oLog;

		private static readonly DateTime ms_oLongTimeAgo = new DateTime(1976, 7, 1, 9, 30, 0, DateTimeKind.Utc);

	} // class VatReturnSeeds
} // namespace Ezbob.HmrcHarvester
