namespace EzService.EzServiceImplementation {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MainStrategy;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	partial class EzServiceImplementation {
		public ActionMetaData MainStrategyAsync(
			int underwriterId,
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison,
			long? cashRequestID,
			CashRequestOriginator? cashRequestOriginator
		) {
			return Execute(PrepareMainStrategyArguments(
				underwriterId,
				customerId,
				newCreditLine,
				avoidAutoDescison,
				cashRequestID,
				cashRequestOriginator
			));
		} // MainStrategyAsync

		public ActionMetaData MainStrategySync(
			int underwriterId,
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison,
			long? cashRequestID,
			CashRequestOriginator? cashRequestOriginator
		) {
			return ExecuteSync<MainStrategy>(PrepareMainStrategyArguments(
				underwriterId,
				customerId,
				newCreditLine,
				avoidAutoDescison,
				cashRequestID,
				cashRequestOriginator
			));
		} // MainStrategySync

		private ExecuteArguments PrepareMainStrategyArguments(
			int underwriterId,
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison,
			long? cashRequestID,
			CashRequestOriginator? cashRequestOriginator
		) {
			var onfail = new Action<ActionMetaData>(
				amd => DB.ExecuteNonQuery(
					"UpdateMainStratFinishDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", customerId),
					new QueryParameter("Now", DateTime.UtcNow)
				)
			);

			return new ExecuteArguments(
				underwriterId,
				customerId,
				newCreditLine,
				avoidAutoDescison,
				null,
				cashRequestID,
				cashRequestOriginator
			) {
				CustomerID = customerId,
				UserID = underwriterId,
				StrategyType = typeof(MainStrategy),
				OnException = onfail,
				OnFail = onfail,
			};
		} // PrepareMainStrategyArguments
	} // class EzServiceImplementation
} // namespace EzService
