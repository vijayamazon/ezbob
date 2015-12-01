namespace Ezbob.Backend.ModelsWithDB {
	using Ezbob.Utils.dbutils;

	public class PostcodeNutsDBModel {
		[PK(true)]
		public int PostcodeNutsID { get; set; }
		[Length(10)]
		public string Postcode { get; set; }
		[Length(200)]
		public string NutsCode { get; set; }
		[Length(200)]
		public string Nuts { get; set; }
		public int Quality { get; set; }
		public long Eastings { get; set; }
		public long Northings { get; set; }
		[Length(200)]
		public string Country { get; set; }
		[Length(200)]
		public string Nhs_ha { get; set; }
		public decimal Longitude { get; set; }
		public decimal Latitude { get; set; }
		[Length(200)]
		public string ParliamentaryConstituency { get; set; }
		[Length(200)]
		public string EuropeanElectoralRegion { get; set; }
		[Length(200)]
		public string PrimaryCareTrust { get; set; }
		[Length(200)]
		public string Region { get; set; }
		[Length(200)]
		public string Lsoa { get; set; }
		[Length(200)]
		public string Msoa { get; set; }
		[Length(10)]
		public string Incode { get; set; }
		[Length(10)]
		public string Outcode { get; set; }
		[Length(200)]
		public string AdminDistrict { get; set; }
		[Length(50)]
		public string AdminDistrictCode { get; set; }
		[Length(200)]
		public string Parish { get; set; }
		[Length(50)]
		public string ParishCode { get; set; }
		[Length(200)]
		public string AdminCounty { get; set; }
		[Length(50)]
		public string AdminCountyCode { get; set; }
		[Length(200)]
		public string AdminWard { get; set; }
		[Length(50)]
		public string AdminWardCode { get; set; }
		[Length(200)]
		public string Ccg { get; set; }
		[Length(50)]
		public string CcgCode { get; set; }
		public int Status { get; set; }
	}
}
