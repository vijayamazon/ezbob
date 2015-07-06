namespace Ezbob.Backend.ModelsWithDB    {
	using System;
	using System.Runtime.Serialization;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.ModelsWithDB.Experian;

    [DataContract(IsReference = true)]
	public class WriteToLogPackage {
        public WriteToLogPackage(InputData input) {
            In = input;
	        Out = new OutputData();
        }

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
        [DataMember]
		public InputData In { get; private set; }
        
        [IgnoreDataMember]
		public OutputData Out { get; private set; }

        [DataContract]
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

            [DataMember]
			public string Request { get; private set; }
            [DataMember]
			public string Response { get; private set; }
            [DataMember]
			public ExperianServiceType ServiceType { get; private set; }
            [DataMember]
			public int CustomerID { get; private set; }
            [DataMember]
			public int? DirectorID { get; private set; }
            [DataMember]
			public string Firstname { get; private set; }
            [DataMember]
			public string Surname { get; private set; }
            [DataMember]
			public DateTime? DateOfBirth { get; private set; }
            [DataMember]
			public string PostCode { get; private set; }
            [DataMember]
			public string CompanyRefNum { get; private set; }

	        public override string ToString() {
		        return this.CustomerID + " " + this.ServiceType;
	        }
        } // class In


		public class OutputData {
			public MP_ServiceLog ServiceLog { get; set; }
			public ExperianLtd ExperianLtd { get; set; }
			public ExperianConsumerData ExperianConsumer { get; set; }
		} // class Out
	} // class WriteToLogPackage
} // namespace
