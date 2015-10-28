namespace Ezbob.Integration.LogicalGlue.Interface {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class Error {
		public string Exception { get; set; }
		public string ErrorCode { get; set; }
		public Guid? Uuid { get; set; }

		public List<Warning> Warnings{
			get {
				if (this.warnings == null)
					this.warnings = new List<Warning>();

				return this.warnings;
			} // get
			set { this.warnings = SetList(this.warnings, value); }
		} // Warnings

		public List<EncodingFailure> EncodingFailures {
			get {
				if (this.encodingFailures == null)
					this.encodingFailures = new List<EncodingFailure>();

				return this.encodingFailures;
			} // get
			set { this.encodingFailures = SetList(this.encodingFailures, value); }
		} // EncodingFailures

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
					(Warnings.Count < 1) &&
					(EncodingFailures.Count < 1) &&
					(MissingColumns.Count < 1);
			} // get
		} // IsEmpty

		internal static List<T> SetList<T>(List<T> target, IEnumerable<T> source) where T : class, ICanBeEmpty {
			if (ReferenceEquals(source, target))
				return target;

			if (target == null)
				target = new List<T>();

			target.Clear();

			if (source == null)
				return target;

			target.AddRange(source.Where(w => (w != null) && !w.IsEmpty));

			return target;
		} // SetList

		private List<Warning> warnings;
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

			target.Warnings = Error.SetList(target.Warnings, source.Warnings);
			target.EncodingFailures = Error.SetList(target.EncodingFailures, source.EncodingFailures);
			target.MissingColumns = source.MissingColumns;

			return target;
		} // CloneFrom
	} // class ErrorExt
} // namespace
