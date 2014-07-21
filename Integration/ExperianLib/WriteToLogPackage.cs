namespace ExperianLib {
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.ModelsWithDB.Experian;

	public class WriteToLogPackage {
		public WriteToLogPackage(
			string sRequestText,
			string sResponseText,
			ExperianServiceType nServiceType,
			int nCustomerID,
			int? nDirectorID = null
		) {
			In = new InputData(sRequestText, sResponseText, nServiceType, nCustomerID, nDirectorID);
			Out = new OutputData();
		} // constructor

		public InputData In { get; private set; }

		public OutputData Out { get; private set; }

		public class InputData {
			public InputData(
				string sRequest,
				string sResponse,
				ExperianServiceType nServiceType,
				int nCustomerID,
				int? nDirectorID
			) {
				Request = sRequest;
				Response = sResponse;
				ServiceType = nServiceType;
				CustomerID = nCustomerID;
				DirectorID = nDirectorID;
			} // constructor

			public string Request { get; private set; }
			public string Response { get; private set; }
			public ExperianServiceType ServiceType { get; private set; }
			public int CustomerID { get; private set; }
			public int? DirectorID { get; private set; }
		} // class In

		public class OutputData {
			public MP_ServiceLog ServiceLog { get; set; }
			public ExperianLtd ExperianLtd { get; set; }
		} // class Out
	} // class WriteToLogPackage
} // namespace
