namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;

	[DataContract]
	public class ModelError {
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
	} // class ModelError

	public static class ModelErrorExt {
		public static ModelError CloneFrom(this ModelError target, ModelError source) {
			if (source == null)
				return new ModelError();

			target = target ?? new ModelError();

			target.Exception = source.Exception;
			target.ErrorCode = source.ErrorCode;
			target.Uuid = source.Uuid;
			target.EncodingFailures = Utility.SetList(target.EncodingFailures, source.EncodingFailures);
			target.MissingColumns = source.MissingColumns;

			return target;
		} // CloneFrom
	} // class ModelErrorExt
} // namespace
