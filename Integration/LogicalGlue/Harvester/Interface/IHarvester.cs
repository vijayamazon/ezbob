namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	public interface IHarvester {
		Response<InferenceInput> GetInferenceInputData(EvaluateeDetails evaluateeDetails);
		Response<InferenceOutput> Infer(InferenceInput inputData);
	} // interface ILogicalGlueHarvester
} // namespace
