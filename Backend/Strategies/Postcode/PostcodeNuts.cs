namespace Ezbob.Backend.Strategies.Postcode {
	using System;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using RestSharp;

	public class PostcodeNuts : AStrategy {

		public PostcodeNuts(string postcode) {
			this.postcode = postcode;
		} // constructor

		public override string Name {
			get { return "PostcodeNuts"; }
		} // Name

		public override void Execute() {
			try {
				if (CheckDBCache()) {
					Log.Info("postcodeNuts for postcode {0} already exists", this.postcode);
					ExistsInCash = true;
					return;
				}

				IRestClient client = new RestClient("https://api.postcodes.io/");
				IRestRequest request = new RestRequest {
					Method = Method.GET,
					Resource = "postcodes/" + this.postcode,
				};
				var response = client.Get<PostcodeNutsModel>(request);

				if (response == null || response.Data == null || response.Data.result == null || response.Data.status != 200 || response.Data.result.codes == null) {
					Log.Error("postcodeNuts failed to retrieve for postcode {0}", this.postcode);
					return;
				}

				Result = response.Data;
				SaveToDB(response.Data);
			} catch (Exception ex) {
				Log.Error(ex, "Failed to retrieve and save postcode nuts for postcode {0}", this.postcode);
			}
		}// Execute

		private bool CheckDBCache() {
			return DB.ExecuteScalar<bool>("PostcodeNutsCheckExists", 
				CommandSpecies.StoredProcedure, 
				new QueryParameter("Postcode", this.postcode));
		}//CheckDBCache

		private void SaveToDB(PostcodeNutsModel data) {
			var dbModel = new PostcodeNutsDBModel {
				Postcode = data.result.postcode,
				AdminCounty = data.result.admin_county,
				AdminCountyCode = data.result.codes.admin_county,
				AdminDistrict = data.result.admin_district,
				AdminDistrictCode = data.result.codes.admin_district,
				AdminWard = data.result.codes.admin_ward,
				AdminWardCode = data.result.codes.admin_ward,
				Ccg = data.result.ccg,
				CcgCode = data.result.codes.ccg,
				Country = data.result.country,
				Eastings = data.result.eastings,
				EuropeanElectoralRegion = data.result.european_electoral_region,
				Incode = data.result.incode,
				Latitude = data.result.latitude,
				Longitude = data.result.longitude,
				Lsoa = data.result.lsoa,
				Msoa = data.result.msoa,
				Nhs_ha = data.result.nhs_ha,
				Northings = data.result.northings,
				Nuts = data.result.nuts,
				NutsCode = data.result.codes.nuts,
				Outcode = data.result.outcode,
				Parish = data.result.parish,
				ParishCode = data.result.codes.parish,
				ParliamentaryConstituency = data.result.parliamentary_constituency,
				PrimaryCareTrust = data.result.primary_care_trust,
				Quality = data.result.quality,
				Region = data.result.region,
				Status = data.status
			};

			var postcodeNutsID = DB.ExecuteScalar<int>("PostcodeNutsSave", CommandSpecies.StoredProcedure, 
				DB.CreateTableParameter<PostcodeNutsDBModel>("@Tbl", dbModel)
			);

			Log.Info("postcodeNuts was saved for postcode {0} id {1}", data.result.postcode, postcodeNutsID);
		}//SaveToDB

		private readonly string postcode;
		public PostcodeNutsModel Result { get; private set; }
		public bool ExistsInCash { get; private set; }

	} // class PostcodeNuts

	public class PostcodeNutsModel {
		public int status { get; set; }
		public PostcodeNutsResultModel result { get; set; }
	}//class PostcodeNutsModel

	public class PostcodeNutsResultModel {
		public string postcode { get; set; }
		public int quality { get; set; }
		public long eastings { get; set; }
		public long northings { get; set; }
		public string country { get; set; }
		public string nhs_ha { get; set; }
		public decimal longitude { get; set; }
		public decimal latitude { get; set; }
		public string parliamentary_constituency { get; set; }
		public string european_electoral_region { get; set; }
		public string primary_care_trust { get; set; }
		public string region { get; set; }
		public string lsoa { get; set; }
		public string msoa { get; set; }
		public string incode { get; set; }
		public string outcode { get; set; }
		public string admin_district { get; set; }
		public string parish { get; set; }
		public string admin_county { get; set; }
		public string admin_ward { get; set; }
		public string ccg { get; set; }
		public string nuts { get; set; }
		public PostcodeNutsResultCodesModel codes { get; set; }
	}//class PostcodeNutsResultModel

	public class PostcodeNutsResultCodesModel {
		public string admin_district { get; set; }
		public string admin_county { get; set; }
		public string admin_ward { get; set; }
		public string parish { get; set; }
		public string ccg { get; set; }
		public string nuts { get; set; }
	}//class PostcodeNutsResultCodesModel
} // namespace Ezbob.Backend.Strategies.Postcode
