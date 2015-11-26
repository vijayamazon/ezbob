namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;

	public interface IEngine {
		/// <summary>
		/// Infers customer using current customer details.
		/// <para>Collects customer name, address, etc by customer ID and executes a call to Logical Glue API.</para>
		/// <para>LG output is stored in DB and returned to caller.</para>
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <returns>Customer inference from fuzzy logic and neural network models.</returns>
		Inference Infer(int customerID);

		/// <summary>
		/// Gets the latest existing inference from DB or queries Logical Glue API for specified customer.
		/// Method behaviour depending on <see cref="checkCacheOnly" /> and <see cref="forceRemoteRequest" />.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="forceRemoteRequest">If true, queries Logical Glue API, attempts to store results to DB, and
		/// returns the last existing inference from DB. If no new inference stored, returns the latest among previously
		/// exising.</param>
		/// <returns>Customer inference results.</returns>
		Inference GetInference(int customerID, bool forceRemoteRequest, bool checkCacheOnly);

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// Logical Glue API is not queried.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="time">Time of interest.</param>
		/// <returns>The latest customer inference results that were available on requested time.</returns>
		Inference GetHistoricalInference(int customerID, DateTime time);
	} // interface IEngine
} // namespace
