namespace Ezbob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Runtime.Serialization;
	using System.Text;

	[DataContract]
	public class Esignature {
		[DataMember]
		public int CustomerID { get; set; }

		[DataMember]
		public long ID { get; set; }

		[DataMember]
		public DateTime SendDate { get; set; }

		[DataMember]
		public int TemplateID { get; set; }

		[DataMember]
		public string TemplateName { get; set; }

		[DataMember]
		public int StatusID { get; set; }

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public bool HasDocument { get; set; }

		[DataMember]
		public List<Esigner> Signers { get; set; }

		public override string ToString() {
			var os = new StringBuilder();

			os.AppendFormat(
				"customer {0}: {1} sent on {2} as {6} {7}, {3} {4}, {5} document\n",
				CustomerID, ID, SendDate.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture),
				Status, StatusID, HasDocument ? "with" : "without",
				TemplateName, TemplateID
			);

			foreach (var s in Signers)
				os.AppendFormat("signer {0}\n", s);

			return os.ToString();
		} // ToString
	} // class Esignature
} // namespace
