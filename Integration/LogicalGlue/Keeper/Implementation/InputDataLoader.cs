namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Exceptions.Keeper;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;
	using JetBrains.Annotations;
	using Newtonsoft.Json;

	class InputDataLoader : ATimedCustomerActionBase {
		public InputDataLoader(AConnection db, ASafeLog log, int customerID, DateTime now) : base(db, log, customerID, now) {
			Result = new InferenceInput();
		} // constructor

		public InputDataLoader Execute() {
			if (Executed) {
				Log.Alert("Input data loader({0}, '{1}') has already been executed.", CustomerID, NowStr);
				return this;
			} // if

			Executed = true;

			Log.Debug("Executing input data loader({0}, '{1}')...", CustomerID, NowStr);

			new LoadInputData(DB, Log) { CustomerID = CustomerID, Now = Now, }.ForEachRowSafe(ProcessInputDataRow);

			Log.Debug("Executing input data loader({0}, '{1}') complete.", CustomerID, NowStr);

			return this;
		} // Execute

		public InferenceInput Result { get; private set; }
		public int CompanyID { get; private set; }

		private void ProcessInputDataRow(SafeReader sr) {
			string rowTypeName = sr["RowType"];

			RowTypes rowType;

			if (!Enum.TryParse(rowTypeName, false, out rowType)) {
				throw new KeeperAlert(
					Log,
					"Input data loader({1}, '{2}'): unknown row type '{0}'.",
					rowTypeName,
					CustomerID,
					NowStr
				);
			} // if

			switch (rowType) {
			case RowTypes.CompanyRegistrationNumber:
				Result.CompanyRegistrationNumber = sr["CompanyNumber"];
				break;

			case RowTypes.RequestedLoan:
				decimal defaultAmount = sr["DefaultAmount"];
				if (defaultAmount <= 0)
					defaultAmount = 10000;

				int defaultTerm = sr["DefaultTerm"];
				if (defaultTerm <= 0)
					defaultTerm = 12;

				decimal amount = sr["Amount"];
				int term = sr["Term"];

				Result.MonthlyPayment = amount / (term > 0 ? term : defaultTerm);

				if (Result.MonthlyPayment.Value <= 0)
					Result.MonthlyPayment = defaultAmount / (term > 0 ? term : defaultTerm);

				break;

			case RowTypes.DirectorData:
				Result.Director.FirstName = sr["FirstName"];
				Result.Director.LastName = sr["Surname"];
				Result.Director.DateOfBirth = sr["DateOfBirth"];

				CompanyID = sr["CompanyID"];
				break;

			case RowTypes.EquifaxData:
				Reply reply = JsonConvert.DeserializeObject<Reply>(sr["ResponseData"]);
				Result.EquifaxData = reply.EquifaxData;
				break;

			default:
				throw OutOfRangeException(
					"Input data loader({1}, '{2}'): unsupported row type '{0}'.",
					rowTypeName,
					CustomerID,
					NowStr
				);
			} // switch
		} // ProcessInputDataRow

		[StringFormatMethod("format")]
		private KeeperAlert OutOfRangeException(string format, params object[] args) {
			return new KeeperAlert(Log, new ArgumentOutOfRangeException(), format, args);
		} // OutOfRangeException

		private enum RowTypes {
			CompanyRegistrationNumber,
			RequestedLoan,
			DirectorData,
			EquifaxData,
		} // enum RowType
	} // class InputDataLoader
} // namespace
