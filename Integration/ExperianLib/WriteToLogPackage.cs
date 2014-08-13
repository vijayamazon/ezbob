namespace ExperianLib {
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.ModelsWithDB.Experian;

	public class WriteToLogPackage {
		public WriteToLogPackage(
			string sRequestText,
			string sResponseText,
			ExperianServiceType nServiceType,
			int nCustomerID,
			int? nDirectorID = null,
			string firstname = null,
			string surname = null,
			DateTime? dob = null,
			string postCode = null,
			string companyRefNum = null
		) {
			In = new InputData(sRequestText, sResponseText, nServiceType, nCustomerID, nDirectorID, firstname, surname, dob, postCode, companyRefNum);
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
				int? nDirectorID,
				string firstname = null,
				string surname = null,
				DateTime? dob = null,
				string postCode = null,
				string companyRefNum = null
			) {
				Request = sRequest;
				Response = sResponse;
				ServiceType = nServiceType;
				CustomerID = nCustomerID;
				DirectorID = nDirectorID;
				Firstname = firstname;
				Surname = surname;
				DateOfBirth = dob;
				PostCode = postCode;
				CompanyRefNum = companyRefNum;
			} // constructor

			public string Request { get; private set; }
			public string Response { get; private set; }
			public ExperianServiceType ServiceType { get; private set; }
			public int CustomerID { get; private set; }
			public int? DirectorID { get; private set; }
			public string Firstname { get; private set; }
			public string Surname { get; private set; }
			public DateTime? DateOfBirth { get; private set; }
			public string PostCode { get; private set; }
			public string CompanyRefNum { get; private set; }
		} // class In

		public class OutputData {
			public MP_ServiceLog ServiceLog { get; set; }
			public ExperianLtd ExperianLtd { get; set; }
			public ExperianConsumerData ExperianConsumer { get; set; }
		} // class Out
	} // class WriteToLogPackage
} // namespace
