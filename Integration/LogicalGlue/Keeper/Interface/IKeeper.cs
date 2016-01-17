namespace Ezbob.Integration.LogicalGlue.Keeper.Interface {
	using System;
	using System.Collections.Generic;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;

	public interface IKeeper {
		/// <summary>
		/// Loads input data to send to Logical Glue API for customer at specific time.
		/// </summary>
		/// <param name="customerID">Customer to load data for.</param>
		/// <param name="now">Current (specific time).</param>
		/// <param name="loadMonthlyRepaymentOnly">Load only monthly repayment or full input data.</param>
		/// <returns>Customer input data for LG API.</returns>
		InferenceInputPackage LoadInputData(int customerID, DateTime now, bool loadMonthlyRepaymentOnly);

		/// <summary>
		/// Saves LG request into DB.
		/// </summary>
		/// <param name="customerID">ID of customer to save inference request for.</param>
		/// <param name="companyID">ID of company to save inference request for.</param>
		/// <param name="isTryOut">This request contains actual customer data (false), or
		/// it contains some fictional data just to see "what if".</param>
		/// <param name="request">Request to save.</param>
		/// <returns>ID of the request in MP_ServiceLog table.</returns>
		long SaveInferenceRequest(int customerID, int companyID, bool isTryOut, InferenceInput request);

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// </summary>
		/// <remarks>This method returns NULL if there is no matched RESPONSE.</remarks>
		/// <param name="customerID">ID of customer to load inference for.</param>
		/// <param name="time">Time of inference.</param>
		/// <param name="includeTryOutData">Include try out data or not.</param>
		/// <param name="monthlyPayment">Ignored if is non-positive.
		/// Otherwise only inferences with this amount are included.</param>
		/// <returns>Inference that was available for the requested customer at requested time. Can be null.</returns>
		Inference LoadInference(int customerID, DateTime time, bool includeTryOutData, decimal monthlyPayment);

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

		/// <summary>
		/// Loads all the inference results for customer that were available up to specific time.
		/// Number of returned results can be limited.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="time">Time of interest, results received after this time are ignored.</param>
		/// <param name="includeTryOuts">Include try out data or not.</param>
		/// <param name="maxHistoryLength">Limit returned list by this length. Ignored if null or non-positive.</param>
		/// <returns>List of customer inferences ordered from the newest to the oldest.</returns>
		List<Inference> LoadInferenceHistory(int customerID, DateTime time, bool includeTryOuts, int? maxHistoryLength);

		/// <summary>
		/// Changes request "is try out" status.
		/// </summary>
		/// <param name="requestID">Request unique id.</param>
		/// <param name="isTryOut">New "is try out" status.</param>
		/// <returns>True, if request was found and updated; false, otherwise.</returns>
		bool SetRequestIsTryOut(Guid requestID, bool isTryOut);

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// </summary>
		/// <remarks>This method returns NULL if there is no matched REQUEST.
		/// If request exists but there is no matching response, this method returns <see cref="Inference "/> instance with
		/// request data set but response fields remain empty.</remarks>
		/// <param name="customerID">ID of customer to load inference for.</param>
		/// <param name="time">Time of inference.</param>
		/// <param name="includeTryOutData">Include try out data or not.</param>
		/// <param name="monthlyPayment">Ignored if is non-positive.
		/// Otherwise only inferences with this amount are included.</param>
		/// <returns>Inference that was available for the requested customer at requested time. Can be null.</returns>
		Inference LoadInferenceIfExists(int customerID, DateTime time, bool includeTryOutData, decimal monthlyPayment);
	} // interface IKeeper
} // namespace
