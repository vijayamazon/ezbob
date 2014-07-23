namespace Ezbob.Backend.ModelsWithDB.CompanyScore {
	using System.Collections.Generic;

	using CompanyScoreModelDataset = System.Collections.Generic.SortedDictionary<string, CompanyScore.CompanyScoreModelItem>;

	public class CompanyScoreModelItemValues {
		#region constructor

		public CompanyScoreModelItemValues() {
			Values = new SortedDictionary<string, string>();
			Children = new CompanyScoreModelDataset();
		} // constructor

		#endregion constructor

		public SortedDictionary<string, string> Values { get; private set; }
		public CompanyScoreModelDataset Children { get; private set; }
	} // class CompanyScoreModelItemValues
} // namespace
