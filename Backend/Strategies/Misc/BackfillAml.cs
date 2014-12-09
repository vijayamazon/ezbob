namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using ExperianLib.IdIdentityHub;
	using Ezbob.Database;

	public class BackfillAml : AStrategy {
		private readonly IdHubService idHubService = new IdHubService();

		public override string Name {
			get { return "BackfillAml"; }
		}

		public override void Execute() {
			DB.ForEachRowSafe((entriesSafeReader, bRowsetStart) => {
				int customerId = entriesSafeReader["CustomerId"];
				long serviceLogId = entriesSafeReader["Id"];

				try {
					SafeReader dataSafeReader = DB.GetFirst("GetDataForAmlBackfill",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("ServiceLogId", serviceLogId));

					if (!dataSafeReader.IsEmpty) {
						Log.Debug("Backfilling aml for service log: {0} customer {1}", serviceLogId, customerId);

						DateTime insertDate = dataSafeReader["InsertDate"];
						string xml = dataSafeReader["ResponseData"];
						string firstName = dataSafeReader["FirstName"];
						string middleName = dataSafeReader["MiddleName"];
						string surname = dataSafeReader["Surname"];
						string postcode = dataSafeReader["Postcode"];
						string existingAmlResult = dataSafeReader["ExistingAmlResult"];
						bool isLastEntryForCustomer = dataSafeReader["IsLastEntryForCustomer"];

						string key = string.Format("{0}_{1}_{2}_{3}", firstName, middleName, surname, postcode);
						AuthenticationResults result = idHubService.ParseAndSave_ForBackfill(xml, serviceLogId, insertDate, key);

						int authentication = result.AuthenticationIndex;
						string description = result.AuthIndexText;

						if (isLastEntryForCustomer) {
							// Update customer table
							DB.ExecuteNonQuery("UpdateAmlResult", CommandSpecies.StoredProcedure,
								   new QueryParameter("CustomerId", customerId),
								   new QueryParameter("AmlResult", existingAmlResult),
								   new QueryParameter("AmlScore", authentication),
								   new QueryParameter("AmlDescription", description)
							);
						}
						else {
							// Update IsActive to 0 (The save always saves it as active)
							DB.ExecuteNonQuery("SetAmlResultInactive", CommandSpecies.StoredProcedure,
								   new QueryParameter("LookupKey", key),
								   new QueryParameter("ServiceLogId", serviceLogId)
							);
						}
					}
				}
				catch (Exception ex) {
					Log.Error("The backfill for customer:{0} failed with exception:{1}", customerId, ex);
				}

				return ActionResult.Continue;
			}, "GetEntriesForAmlBackfill", CommandSpecies.StoredProcedure);
		}
	}
}
