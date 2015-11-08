namespace Ezbob.Integration.LogicalGlue.Interface {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;

	[DataContract]
	public class Error {
		[DataMember]
		public string Exception { get; set; }

		[DataMember]
		public string ErrorCode { get; set; }

		[DataMember]
		public Guid? Uuid { get; set; }

		[DataMember]
		public List<EncodingFailure> EncodingFailures {
			get {
				if (this.encodingFailures == null)
					this.encodingFailures = new List<EncodingFailure>();

				return this.encodingFailures;
			} // get
			set { this.encodingFailures = Utility.SetList(this.encodingFailures, value); }
		} // EncodingFailures

		[DataMember]
		public List<string> MissingColumns {
			get {
				if (this.missingColumns == null)
					this.missingColumns = new List<string>();

				return this.missingColumns;
			} // get
			set {
				if (this.missingColumns == null)
					this.missingColumns = new List<string>();
				else
					this.missingColumns.Clear();

				if (value != null)
					this.missingColumns.AddRange(value.Where(s => !string.IsNullOrWhiteSpace(s)));
			} // set
		} // MissingColumns

		public bool IsEmpty {
			get {
				return
					string.IsNullOrWhiteSpace(ErrorCode) &&
					(EncodingFailures.Count < 1) &&
					(MissingColumns.Count < 1);
			} // get
		} // IsEmpty

		private List<EncodingFailure> encodingFailures;
		private List<string> missingColumns;
	} // class Error

	public static class ErrorExt {
		public static Error CloneFrom(this Error target, Error source) {
			if (source == null)
				return new Error();

			target = target ?? new Error();

			target.Exception = source.Exception;
			target.ErrorCode = source.ErrorCode;
			target.Uuid = source.Uuid;
			target.EncodingFailures = Utility.SetList(target.EncodingFailures, source.EncodingFailures);
			target.MissingColumns = source.MissingColumns;

			return target;
		} // CloneFrom
	} // class ErrorExt
} // namespace
