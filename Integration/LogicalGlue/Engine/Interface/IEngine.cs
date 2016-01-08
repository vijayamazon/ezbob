namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;

	public interface IEngine {
		/// <summary>
		/// Gets the latest existing inference (excluding try out data)
		/// from DB or queries Logical Glue API for specified customer.
		/// Method behaviour depends on <see cref="mode" />.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="mode">Data retrieve mode, refer to <see cref="GetInferenceMode"/> for details.</param>
		/// <returns>Customer inference results. Can be NULL.</returns>
		Inference GetInference(int customerID, GetInferenceMode mode);

		/// <summary>
		/// Gets the latest existing inference (including try out data)
		/// from DB or queries Logical Glue API for specified customer and monthly payment.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="monthlyPayment">Customer monthly payment. If non-positive just calls
		/// previous version of the method with requested customerID and GetInferenceMode.DownloadIfOld.
		/// </param>
		/// <returns>Customer inference results. Can be NULL.</returns>
		Inference GetInference(int customerID, decimal monthlyPayment);

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
	} // interface IEngine
} // namespace
