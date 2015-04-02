namespace EzService.EzServiceImplementation {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MainStrategy;
	using Ezbob.Database;

	partial class EzServiceImplementation {
		public ActionMetaData MainStrategy1(
			int underwriterId,
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison,
			long? cashRequestID,
			MainStrategy.DoAction createCashRequest,
			MainStrategy.DoAction updateCashRequest
		) {
			var onfail = new Action<ActionMetaData>(
				amd => this.DB.ExecuteNonQuery(
					"UpdateMainStratFinishDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", customerId),
					new QueryParameter("Now", DateTime.UtcNow)
				)
			);

			return Execute(new ExecuteArguments(
				customerId,
				newCreditLine,
				avoidAutoDescison,
				null,
				cashRequestID,
				createCashRequest,
				updateCashRequest
			) {
				CustomerID = customerId,
				UserID = underwriterId,
				StrategyType = typeof(MainStrategy),
				OnException = onfail,
				OnFail = onfail,
			});
		} // MainStrategy1

		public ActionMetaData MainStrategySync1(
			int underwriterId,
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison,
			long? cashRequestID,
			MainStrategy.DoAction createCashRequest,
			MainStrategy.DoAction updateCashRequest
		) {
			var onfail = new Action<ActionMetaData>(
				amd => DB.ExecuteNonQuery(
					"UpdateMainStratFinishDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", customerId),
					new QueryParameter("Now", DateTime.UtcNow)
				)
			);

			return ExecuteSync<MainStrategy>(new ExecuteArguments(
				customerId,
				newCreditLine,
				avoidAutoDescison,
				null,
				cashRequestID,
				createCashRequest,
				updateCashRequest
			) {
				CustomerID = customerId,
				UserID = underwriterId,
				OnException = onfail,
				OnFail = onfail,
			});
		} // MainStrategySync1
	} // class EzServiceImplementation
} // namespace EzService
