using System;
using System.Linq;
using System.Text;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	using Marketplaces.FreeAgent;

	public class MP_CustomerMarketplaceUpdatingHistory
	{
		public MP_CustomerMarketplaceUpdatingHistory()
		{
			AnalyisisFunctionValue = new HashedSet<MP_AnalyisisFunctionValue>();
			ActionLog = new HashedSet<MP_CustomerMarketplaceUpdatingActionLog>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime? UpdatingStart { get; set; }
		public virtual DateTime? UpdatingEnd { get; set; }

		public virtual MP_AmazonOrder AmazonOrder { get; set; }
		public virtual MP_AmazonFeedback AmazonFeedback { get; set; }
		public virtual ISet<MP_AnalyisisFunctionValue> AnalyisisFunctionValue { get; set; }
		public virtual ISet<MP_CustomerMarketplaceUpdatingActionLog> ActionLog { get; set; }
		public virtual MP_EbayAmazonInventory Inventory { get; set; }
		public virtual MP_EbayFeedback EbayFeedback { get; set; }
		public virtual MP_EbayOrder EbayOrder { get; set; }
		public virtual MP_TeraPeakOrder TeraPeakOrder { get; set; }
		public virtual MP_EbayUserAccountData EbayUserAccountData { get; set; }
		public virtual MP_EbayUserData EbayUserData { get; set; }
		public virtual MP_PayPalTransaction PayPalTransaction { get; set; }

		public virtual MP_EkmOrder EkmOrder { get; set; }
		public virtual MP_FreeAgentRequest FreeAgentRequest { get; set; }
        public virtual MP_VolusionOrder VolusionOrder { get; set; }
        public virtual MP_PlayOrder PlayOrder { get; set; }
        public virtual MP_PayPointOrder PayPointOrder { get; set; }
        public virtual MP_YodleeOrder YodleeOrder { get; set; }
		public virtual string Error { get; set; }
	}
}
