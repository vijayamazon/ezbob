﻿namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Interface;
	using Ezbob.Logger;
	using log4net;

	class Keeper : IKeeper {
		public Keeper(AConnection db, ILog log) {
			this.db = db;
			this.log = new SafeILog(log);
		} // constructor

		/// <summary>
		/// Loads the latest customer inference results that were available on specific time.
		/// </summary>
		public Inference LoadInference(int customerID, DateTime time) {
			return new InferenceLoader(this.db, this.log, customerID, time).Execute().Result;
		} // LoadInference

		/// <summary>
		/// Saves inference output to the database.
		/// </summary>
		/// <param name="customerID">ID of customer to infer.</param>
		/// <param name="requestType">Request type (fuzzy logic, neural network).</param>
		/// <param name="response">Inference output (raw and parsed).</param>
		public void SaveInference(int customerID, RequestType requestType, Response<Reply> response) {
			var dbResponse = new DBTable.Response();
			dbResponse.RequestTypeID = (long)requestType;
		} // SaveInference

		private readonly AConnection db;
		private readonly ASafeLog log;
	} // class Keeper
} // namespace
