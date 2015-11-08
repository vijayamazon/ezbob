namespace Ezbob.Integration.LogicalGlue.Keeper {
	using System;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Interface;
	using Ezbob.Logger;

	internal class InferenceLoader {
		public InferenceLoader(DBKeeper keeper, int customerID, DateTime time) {
			this.db = keeper.DB;
			this.log = keeper.Log;
			this.customerID = customerID;
			this.time = time;

			Result = new Inference();
		} // constructor

		public Inference Result { get; private set; }

		public InferenceLoader Execute() {
			this.db.ForEachRowSafe(
				ProcessInferenceRow,
				"LogicalGlueLoadInference",
				new QueryParameter("@CustomerID", this.customerID),
				new QueryParameter("@Now", this.time)
			);

			return this;
		} // Execute

		private void ProcessInferenceRow(SafeReader sr) {
		} // ProcessInferenceRow

		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly int customerID;
		private readonly DateTime time;
	} // class InferenceLoader
} // namespace
