namespace Ezbob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Runtime.Serialization;
	using Utils;

	[DataContract]
	public class VatReturnRawData : ITraversable, IComparable<VatReturnRawData> {
		#region constructor

		public VatReturnRawData() {
			Data = new SortedDictionary<string, Coin>();
		} // constructor

		#endregion constructor

		#region method CompareTo

		public int CompareTo(VatReturnRawData a) {
			return DateFrom.CompareTo(a.DateFrom);
		} // CompareTo

		#endregion method CompareTo

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

		public bool IsDeleted { get; set; }

		[DataMember]
		[NonTraversable]
		public SortedDictionary<string, Coin> Data { get; set; }

		public override string ToString() {
			return string.Format(
				"Record ID: {7}{8}\n" +
				"{0}: {1} - {2} at {3}\n" +
				"{4}: {5} at {6}\n{9}",
				Period,
				DateFrom.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
				DateTo.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
				DateDue.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
				RegistrationNo,
				BusinessName,
				string.Join(" ", BusinessAddress),
				RecordID,
				IsDeleted ? " DELETED" : "",
				string.Join("\n", Data.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value)))
			);
		} // ToString
	} // class VatReturnRawData
} // namespace
