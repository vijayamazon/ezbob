namespace Ezbob.Integration.LogicalGlue.Interface {
	using System.Collections.Generic;

	public interface IInferenceResult : IReadOnlyDictionary<RequestType, IModelOutput> {
	} // interface IInferenceResult
} // namespace
