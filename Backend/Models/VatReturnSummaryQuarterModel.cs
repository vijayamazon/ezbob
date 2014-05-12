namespace Ezbob.Backend.Models {
	using System;
	using System.Globalization;
	using System.Runtime.Serialization;
	using System.Text;

	[DataContract]
	public class VatReturnQuarter : VatReturnSummaryBase {
		[DataMember]
		public DateTime DateFrom { get; set; }

		[DataMember]
		public DateTime DateTo { get; set; }

		#region method ToString

		public override string ToString() {
			return ToString("");
		} // ToString

		public virtual string ToString(string sPrefix) {
			sPrefix = sPrefix ?? "";

			var os = new StringBuilder();

			os.AppendFormat(
				"{0} - {1}",
				DateFrom.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture),
				DateTo.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)
			);

			ToString(os, sPrefix);

			return os.ToString();
		} // ToString

		#endregion method ToString
	} // class VatReturnQuarter
} // namespace Ezbob.Backend.Models
