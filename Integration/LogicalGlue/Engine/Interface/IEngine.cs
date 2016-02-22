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
		/// <param name="monthlyPayment">Customer monthly payment. Ignored if non-positive, i.e. searches for
		/// any value of monthly payment.
		/// </param>
		/// <param name="isTryout">If true, requests marked as tryout are included into search;
		/// not included, otherwise.</param>
		/// <param name="mode">Data retrieve mode, refer to <see cref="GetInferenceMode"/> for details.</param>
		/// <remarks>This method returns NULL if there is no matched RESPONSE.</remarks>
		/// <returns>Customer inference results. Can be NULL.</returns>
		Inference GetInference(int customerID, decimal monthlyPayment, bool isTryout, GetInferenceMode mode);

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// Logical Glue API is not queried.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="time">Time of interest.</param>
		/// <param name="includeTryOuts">Include try out data or not.</param>
		/// <param name="monthlyPayment">Customer monthly payment. Ignored if is non-positive, i.e. searches for
		/// any value of monthly payment.</param>
		/// <remarks>This method returns NULL if there is no matched RESPONSE.</remarks>
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

		/// <summary>
		/// Changes request "is try out" status.
		/// </summary>
		/// <param name="requestID">Request unique id.</param>
		/// <param name="isTryOut">New "is try out" status.</param>
		/// <returns>True, if request was found and updated; false, otherwise.</returns>
		bool SetRequestIsTryOut(Guid requestID, bool isTryOut);

		/// <summary>
		/// Loads monthly repayment for customer at specific time (requested loan / requested term + open loans).
		/// </summary>
		/// <param name="customerID">Customer to load data for.</param>
		/// <param name="now">Current (specific time).</param>
		/// <returns>Customer monthly repayment.</returns>
		decimal GetMonthlyRepayment(int customerID, DateTime now);

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
		Inference GetInferenceIfExists(int customerID, DateTime time, bool includeTryOutData, decimal monthlyPayment);

		/// <summary>
		/// Loads monthly repayment for customer at specific time (requested loan / requested term + open loans).
		/// </summary>
		/// <param name="customerID">Customer to load data for.</param>
		/// <param name="now">Current (specific time).</param>
		/// <returns>Customer monthly repayment.</returns>
		MonthlyRepaymentData GetMonthlyRepaymentData(int customerID, DateTime now);
	} // interface IEngine
} // namespace
