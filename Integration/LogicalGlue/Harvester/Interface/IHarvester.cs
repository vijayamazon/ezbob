namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	public interface IHarvester {
		Response<Reply> Infer(InferenceInput inputData);
	} // interface ILogicalGlueHarvester
} // namespace
