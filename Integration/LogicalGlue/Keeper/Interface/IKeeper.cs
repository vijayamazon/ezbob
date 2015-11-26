namespace Ezbob.Integration.LogicalGlue.Keeper.Interface {
	using System;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;

	public interface IKeeper {
		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// </summary>
		Inference LoadInference(int customerID, DateTime time);

		/// <summary>
		/// Saves inference output to the database.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="response">Inference output (raw and parsed).</param>
		void SaveInference(int customerID, Response<Reply> response);
	} // interface IKeeper
} // namespace
