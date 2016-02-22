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

		[DataMember]
		public List<Warning> Warnings {
			get {
				if (this.warnings == null)
					this.warnings = new List<Warning>();

				return this.warnings;
			} // get
			set { this.warnings = Utility.SetList(this.warnings, value); }
		} // EncodingFailures

		public bool IsEmpty {
			get {
				return
					string.IsNullOrWhiteSpace(ErrorCode) &&
					string.IsNullOrWhiteSpace(Exception) &&
					((EncodingFailures.Count < 1) || EncodingFailures.All(ef => ef.IsEmpty)) &&
					(MissingColumns.Count < 1) &&
					((Warnings.Count < 1) || Warnings.All(ef => ef.IsEmpty))
					;
			} // get
		} // IsEmpty

		private List<EncodingFailure> encodingFailures;
		private List<string> missingColumns;
		private List<Warning> warnings;
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
			target.Warnings = Utility.SetList(target.Warnings, source.Warnings);

			return target;
		} // CloneFrom
	} // class ModelErrorExt
} // namespace
