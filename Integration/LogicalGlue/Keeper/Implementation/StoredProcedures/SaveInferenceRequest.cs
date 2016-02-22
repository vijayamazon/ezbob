namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using Newtonsoft.Json;

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class SaveInferenceRequest : ACustomerTimeStoredProc {
		public SaveInferenceRequest(
			int customerID,
			int companyID,
			bool isTryOut,
			InferenceInput request,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			this.request = request;
			CustomerID = customerID;
			CompanyID = companyID;
			IsTryOut = isTryOut;
			Now = DateTime.UtcNow;
		} // constructor

		public override bool HasValidParameters() {
			return base.HasValidParameters() && (this.request != null) && (CompanyID > 0);
		} // HasValidParameters

		public int CompanyID { get; set; }

		public bool IsTryOut { get; set; }

		public Guid UniqueID {
			get { return this.request.UniqueID; }
			set { }
		} // UniqueID 

		public string RequestText {
			get { return new Encrypted(JsonConvert.SerializeObject(this.request)).ToString(); }
			set { }
		} // RequestText

		public string EquifaxData {
			get {
				return string.IsNullOrWhiteSpace(this.request.EquifaxData)
					? null
					: new Encrypted(this.request.EquifaxData).ToString();
			} // get
			set { }
		} // EquifaxData

		public decimal? MonthlyPayment {
			get { return this.request.MonthlyPayment; }
			set { }
		} // MonthlyPayment

		public string CompanyRegistrationNumber {
			get { return this.request.CompanyRegistrationNumber; }
			set { }
		} // CompanyRegistrationNumber

		public string FirstName {
			get { return this.request.Director.FirstName; }
			set { }
		} // FirstName

		public string LastName {
			get { return this.request.Director.LastName; }
			set { }
		} // LastName

		public DateTime DateOfBirth {
			get { return this.request.Director.DateOfBirth; }
			set { }
		} // DateOfBirth

		public string Postcode {
			get { return this.request.Director.Postcode; }
			set { }
		} // Postcode

		public string HouseNumber {
			get { return this.request.Director.HouseNumber; }
			set { }
		} // HouseNumber

		private readonly InferenceInput request;
	} // class SaveInferenceRequest
} // namespace
