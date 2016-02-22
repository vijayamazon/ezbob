namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class MessagesModel {
		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public string RawFileName { get; set; }

		[DataMember]
		public DateTime RawCreationDate { get; set; }

		[DataMember]
		public bool IsOwn { get; set; }

		public string FileName {
			get {
				return string.IsNullOrWhiteSpace(RawFileName)
					? string.Format(
						"Askville({0}).docx",
						FormattingUtils.FormatDateTimeToStringWithoutSpaces(RawCreationDate)
					)
					: RawFileName;
			} // get
		} // FileNameStr

		public string CreationDate {
			get { return FormattingUtils.FormatDateTimeToString(RawCreationDate); }
		} // CreationDate
	} // class MessagesModel
} // namespace
