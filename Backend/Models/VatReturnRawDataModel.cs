namespace Ezbob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Runtime.Serialization;
	using Utils;
	using ValueIntervals;

	[DataContract]
	public class VatReturnRawData : IComparable<VatReturnRawData> {

		public VatReturnRawData() {
			InternalID = Guid.NewGuid();
			Data = new SortedDictionary<string, Coin>();
		} // constructor

		public int CompareTo(VatReturnRawData a) {
			return DateFrom.CompareTo(a.DateFrom);
		} // CompareTo

		[DataMember]
		public DateTime DateFrom { get; set; }
		[DataMember]
		public DateTime DateTo { get; set; }
		[DataMember]
		public DateTime DateDue { get; set; }
		[DataMember]
		public string Period { get; set; }

		[DataMember]
		public long RegistrationNo { get; set; }
		[DataMember]
		public string BusinessName { get; set; }

		[DataMember]
		[NonTraversable]
		public string[] BusinessAddress { get; set; } // BusinessAddress

		public string Address {
			get { return BusinessAddress == null ? string.Empty : string.Join("\n", BusinessAddress); }
			set { BusinessAddress = (value ?? string.Empty).Split('\n'); }
		} // Address

		[DataMember]
		public int RecordID { get; set; }

		[DataMember]
		public int SourceID {
			get { return (int)SourceType; }
			set { SourceType = (VatReturnSourceType)value; }
		} // SourceID

		[NonTraversable]
		public VatReturnSourceType SourceType { get; set; }

		[DataMember]
		public bool IsDeleted { get; set; }

		[DataMember]
		public Guid InternalID { get; set; }

		[DataMember]
		[NonTraversable]
		public SortedDictionary<string, Coin> Data { get; set; }

		[NonTraversable]
		public DateInterval Interval {
			get {
				if (m_oInterval == null)
					m_oInterval = new DateInterval(DateFrom, DateTo);

				return m_oInterval;
			} // get
		} // Interval

		private DateInterval m_oInterval;

		public override string ToString() {
			return string.Format(
				"Record ID: {7}{8} internal id: {9}\n" +
				"{0}: {1} - {2} at {3}\n" +
				"{4}: {5} at {6}\n{10}",
				Period,
				DateFrom.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
				DateTo.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
				DateDue.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
				RegistrationNo,
				BusinessName,
				string.Join(" ", BusinessAddress),
				RecordID,
				IsDeleted ? " DELETED" : "",
				InternalID,
				string.Join("\n", Data.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value)))
			);
		} // ToString

		public bool Overlaps(VatReturnRawData oOther) {
			return !oOther.IsDeleted && (RegistrationNo == oOther.RegistrationNo) && Interval.Intersects(oOther.Interval);
		} // Overlaps

		public bool SameAs(VatReturnRawData oOther) {
			if (DateFrom != oOther.DateFrom)
				return false;

			if (DateTo != oOther.DateTo)
				return false;

			var oMyData = BoxData();

			if (oMyData == null)
				return false;

			var oOtherData = oOther.BoxData();

			if (oOtherData == null)
				return false;

			var oKeys = new SortedSet<int>();

			foreach (var i in oMyData.Keys)
				oKeys.Add(i);

			foreach (var i in oOtherData.Keys)
				oKeys.Add(i);

			foreach (var i in oKeys) {
				if (!oMyData.ContainsKey(i))
					return false;

				if (!oOtherData.ContainsKey(i))
					return false;

				if (!oMyData[i].Equals(oOtherData[i]))
					return false;
			} // for each key

			return true;
		} // SameAs

		private SortedDictionary<int, Coin> BoxData() {
			var oResult = new SortedDictionary<int, Coin>();

			foreach (var pair in Data) {
				int nBoxNum = VatReturnUtils.BoxNameToNum(pair.Key);

				if (nBoxNum > 0)
					oResult[nBoxNum] = pair.Value;
				else
					return null;
			} // for each

			return oResult;
		} // BoxData

	} // class VatReturnRawData
} // namespace
