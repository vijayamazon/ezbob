namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;

	public interface IEngine {
		/// <summary>
		/// Gets the latest existing inference from DB or queries Logical Glue API for specified customer.
		/// Method behaviour depends on <see cref="mode" />.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="mode">Data retrieve mode, refer to <see cref="GetInferenceMode"/> for details.</param>
		/// <returns>Customer inference results. Can be NULL.</returns>
		Inference GetInference(int customerID, GetInferenceMode mode);

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// Logical Glue API is not queried.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="time">Time of interest.</param>
		/// <returns>The latest customer inference results that were available on requested time. Can be NULL.</returns>
		Inference GetInference(int customerID, DateTime time);
	} // interface IEngine
} // namespace
