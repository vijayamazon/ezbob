namespace Ezbob.Integration.LogicalGlue.Interface {
	using System;
	using System.Collections.Generic;

	public interface IError : ICanBeEmpty {
		string Exception { get; set; }
		string ErrorCode { get; set; }
		Guid? Uuid { get; set; }

		List<IWarning> Warnings { get; set; }
		List<IEncodingFailure> EncodingFailures { get; set; }
		List<string> MissingColumns { get; set; }
	} // interface IError
} // namespace
