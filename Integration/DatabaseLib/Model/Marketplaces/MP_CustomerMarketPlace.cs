using System;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database {
    
    public class MP_CustomerMarketPlace 
	{
        public MP_CustomerMarketPlace() 
		{
			PayPalTransactions = new HashedSet<MP_PayPalTransaction>();
			EbayOrders = new HashedSet<MP_EbayOrder>();
			AmazonOrders = new HashedSet<MP_AmazonOrder>();
			Inventory = new HashedSet<MP_EbayAmazonInventory>();
			UpdatingHistory = new HashedSet<MP_CustomerMarketplaceUpdatingHistory>();
			EbayUserData = new HashedSet<MP_EbayUserData>();
			EbayUserAccountData = new HashedSet<MP_EbayUserAccountData>();
			EbayFeedback = new HashedSet<MP_EbayFeedback>();
			AmazonFeedback = new HashedSet<MP_AmazonFeedback>();
			TeraPeakOrders = new HashedSet<MP_TeraPeakOrder>();
			AnalysysFunctionValues = new HashedSet<MP_AnalyisisFunctionValue>();
            EkmOrders = new HashedSet<MP_EkmOrder>();
            VolusionOrders = new HashedSet<MP_VolusionOrder>();
		}
        public virtual int Id { get; set; }
        public virtual MP_MarketplaceType Marketplace { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual byte[] SecurityData { get; set; }
        public virtual string DisplayName { get; set; }
		public virtual MP_PayPalPersonalInfo PersonalInfo { get; set; }

		public virtual DateTime? Created { get; set; }
		public virtual DateTime? Updated { get; set; }
		public virtual DateTime? UpdatingStart { get; set; }
		public virtual DateTime? UpdatingEnd { get; set; }
        public virtual string UpdateError { get; set; }

        public virtual bool EliminationPassed { get; set; }

    	public virtual ISet<MP_PayPalTransaction> PayPalTransactions { get; set; }
		public virtual ISet<MP_EbayOrder> EbayOrders { get; set; }
		public virtual ISet<MP_AmazonOrder> AmazonOrders { get; set; }
		public virtual ISet<MP_EbayAmazonInventory> Inventory { get; set; }
		public virtual ISet<MP_CustomerMarketplaceUpdatingHistory> UpdatingHistory { get; set; }
		public virtual ISet<MP_EbayUserData> EbayUserData { get; set; }
		public virtual ISet<MP_EbayUserAccountData> EbayUserAccountData { get; set; }
		public virtual ISet<MP_EbayFeedback> EbayFeedback { get; set; }
		public virtual ISet<MP_AmazonFeedback> AmazonFeedback { get; set; }
		public virtual ISet<MP_TeraPeakOrder> TeraPeakOrders { get; set; }
		public virtual ISet<MP_AnalyisisFunctionValue> AnalysysFunctionValues { get; set; }

        public virtual ISet<MP_EkmOrder> EkmOrders { get; set; }
        public virtual ISet<MP_VolusionOrder> VolusionOrders { get; set; }
        public virtual ISet<MP_PayPointOrder> PayPointOrders { get; set; }
    }
}
