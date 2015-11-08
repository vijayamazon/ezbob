namespace Ezbob.Integration.LogicalGlue.KeeperInterface {
	using System;
	using Ezbob.Integration.LogicalGlue.Interface;

	public interface IKeeper {
		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// </summary>
		Inference LoadInference(int customerID, DateTime time);
	} // interface IKeeper
} // namespace
