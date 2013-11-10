using System.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee {
	public class YodleeParentBankModel
	{
		public string parentBankName { get; set; }
		public List<YodleeSubBankModel> subBanks { get; set; }
	}

	public class YodleeSubBankModel
	{
		public long csId { get; set; }
		public string displayName { get; set; }
	}

	public class YodleeBanksModel
	{
		public List<YodleeParentBankModel> ImageBanks;
		public List<YodleeSubBankModel> DropDownBanks;
	}
} // namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
