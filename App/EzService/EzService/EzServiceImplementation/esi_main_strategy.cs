namespace EzService.EzServiceImplementation {
	using System;
	using System.Linq;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MainStrategy;
	using Ezbob.Database;


	partial class EzServiceImplementation {
		public ActionMetaData MainStrategy1(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison) {
			var onfail = new Action<ActionMetaData>(
				amd => this.DB.ExecuteNonQuery(
					"UpdateMainStratFinishDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", customerId),
					new QueryParameter("Now", DateTime.UtcNow)
					)
				);

			return Execute(new ExecuteArguments(customerId, newCreditLine, avoidAutoDescison, null) {
				CustomerID = customerId,
				UserID = underwriterId,
				StrategyType = typeof(MainStrategy),
				OnException = onfail,
				OnFail = onfail,
			});
		} // MainStrategy1

		public ActionMetaData MainStrategySync1(int underwriterId, int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison) {
			var onfail = new Action<ActionMetaData>(
				amd => this.DB.ExecuteNonQuery(
					"UpdateMainStratFinishDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", customerId),
					new QueryParameter("Now", DateTime.UtcNow)
					)
				);

			return ExecuteSync<MainStrategy>(new ExecuteArguments(customerId, newCreditLine, avoidAutoDescison, null) {
				CustomerID = customerId,
				UserID = underwriterId,
				OnException = onfail,
				OnFail = onfail,
			});
		} // MainStrategySync1

	/*	/*
		 * elina
		 * DON"T DELETE, should be completed for  QualifyCustomer ezbob API service		

	/*	public AvailableCreditActionResult RequalifyCustomer(string customerEmail, NewCreditLineOption newCreditLine, int avoidAutoDescison) {

			int customerID = DB.ExecuteScalar<int>("GetCustomerIdByEmail", CommandSpecies.StoredProcedure, new QueryParameter("CustomerEmail", customerEmail));

			if (customerID < 0) {
				Log.Debug("Failed to retrieve customerID for email requested: {0}", customerEmail);
				return null;
			}
			// 1. create cash request
			// 2. run main strategy
			try {
				var onfail = new Action<ActionMetaData>(
					amd => this.DB.ExecuteNonQuery(
						"UpdateMainStratFinishDate",
						CommandSpecies.StoredProcedure,
						new QueryParameter("CustomerID", customerID),
						new QueryParameter("Now", DateTime.UtcNow)
						)
					);

				MainStrategy strategy;

				var metaData = ExecuteSync(out strategy, new ExecuteArguments(customerID, newCreditLine, avoidAutoDescison, null) {
					CustomerID = customerID,
					UserID = null,
					OnException = onfail,
					OnFail = onfail,
				});

				return new AvailableCreditActionResult {
					MetaData = metaData,
					Email = customerEmail,
					Decision = strategy.AutoDecisionResponse
				};

			} catch (Exception e) {

				Log.Debug("Failed to run main strategy for GetAvalableCreditByCustomerEmail, customerID {0}, email requested: {1}, error: {2}", customerID, customerEmail, e.Message);
				return null;
			}

		} */// RequalifyCustomer

	} // class EzServiceImplementation

} // namespace EzService
