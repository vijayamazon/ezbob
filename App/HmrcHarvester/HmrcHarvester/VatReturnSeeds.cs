using System;
using System.Globalization;
using System.Collections.Generic;
using Ezbob.Logger;

namespace Ezbob.HmrcHarvester {
	#region class VatReturnSeeds

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

		public VatReturnSeeds() {
			m_oProperties = new SortedDictionary<Field, dynamic>();

			Period = null;
			DateFrom = LongTimeAgo;
			DateTo = LongTimeAgo;
			DateDue = LongTimeAgo;

			RegistrationNo = 0;
			BusinessName = null;
			BusinessAddress = null;

			ReturnDetails = new SortedDictionary<string, decimal>();
		} // constructor

		#endregion constructor

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
			return
				! string.IsNullOrWhiteSpace(Period) &&
				! DateFrom.Equals(LongTimeAgo) &&
				! DateTo.Equals(LongTimeAgo) &&
				! DateDue.Equals(LongTimeAgo);
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
			return
				RegistrationNo > 0 &&
				!string.IsNullOrWhiteSpace(BusinessName) &&
				BusinessAddress != null &&
				BusinessAddress.Length > 0;
		} // AreBusinessDetailsValid

		#endregion method AreBusinessDetailsValid

		#endregion Business details related

		#region property ReturnDetails

		public SortedDictionary<string, decimal> ReturnDetails {
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

		#region protected
		#endregion protected

		#region private

		private readonly SortedDictionary<Field, dynamic> m_oProperties;

		private static readonly DateTime LongTimeAgo = new DateTime(1976, 7, 1, 9, 30, 0, DateTimeKind.Utc);

		#endregion private
	} // class VatReturnSeeds

	#endregion class VatReturnSeeds
} // namespace Ezbob.HmrcHarvester
