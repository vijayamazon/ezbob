namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using LandRegistryLib;
	using LandRegistryLib.LREnquiryServiceNS;
	using StructureMap;

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
			var lrRepo = ObjectFactory.GetInstance<LandRegistryRepository>();

			try {
				//check cache
				var cache = lrRepo.GetAll()
					.Where(x => x.Customer.Id == customerID && x.RequestType == LandRegistryRequestType.Enquiry);

				if (cache.Any()) {
					foreach (var landRegistry in cache) {
						var lrReq = Serialized.Deserialize<LandRegistryLib.LREnquiryServiceNS.RequestSearchByPropertyDescriptionV2_0Type>(landRegistry.Request);
						Q1AddressType lrAddress = lrReq.Product.SubjectProperty.Address;

						if (lrAddress.IsA(buildingNumber, buildingName, streetName, cityName, postCode)) {
							var b = new LandRegistryModelBuilder();
							var cacheModel = new LandRegistryDataModel {
								Request = landRegistry.Request,
								Response = landRegistry.Response,
								Enquery = b.BuildEnquiryModel(landRegistry.Response),
								RequestType = landRegistry.RequestType,
								ResponseType = landRegistry.ResponseType,
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

			var model = lr.EnquiryByPropertyDescription(buildingNumber, buildingName, streetName, cityName, postCode, customerID);

			lrRepo.Save(new LandRegistry {
				Customer = ObjectFactory.GetInstance<CustomerRepository>().Get(customerID),
				InsertDate = DateTime.UtcNow,
				Postcode = string.IsNullOrEmpty(postCode) ? string.Format("{3}{0},{1},{2}", buildingNumber, streetName, cityName, buildingName) : postCode,
				Request = model.Request,
				Response = model.Response,
				RequestType = model.RequestType,
				ResponseType = model.ResponseType
			});

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
