namespace Ezbob.Backend.ModelsWithDB.CompanyScore {
	using System.Collections.Generic;

	public class CompanyScoreModelItem {
		public CompanyScoreModelItem(string sGroupName, SortedDictionary<string, string> oMetaData) {
			GroupName = sGroupName;
			MetaData = oMetaData ?? new SortedDictionary<string, string>();
			Data = new List<CompanyScoreModelItemValues>();
		} // constructor

		public string GroupName { get; private set; }

		public SortedDictionary<string, string> MetaData { get; private set; }

		public List<CompanyScoreModelItemValues> Data { get; private set; }
	} // CompanyScoreModelItem

} // namespace
