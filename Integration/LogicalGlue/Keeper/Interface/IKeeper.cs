namespace Ezbob.Integration.LogicalGlue.Keeper.Interface {
	using System;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;

	public interface IKeeper {
		/// <summary>
		/// Loads input data to send to Logical Glue API for customer at specific time.
		/// </summary>
		/// <param name="customerID">Customer to load data for.</param>
		/// <param name="now">Current (specific time).</param>
		/// <returns>Customer input data for LG API.</returns>
		InferenceInput LoadInputData(int customerID, DateTime now);

		/// <summary>
		/// Saves LG request into DB.
		/// </summary>
		/// <param name="customerID">ID of customer to save inference request for.</param>
		/// <param name="request">Request to save.</param>
		/// <returns>ID of the request in MP_ServiceLog table.</returns>
		long SaveInferenceRequest(int customerID, InferenceInput request);

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// </summary>
		/// <param name="customerID">ID of customer to load inference for.</param>
		/// <param name="time">Time of inference.</param>
		/// <returns>Inference that was available for the requested customer at requested time. Can be null.</returns>
		Inference LoadInference(int customerID, DateTime time);

		/// <summary>
		/// Saves inference output to the database.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="requestID">ID of the request in MP_ServiceLog table.</param>
		/// <param name="response">Inference output (raw and parsed).</param>
		Inference SaveInference(int customerID, long requestID, Response<Reply> response);

		/// <summary>
		/// Loads module configuration from the database.
		/// </summary>
		/// <returns>Module configuration.</returns>
		ModuleConfiguration LoadModuleConfiguration();

		/// <summary>
		/// Loads harvester configuration from the database.
		/// </summary>
		/// <returns>Harvester configuration.</returns>
		HarvesterConfiguration LoadHarvesterConfiguration();
	} // interface IKeeper
} // namespace
