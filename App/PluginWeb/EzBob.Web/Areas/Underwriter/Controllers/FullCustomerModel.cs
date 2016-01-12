namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.ApplicationInfo;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Models;

	public class FullCustomerModel {
		public PersonalInfoModel PersonalInfoModel { get; set; }
		public ApplicationInfoModel ApplicationInfoModel { get; set; }
		public List<AffordabilityData> Affordability { get; set; }
		public CreditBureauModel CreditBureauModel { get; set; }
		public ProfileSummaryModel SummaryModel { get; set; }
		public MedalCalculators MedalCalculations { get; set; }
		public PropertiesModel Properties { get; set; }
		public CrmModel CustomerRelations { get; set; }
		public List<BugModel> Bugs { get; set; }
		public string State { get; set; }
		public CompanyScoreModel CompanyScore { get; set; }
		public List<string> ExperianDirectors { get; set; }
	} // class FullCustomerModel
}