namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	public interface IHarvester {
		Response<Reply> Infer(InferenceInput inputData, HarvesterConfiguration configuration);
	} // interface ILogicalGlueHarvester
} // namespace
