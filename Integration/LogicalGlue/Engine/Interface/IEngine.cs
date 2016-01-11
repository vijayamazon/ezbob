namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;
	using System.Collections.Generic;

	public interface IEngine {
		/// <summary>
		/// Gets the latest existing inference (including try out data)
		/// from DB or queries Logical Glue API for specified customer and monthly payment.
		/// Method behavior depends on <see cref="mode" />.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="monthlyPayment">Customer monthly payment. If non-positive just calls
		/// previous version of the method with requested customerID and GetInferenceMode.DownloadIfOld.
		/// </param>
		/// <param name="isTryout">if true marked as tryout</param>
		/// <param name="mode">Data retrieve mode, refer to <see cref="GetInferenceMode"/> for details.</param>
		/// <returns>Customer inference results. Can be NULL.</returns>
		Inference GetInference(int customerID, decimal monthlyPayment, bool isTryout, GetInferenceMode mode);

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// Logical Glue API is not queried.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="time">Time of interest.</param>
		/// <param name="includeTryOuts">Include try out data or not.</param>
		/// <param name="monthlyPayment">Customer monthly payment.
		/// Ignored if is non-positive or <see cref="includeTryOuts"/> is false.</param>
		/// <returns>The latest customer inference results that were available on requested time. Can be NULL.</returns>
		Inference GetInference(int customerID, DateTime time, bool includeTryOuts, decimal monthlyPayment);

		/// <summary>
		/// Loads all the inference results for customer that were available up to specific time.
		/// Logical Glue API is not queried.
		/// Number of returned results can be limited.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="time">Time of interest, results received after this time are ignored.</param>
		/// <param name="includeTryOuts">Include try out data or not.</param>
		/// <param name="maxHistoryLength">Limit returned list by this length. Ignored if null or non-positive.</param>
		/// <returns>List of customer inferences ordered from the newest to the oldest.</returns>
		List<Inference> GetInferenceHistory(int customerID, DateTime time, bool includeTryOuts, int? maxHistoryLength);
	} // interface IEngine
} // namespace
