namespace Ezbob.Integration.LogicalGlue.Interface {
	using System;

	public interface IProcessor {
		/// <summary>
		/// Infers customer using current customer details.
		/// <para>Collects customer name, address, etc by customer ID and executes three calls to Logical Glue API:
		/// <list type="number">
		/// <item><description>Gets inference input data by customer name, address, etc.</description></item>
		/// <item><description>With inference input data executes fuzzy logic model call.</description></item>
		/// <item><description>With inference input data executes neural network model call.</description></item>
		/// </list>
		/// Model outputs are stored in DB and returned to caller.
		/// </para>
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <returns>Customer inference from fuzzy logic and neural network models.</returns>
		Inference Infer(int customerID);

		/// <summary>
		/// Loads the latest customer inference results that are available now.
		/// Logical Glue API is not queried.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <returns>The latest customer inference results that are available now.</returns>
		Inference GetInference(int customerID);

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// Logical Glue API is not queried.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="time">Time of interest.</param>
		/// <returns>The latest customer inference results that were available on requested time.</returns>
		Inference GetHistoricalInference(int customerID, DateTime time);
	} // interface IProcessor
} // namespace
