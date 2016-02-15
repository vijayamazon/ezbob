namespace Ezbob.Backend.Strategies.LandRegistry {
	using System;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.Strategies.Extensions;
	using Ezbob.Database;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using LandRegistryLib;
	using LandRegistryLib.LREnquiryServiceNS;

	public class LandRegistryEnquiry : AStrategy {
		public static LandRegistryDataModel Get(
			int customerId,
			string buildingNumber,
			string buildingName,
			string streetName,
			string cityName,
			string postCode
		) {
			var lre = new LandRegistryEnquiry(
				customerId,
				buildingNumber,
				buildingName,
				streetName,
				cityName,
				postCode
			);

			lre.Execute();

			return lre.RawResult;
		} // Get

		public LandRegistryEnquiry(
			int customerId,
			string buildingNumber,
			string buildingName,
			string streetName,
			string cityName,
			string postCode
		) {
			this.customerID = customerId;
			this.buildingNumber = buildingNumber;
			this.buildingName = buildingName;
			this.streetName = streetName;
			this.cityName = cityName;
			this.postCode  = postCode;
		} // constructor

		public override string Name { get { return "Land Registry Enquiry"; } } // Name

		public string Result { get; set; }

		public LandRegistryDataModel RawResult { get; private set; }

		public override void Execute() {
			RawResult = GetLandRegistryEnquiryData();

			Result = new Serialized(RawResult);
		} // Execute

		private LandRegistryDataModel GetLandRegistryEnquiryData() {
			try {
				//check cache
				var landRegistryLoad = new LandRegistryLoad(this.customerID);
				landRegistryLoad.Execute();
				var customersLrs = landRegistryLoad.Result;
				var cache = customersLrs.Where(x => x.RequestTypeEnum == LandRegistryRequestType.Enquiry).ToList();

				if (cache.Any()) {
					foreach (var landRegistry in cache) {
						var lrReq = Serialized.Deserialize<LandRegistryLib.LREnquiryServiceNS.RequestSearchByPropertyDescriptionV2_0Type>(landRegistry.Request);
						Q1AddressType lrAddress = lrReq.Product.SubjectProperty.Address;

						if (lrAddress.IsA(this.buildingNumber, this.buildingName, this.streetName, this.cityName, this.postCode)) {
							var b = new LandRegistryModelBuilder();
							var cacheModel = new LandRegistryDataModel {
								Request = landRegistry.Request,
								Response = landRegistry.Response,
								Enquery = b.BuildEnquiryModel(landRegistry.Response),
								RequestType = landRegistry.RequestTypeEnum,
								ResponseType = landRegistry.ResponseTypeEnum,
								DataSource = LandRegistryDataSource.Cache,
							};
							return cacheModel;
						} // if
					} // for each
				} // if
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to retrieve land registry enquiry from cache.");
			} // try

			bool isProd = CurrentValues.Instance.LandRegistryProd;

			ILandRegistryApi lr;
			if (isProd) {
				lr = new LandRegistryApi(
					CurrentValues.Instance.LandRegistryUserName,
					Encrypted.Decrypt(CurrentValues.Instance.LandRegistryPassword),
					CurrentValues.Instance.LandRegistryFilePath);
			} else
				lr = new LandRegistryTestApi();

			var model = lr.EnquiryByPropertyDescription(this.buildingNumber, this.buildingName, this.streetName, this.cityName, this.postCode, this.customerID);


			var lrDB = new LandRegistryDB {
				CustomerId = this.customerID,
				InsertDate = DateTime.UtcNow,
				Postcode = string.IsNullOrEmpty(this.postCode) ? string.Format("{3}{0},{1},{2}", this.buildingNumber, this.streetName, this.cityName, this.buildingName) : this.postCode,
				Request = model.Request,
				Response = model.Response,
				RequestType = model.RequestType.ToString(),
				ResponseType = model.ResponseType.ToString()
			};
			int lrID = DB.ExecuteScalar<int>("LandRegistryDBSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", lrDB));
			Log.Info("LandRegistryDBSave {0}", lrID);
			model.DataSource = LandRegistryDataSource.Api;

			return model;
		} // GetLandRegistryEnquiryData

		private readonly int customerID;
		private readonly string buildingNumber;
		private readonly string buildingName;
		private readonly string streetName;
		private readonly string cityName;
		private readonly string postCode;
	} // LandRegistryEnquiry
} // namespace
