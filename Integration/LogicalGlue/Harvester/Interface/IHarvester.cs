namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	public interface IHarvester {
		Response<InferenceOutput> Infer(InferenceInput inputData);
	} // interface ILogicalGlueHarvester
} // namespace
