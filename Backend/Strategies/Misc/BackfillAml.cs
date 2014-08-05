namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using System.Data;
	using ExperianLib.IdIdentityHub;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BackfillAml : AStrategy
	{
		private readonly IdHubService idHubService = new IdHubService();

		public BackfillAml(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		}

		public override string Name {
			get { return "BackfillAml"; }
		}

		public override void Execute()
		{
			DataTable entriesDataTable = DB.ExecuteReader("GetEntriesForAmlBackfill", CommandSpecies.StoredProcedure);
			
			foreach (DataRow row in entriesDataTable.Rows)
			{
				var entriesSafeReader = new SafeReader(row);
				int customerId = entriesSafeReader["CustomerId"];
				long serviceLogId = entriesSafeReader["Id"];

				try
				{
					DataTable DataDataTable = DB.ExecuteReader("GetDataForAmlBackfill", 
															   CommandSpecies.StoredProcedure,
					                                           new QueryParameter("CustomerId", customerId),
					                                           new QueryParameter("ServiceLogId", serviceLogId));

					if (DataDataTable.Rows.Count == 1)
					{
						Log.Debug("Backfilling aml for service log: {0} customer {1}", serviceLogId, customerId);

						var dataSafeReader = new SafeReader(DataDataTable.Rows[0]);
						
						DateTime insertDate = dataSafeReader["InsertDate"];
						string xml = dataSafeReader["ResponseData"];
						string firstName = dataSafeReader["FirstName"];
						string middleName = dataSafeReader["MiddleName"];
						string surname = dataSafeReader["Surname"];
						string postcode = dataSafeReader["Postcode"];
						string existingAmlResult = dataSafeReader["ExistingAmlResult"];
						bool isLastEntryForCustomer = dataSafeReader["IsLastEntryForCustomer"];

						string key = string.Format("{0}_{1}_{2}_{3}", firstName, middleName, surname, postcode);
						AuthenticationResults result = idHubService.ParseAndSave_ForBackfill(customerId, xml, serviceLogId, insertDate, key);
						
						int authentication = result.AuthenticationIndex;
						string description = result.AuthIndexText;

						if (isLastEntryForCustomer)
						{
							// Update customer table
							DB.ExecuteNonQuery("UpdateAmlResult", CommandSpecies.StoredProcedure,
											   new QueryParameter("CustomerId", customerId),
											   new QueryParameter("AmlResult", existingAmlResult),
											   new QueryParameter("AmlScore", authentication),
											   new QueryParameter("AmlDescription", description)
								);
						}
						else
						{
							// Update IsActive to 0 (The save always saves it as active)
							DB.ExecuteNonQuery("SetAmlResultInactive", CommandSpecies.StoredProcedure,
											   new QueryParameter("LookupKey", key),
											   new QueryParameter("ServiceLogId", serviceLogId)
								);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Error("The backfill for customer:{0} failed with exception:{1}", customerId, ex);
				}
			}
		}
	}
}
