namespace Ezbob.HmrcHarvester {
	using System;
	using System.Collections.Generic;
	using Ezbob.Logger;

	public class VatReturnSeeds : ISeeds {
		#region public

		#region enum Field

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

		#endregion enum Field

		#region constructor

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

		#endregion constructor

		#region property FatalErrors

		public List<string> FatalErrors { get; private set; }

		#endregion property FatalErrors

		#region property FatalError

		public string FatalError {
			get {
				if (FatalErrors.Count < 1)
					return string.Empty;

				return string.Join(" ", FatalErrors);
			} // get
		} // FatalError

		#endregion property FatalError

		#region VAT period related

		#region property Period

		public string Period {
			get { return Get(Field.Period); }
			set { Set(Field.Period, value); }
		} // Period

		#endregion property Period

		#region property DateFrom

		public DateTime DateFrom {
			get { return Get(Field.DateFrom); }
			set { Set(Field.DateFrom, value); }
		} // DateFrom

		#endregion property DateFrom

		#region property DateTo

		public DateTime DateTo {
			get { return Get(Field.DateTo); }
			set { Set(Field.DateTo, value); }
		} // DateTo

		#endregion property DateTo

		#region property DateDue

		public DateTime DateDue {
			get { return Get(Field.DateDue); }
			set { Set(Field.DateDue, value); }
		} // DateDue

		#endregion property DateDue

		#region method IsPeriodValid

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

		#endregion method IsPeriodValid

		#endregion VAT period related

		#region Business details related

		#region property RegistrationNo

		public long RegistrationNo {
			get { return Get(Field.RegistrationNo); }
			set { Set(Field.RegistrationNo, value); }
		} // RegistrationNo

		#endregion property RegistrationNo

		#region property BusinessName

		public string BusinessName {
			get { return Get(Field.BusinessName); }
			set { Set(Field.BusinessName, value); }
		} // BusinessName

		#endregion property BusinessName

		#region property BusinessAddress

		public string[] BusinessAddress {
			get { return Get(Field.BusinessAddress); }
			set { Set(Field.BusinessAddress, value); }
		} // BusinessAddress

		#endregion property BusinessAddress

		#region method AreBusinessDetailsValid

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

		#endregion method AreBusinessDetailsValid

		#endregion Business details related

		#region property ReturnDetails

		public SortedDictionary<string, Coin> ReturnDetails {
			get { return m_oProperties[Field.ReturnDetails]; }
			private set { m_oProperties[Field.ReturnDetails] = value; }
		} // ReturnDetails

		#endregion property ReturnDetails

		#region method Set

		public void Set(Field f, dynamic oValue, ASafeLog oLog = null) {
			m_oProperties[f] = oValue;

			if (oLog != null)
				oLog.Debug("VatReturnSeeds.{0} := {1}", f.ToString(), oValue.ToString());
		} // Set

		#endregion method Set

		#region method Get

		public dynamic Get(Field f) {
			return m_oProperties[f];
		} // Get

		#endregion method Get

		#endregion public

		#region private

		#region method AddFatalError

		private void AddFatalError(string sError) {
			FatalErrors.Add(sError);
			m_oLog.Debug(sError);
		} // AddFatalError

		#endregion method AddFatalError

		private readonly SortedDictionary<Field, dynamic> m_oProperties;

		private readonly ASafeLog m_oLog;

		private static readonly DateTime ms_oLongTimeAgo = new DateTime(1976, 7, 1, 9, 30, 0, DateTimeKind.Utc);

		#endregion private
	} // class VatReturnSeeds
} // namespace Ezbob.HmrcHarvester
